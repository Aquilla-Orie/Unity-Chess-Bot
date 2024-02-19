using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MoveRecorder : MonoBehaviour
{
    [SerializeField] private Dictionary<string, PieceType> pieceNotations = new Dictionary<string, PieceType>
    {
        {"B", PieceType.BISHOP},
        {"N", PieceType.KNIGHT},
        {"R", PieceType.CASTLE},
        {"Q", PieceType.QUEEN},
        {"K", PieceType.KING}
    };

    private List<string> gameData = new List<string>();

    [SerializeField] private GameObject _contentPanel;
    [SerializeField] private GameObject _textObject;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.M))
    //        ParseMoves(m);
    //}
    //Reads the piece move and records it

    private Piece _takenPiece;
    private string whiteMove;
    private string blackMove;
    private void OnEnable()
    {
        Piece.onPieceTaken += RecordTakenPiece;
        Piece.onMoveComplete += RecordCompletedMove;
    }

    private void OnDisable()
    {
        Piece.onPieceTaken -= RecordTakenPiece;
        Piece.onMoveComplete -= RecordCompletedMove;
    }
    private void RecordTakenPiece(Piece piece)
    {
        _takenPiece = piece;
        Debug.Log($"{_takenPiece} has been taken");
    }

    private void RecordCompletedMove(Piece piece, Tile fromTile)
    {
        TMP_Text text = null;
        string moveString = "";
        string pieceChar = "";
        if (piece.PieceType != PieceType.PAWN)
        {
            pieceChar = pieceNotations.FirstOrDefault(x => x.Value == piece.PieceType).Key;
        }

        moveString += pieceChar;
        if (_takenPiece != null)
        {
            Debug.Log($"Taken piece {_takenPiece}");
            if (piece.PieceType == PieceType.PAWN)
                moveString += fromTile.name[0];
            moveString += "x";
            _takenPiece = null;
        }
        else
        {
            Debug.Log($"No Taken piece");
        }
        moveString += piece.Tile.name;

        if (piece.IsSameColor(1))
        {
            text = Instantiate(_textObject, _contentPanel.transform).GetComponent<TMP_Text>();
            whiteMove = moveString;
            text.text = $"{gameData.Count + 1}. {whiteMove}";
            return;
        }

        blackMove = moveString;

        gameData.Add($"{whiteMove} {blackMove}");
        text = _contentPanel.transform.GetChild(_contentPanel.transform.childCount - 1).GetComponent<TMP_Text>();
        text.text = $"{gameData.Count}. {whiteMove} {blackMove}";
        Debug.Log($"Added game move {whiteMove} {blackMove}");
    }

    //Parse a string of recorded moves
    private void ParseMoves(KeyValuePair<string, List<string>> moves)
    {
        string moveName = moves.Key.ToString();
        List<string> moveList = moves.Value;

        ////Loop through the list and seperate into black and white moves
        //foreach (string move in moveList)
        //{
        //    var split = move.Split(' ');
        //    string whiteMove = split[0];
        //    string blackMove = split[1];

        //    //Process and execute moves
        //    ProcessWhiteMove(whiteMove);         
        //    ProcessBlackMove(blackMove);         
        //}
        StartCoroutine(ProcessMoves(moveList));
    }

    private IEnumerator ProcessMoves(List<string> moveList)
    {
        //Loop through the list and seperate into black and white moves
        foreach (string move in moveList)
        {
            var split = move.Split(' ');
            string whiteMove = split[0];
            string blackMove = split[1];

            //Process and execute moves
            yield return new WaitForSeconds(1);
            ProcessWhiteMove(whiteMove);
            yield return new WaitForSeconds(1);
            ProcessBlackMove(blackMove);
        }
    }

    public bool ProcessBlackMove(string blackMove)
    {
        if (char.IsLower(blackMove[0]))//Pawn move
        {
            ulong bPawns = BoardManager.Instance.GetPawnBoard(0);
            var blackPieces = BoardManager.Instance.ConvertBitBoardToPiece(bPawns);

            Tile t;
            ulong b;

            //Regular pawn move
            if (blackMove.Length == 2)
            {
                t = BoardManager.Instance.GetTileByName(blackMove);
                b = t.BitBoard;

                //Get the pawn that can move to that position and move it there
                foreach (var piece in blackPieces)
                {
                    var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);

                    if ((movePos & b) > 0)
                    {
                        return piece.MovePiece(piece.transform.position, t.transform.position);
                    }
                }
                return false;
            }
            //Pawn Takes ... dxe5
            string takeTile = $"{blackMove[2]}{blackMove[3]}";
            t = BoardManager.Instance.GetTileByName(takeTile);
            b = t.BitBoard;
            int file = (char)(blackMove[0] - 97);
            foreach (var piece in blackPieces)
            {
                if (!(piece.transform.position.x == file))
                    continue;

                var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);

                if ((movePos & b) > 0)
                {
                    piece.MovePiece(piece.transform.position, t.transform.position);
                    return true;
                }
            }
        }
        else
        {
            if (blackMove.Contains("-"))
            {
                Debug.Log("Can't castle at the moment");
                return false;
            }
            //Another piece
            bool isTake = blackMove[1] == 'x';
            string takeTile = isTake ? $"{blackMove[2]}{blackMove[3]}" : $"{blackMove[1]}{blackMove[2]}";

            ulong bPieces = BoardManager.Instance.GetPieceBoard(pieceNotations[blackMove[0].ToString()], 0);
            var blackPieces = BoardManager.Instance.ConvertBitBoardToPiece(bPieces);

            Tile t;
            ulong b;

            t = BoardManager.Instance.GetTileByName(takeTile);
            b = t.BitBoard;
            //int file = (char)(blackMove[0] - 97);
            foreach (var piece in blackPieces)
            {
                //if (!(piece.transform.position.x == file))
                //    continue;

                var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);

                if ((movePos & b) > 0)
                {
                    piece.MovePiece(piece.transform.position, t.transform.position);
                    return true;
                }
            }
        }
        return false;
    }

    public void ProcessWhiteMove(string whiteMove)
    {
        if (char.IsLower(whiteMove[0]))//Pawn move
        {
            //Get the pawn that can move to that position and move it there
            ulong wPawns = BoardManager.Instance.GetPawnBoard(1);
            var whitePieces = BoardManager.Instance.ConvertBitBoardToPiece(wPawns);

            Tile t;
            ulong b;

            //Regular pawn move
            if (whiteMove.Length == 2)
            {
                t = BoardManager.Instance.GetTileByName(whiteMove);
                b = t.BitBoard;
                foreach (var piece in whitePieces)
                {
                    var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);

                    if ((movePos & b) > 0)
                    {
                        piece.MovePiece(piece.transform.position, t.transform.position);
                        return;
                    }
                }
            }

            //Pawn Takes ... dxe5
            string takeTile = $"{whiteMove[2]}{whiteMove[3]}";
            t = BoardManager.Instance.GetTileByName(takeTile);
            b = t.BitBoard;
            int file = (char)(whiteMove[0] - 97);

            foreach (var piece in whitePieces)
            {
                if (!(piece.transform.position.x == file))
                    continue;

                var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);

                if ((movePos & b) > 0)
                {
                    piece.MovePiece(piece.transform.position, t.transform.position);
                }
            }
        }
        else
        {
            //Another piece
            bool isTake = whiteMove[1] == 'x';
            string takeTile = isTake ? $"{whiteMove[2]}{whiteMove[3]}" : $"{whiteMove[1]}{whiteMove[2]}";

            ulong wPieces = BoardManager.Instance.GetPieceBoard(pieceNotations[whiteMove[0].ToString()], 1);
            var whitePieces = BoardManager.Instance.ConvertBitBoardToPiece(wPieces);

            Tile t;
            ulong b;

            t = BoardManager.Instance.GetTileByName(takeTile);
            b = t.BitBoard;
            //int file = (char)(blackMove[0] - 97);
            foreach (var piece in whitePieces)
            {
                //if (!(piece.transform.position.x == file))
                //    continue;

                var movePos = MoveGenerator.GetMoveTiles(piece, piece.transform.position);

                if ((movePos & b) > 0)
                {
                    piece.MovePiece(piece.transform.position, t.transform.position);
                    return;
                }
            }
        }
    }
}
