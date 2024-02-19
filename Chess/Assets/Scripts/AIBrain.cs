using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AIBrain : MonoBehaviour
{
    [SerializeField] private Book _book;
    [SerializeField] private MoveRecorder _moveRecorder;

    private int _color;

    private ulong _whitePieceBoard;
    private ulong _blackPieceBoard;

    // The "evaluate" depth of minimax algorithm
    private int _depth = 4;

    /// Piece value
    private const int _pawnValue = 100;
    private const int _knightValue = 320;
    private const int _bishopValue = 330;
    private const int _rookValue = 500;
    private const int _queenValue = 900;
    private const int _kingValue = 20000;

    int _bookLength = 0;
    int _bookIndex = 0;
    List<string> _bookMove = new List<string>();
    System.Random r = new System.Random();
    public void Init(int color)
    {
        _color = color;
        RefreshBoards();
        _bookMove = _book._openingBook.Values.ToList()[r.Next(_book._openingBook.Values.Count)];
        _bookLength = _bookMove.Count;
    }

    public void MakeMove(bool again)
    {
        StartCoroutine(DelayMovement(again));
    }

    private IEnumerator DelayMovement(bool again)
    {
        RefreshBoards();

        yield return new WaitForSeconds(1);

        //Check if King in check
        bool kingChecked = MoveGenerator.IsKingChecked(0, out List<ulong> kingCheckedBoard);
        if (kingChecked)
        {
            ResolveCheck(kingCheckedBoard);
            yield break;
        }

        if (again)
        {
            MoveRandom();
            yield break;
        }

        if (TakeHangingPiece())
        {
            yield break;
        }

        if (MoveWithBook())
        {
            yield break;
        }

        MoveMiniMax();
    }

    private bool MoveWithBook()
    {
        Debug.Log($"Book index {_bookIndex}, book length {_bookLength}");
        if (_bookIndex < _bookLength)
        {
            var split = _bookMove[_bookIndex].Split(' ');
            string whiteMove = split[0];
            string blackMove = split[1];

            Debug.Log($"Book move {blackMove}");
            if (!_moveRecorder.ProcessBlackMove(blackMove))
            {
                Debug.Log("Error processing book move");
                return false;
            }

            _bookIndex++;
        }
        return true;
    }

    private void ResolveCheck(List<ulong> kingCheckedBoard)
    {
        Debug.Log("King is checked, resolving");
        //Get ai pieces attack board
        var blackAttackBoard = BoardManager.Instance.GetAttackSquares(0);
        var whiteAttackBoard = BoardManager.Instance.GetAttackSquares(1);
        List <Piece> piecesCheckingKing = new List<Piece>();
        //Get all the pieces putting the king in check
        foreach (var checkBoard in kingCheckedBoard)
        {
            piecesCheckingKing.Add(BoardManager.Instance.ConvertBitBoardToPiece(checkBoard)[0]);
        }

        if (piecesCheckingKing.Count == 1)
        {
            //Can either block, take checking piece, or evade with king
            var p = piecesCheckingKing[0];
            ulong piecesDefending = 0;
            ulong piecesIntercepting = 0;
            foreach (var piece in BoardManager.Instance.ConvertBitBoardToPiece(_blackPieceBoard))
            {
                bool def = (MoveGenerator.GetAttackTiles(piece, piece.transform.position) & p.BitBoard) > 0;
                piecesDefending |= def ? piece.BitBoard : 0;
            }
            
            if (piecesDefending > 0)
            {
                Debug.Log("A piece can defend the check");
                //If the piece is a hanging piece, take
                var allPTakes = BoardManager.Instance.ConvertBitBoardToPiece(piecesDefending);
                ulong pieceCheckingIsHanging = (whiteAttackBoard & p.BitBoard);
                if (pieceCheckingIsHanging == 0)
                {
                    Debug.Log("Checking piece is hanging");
                    var pTake = allPTakes[r.Next(allPTakes.Count - 1)];
                    pTake.MovePiece(pTake.transform.position, p.transform.position);
                }
                else
                {
                    Debug.Log("Checking piece isn't hanging, but try to take it either ways");
                    if (MoveKingToSafety()) return;

                    //It's a defended piece, take with the lowest value piece available
                    allPTakes.OrderBy(p => p.Score).Reverse().ToList();
                    allPTakes.Remove(allPTakes.Where(p => p.PieceType == PieceType.KING).FirstOrDefault());
                    if (allPTakes.Count > 0)
                    {
                        var pT = allPTakes[0];
                        Debug.Log($"Trying to take checking piece with {pT.name}");
                        pT.MovePiece(pT.transform.position, p.transform.position);
                    }

                    GameManager.Instance.NewGameState(GameState.CHECKMATE);
                    return;

                }
            }
            else
            {
                Debug.Log("Checking piece cannot be attacked");
                foreach (var piece in BoardManager.Instance.ConvertBitBoardToPiece(_blackPieceBoard))
                {
                    bool inter = (MoveGenerator.GetAttackTiles(piece, piece.transform.position) & (MoveGenerator.GetAttackTiles(p, p.transform.position))) > 0;
                    piecesIntercepting |= inter ? piece.BitBoard : 0;
                }
                if (piecesIntercepting > 0)
                {
                    Debug.Log("Checking piece can be intercepted");
                    var allPInter = BoardManager.Instance.ConvertBitBoardToPiece(piecesIntercepting);
                    allPInter.OrderBy(piece => piece.Score).Reverse();

                    var interceptionPiece = allPInter[0];
                    ulong interPoint = MoveGenerator.GetAttackTiles(interceptionPiece, interceptionPiece.transform.position) & (MoveGenerator.GetAttackTiles(p, p.transform.position));
                    Tile interceptionTile = BoardManager.Instance.ConvertBitBoardToTile(interPoint)[0];

                    if (interceptionPiece.MovePiece(interceptionPiece.transform.position, interceptionTile.transform.position)) return;
                }
                Debug.Log("Checking piece can't be intercepted");
                if (!MoveKingToSafety())
                {
                    GameManager.Instance.NewGameState(GameState.CHECKMATE);
                    return;
                }
            }
        }
        else
        {
            //Move the king away or resign
            if (UnityEngine.Random.Range(0, 5) >= 3)
            {
                MoveKingToSafety();
            }
            else
            {
                GameManager.Instance.NewGameState(GameState.CHECKMATE);
            }
        }
    }

    private bool MoveKingToSafety()
    {
        var kingPiece = BoardManager.Instance.ConvertBitBoardToPiece(BoardManager.Instance.GetKingBoard(0))[0];
        ulong kingMoves = MoveGenerator.GetMoveTiles(kingPiece, kingPiece.transform.position);

        if (kingMoves == 0)
        {
            Debug.Log("King has no where to go");
            //Black is checkmated
            return false;
        }
        var moveTiles = BoardManager.Instance.ConvertBitBoardToTile(kingMoves);
        var m = moveTiles[r.Next(moveTiles.Count - 1)];

        kingPiece.MovePiece(kingPiece.transform.position, m.transform.position);
        return true;
    }

    private void MoveMiniMax()
    {
        ulong pawnBoard = BoardManager.Instance.GetPawnBoard(0) | BoardManager.Instance.GetPawnBoard(1);
        ulong rookBoard = BoardManager.Instance.GetRookBoard(0) | BoardManager.Instance.GetRookBoard(1);
        ulong knightBoard = BoardManager.Instance.GetKnightBoard(0) | BoardManager.Instance.GetKnightBoard(1);
        ulong bishopBoard = BoardManager.Instance.GetBishopBoard(0) | BoardManager.Instance.GetBishopBoard(1);
        ulong queenBoard = BoardManager.Instance.GetQueenBoard(0) | BoardManager.Instance.GetQueenBoard(1);
        ulong kingBoard = BoardManager.Instance.GetKingBoard(0) | BoardManager.Instance.GetKingBoard(1);
        ulong blackBoard = BoardManager.Instance.GetBlackBoard() | BoardManager.Instance.GetBlackBoard();
        ulong whiteBoard = BoardManager.Instance.GetWhiteBoard() | BoardManager.Instance.GetWhiteBoard();
        FauxBoard board = new FauxBoard( pawnBoard, rookBoard, knightBoard, bishopBoard, queenBoard, kingBoard, blackBoard, whiteBoard);
        var bestMove = GetBestMove(board);

        var bestPiece = bestMove.Key;
        var bestTile = bestMove.Value;
        var ti = BoardManager.Instance.ConvertBitBoardToTile(bestTile)[0];

        Debug.Log($"Best move is {bestPiece.Color} {bestPiece.PieceType} {bestPiece.Tile.Name} to {ti.name}");

        foreach (var piece in BoardManager.Instance.ConvertBitBoardToPiece(_blackPieceBoard))
        {
            var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);
            //Debug.Log($"Move Pos for {piece.GetColor()} {piece.PieceType} {piece.Tile.name} is {BoardManager.Instance.ConvertUInt64ToBinary(movePos)}");
            if ((movePos & bestTile) > 0 && piece.PieceType == bestPiece.PieceType && piece.IsSameColor(bestPiece.Color))
            {
                var t = BoardManager.Instance.ConvertBitBoardToTile(movePos)[0];
                
                piece.MovePiece(piece.transform.position, t.transform.position);
                return;
            }
        }
    }

    private void MoveRandom()
    {
        //Get all the physical pieces
        var pieces = BoardManager.Instance.ConvertBitBoardToPiece(_blackPieceBoard);
        List<Piece> movables = new List<Piece>();
        var whiteAttackBoard = BoardManager.Instance.GetAttackSquares(1);
        var blackAttackBoard = BoardManager.Instance.GetAttackSquares(0);

        if (TakeHangingPiece())
        {
            return;
        }


        //Pick a random piece and move it
        foreach (var piece in pieces)
        {
            if (MoveGenerator.GetMoveTiles(piece, piece.transform.position) > 0)
            {
                movables.Add(piece);
            }
        }
        System.Random rand = new System.Random();
        var p = movables[rand.Next(movables.Count())];
        var pieceMoveTiles = BoardManager.Instance.ConvertBitBoardToTile(MoveGenerator.GetMoveTiles(p, p.transform.position));

        var t = pieceMoveTiles[rand.Next(pieceMoveTiles.Count())];

        p.MovePiece(p.transform.position, t.transform.position);
    }

    private bool TakeHangingPiece()
    {
        //Get all the physical pieces
        var pieces = BoardManager.Instance.ConvertBitBoardToPiece(_blackPieceBoard);
        List<Piece> movables = new List<Piece>();
        var whiteAttackBoard = BoardManager.Instance.GetAttackSquares(1);
        var blackAttackBoard = BoardManager.Instance.GetAttackSquares(0);

        //Check for hanging pieces
        //If the piece is a hanging piece, take
        List<Piece> whitePieces = BoardManager.Instance.ConvertBitBoardToPiece(_whitePieceBoard);
        List<Piece> whiteHangingPieces = new List<Piece>();
        foreach (var whitePiece in whitePieces)
        {
            ulong pieceCheckingIsHanging = (whiteAttackBoard & whitePiece.BitBoard);
            ulong canTakeHangingPiece = (blackAttackBoard & whitePiece.BitBoard);
            if (pieceCheckingIsHanging == 0 && canTakeHangingPiece > 0)
            {
                Debug.LogWarning($"added hanging piece {whitePiece.name}");
                whiteHangingPieces.Add(whitePiece);
            }
        }
        if (whiteHangingPieces.Count > 0)
        {
            whiteHangingPieces.OrderBy(x => x.Score);//Get the most valuable hanging piece and take
            Debug.LogWarning($"most valuable hanging piece {whiteHangingPieces[0].name}");
            List<Piece> piecesDefending = new List<Piece>();
            foreach (var piece in pieces)
            {
                bool def = (MoveGenerator.GetAttackTiles(piece, piece.transform.position) & whiteHangingPieces[0].BitBoard) > 0;
                if (def)
                {
                    piecesDefending.Add(piece);
                    Debug.LogWarning($"{piece.name} can take hanging piece {whiteHangingPieces[0].name}");
                }
            }
            piecesDefending.OrderBy(x => x.Score).Reverse().ToList();
            if (piecesDefending.Count > 0)
            {
                Piece take = piecesDefending[0];
                Debug.LogWarning($"Using {take.name} to take hanging piece {whiteHangingPieces[0].name}");
                if (take.MovePiece(take.transform.position, whiteHangingPieces[0].transform.position))
                    return true;
            }
        }
        return false;
    }

    public int EvaluatePosition(FauxBoard board, int depth, int alpha, int beta, bool IsMaximizingPlayer)
    {
        if (depth == 0)
            return board.Evaluate();

        if (IsMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var move in board.GetPossibleMoves(0))
            {
                FauxPiece piece = move.Key;
                List<FauxTile> tiles = board.ConvertBitBoardToFauxTile(move.Value);
                foreach (var tile in tiles)
                {
                    FauxBoard newBoard = board.MovePiece(piece, tile);
                    int eval = EvaluatePosition(newBoard, depth - 1, alpha, beta, false);
                    board.UndoMove();
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var move in board.GetPossibleMoves(1))
            {
                FauxPiece piece = move.Key;
                List<FauxTile> tiles = board.ConvertBitBoardToFauxTile(move.Value);
                foreach (var tile in tiles)
                {
                    FauxBoard newBoard = board.MovePiece(piece, tile);
                    int eval = EvaluatePosition(newBoard, depth - 1, alpha, beta, true);
                    board.UndoMove();
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
            }
            return minEval;
        }
    }

    public KeyValuePair<FauxPiece, ulong> GetBestMove(FauxBoard board)
    {
        FauxPiece pieceWithBestMove = null;
        ulong bestMove = 0;
        int bestValue = int.MinValue;
        bool turn;
        int colorTurn;
        if (GameManager.State == GameState.BLACKTURN)
        {
            colorTurn = 0;
            turn = false;
        }
        else
        {
            colorTurn = 1;
            turn = true;
        }

        var possibleMoves = board.GetPossibleMoves(colorTurn);
        //OrderMoves(possibleMoves, board);
        foreach (var move in possibleMoves)
        {
            FauxPiece p = move.Key;
            List<FauxTile> moveTiles = board.ConvertBitBoardToFauxTile(move.Value);
            foreach (var tile in moveTiles)
            {
                FauxBoard newBoard = board.MovePiece(p, tile);
                //Debug.Log($"Getting minimax for Tile {tile.Name} with board {BoardManager.Instance.ConvertUInt64ToBinary(newBoard)} from {BoardManager.Instance.ConvertUInt64ToBinary(p.BitBoard)}");
                int value = EvaluatePosition(newBoard, _depth, int.MinValue, int.MaxValue, turn);

                if (value >= bestValue)
                {
                    bestValue = value;
                    bestMove = tile.BitBoard;
                    pieceWithBestMove = p;
                }

                //board = BoardManager.Instance.RestoreBoard();
                //Debug.LogWarning($"main Reloaded board {BoardManager.Instance.ConvertUInt64ToBinary(board)}");
            }
        }

        //board = BoardManager.Instance.RestoreBoard();
        Debug.Log($"Best Value is {bestValue}");
        return new KeyValuePair<FauxPiece, ulong>(pieceWithBestMove, bestMove);
    }

    private void RefreshBoards()
    {
        _whitePieceBoard = BoardManager.Instance.GetWhiteBoard();
        _blackPieceBoard = BoardManager.Instance.GetBlackBoard();
    }


}
