using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FauxTile
{
    public FauxPiece Piece
    {
        get
        {
            return _piece;
        }
        set
        {
            _piece = value;
        }
    }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public string StringBitBoard { get; set; }
    public ulong BitBoard
    {
        get
        {
            return _bitBoard;
        }
        set
        {
            _bitBoard = value;
            StringBitBoard = Convert.ToString((long)value, 2).PadLeft(64, '0');
        }
    }

    private ulong _bitBoard;
    [SerializeField] private FauxPiece _piece;
}
public class FauxBoard
{
    private const int HEIGHT = 8;
    private const int WIDTH = 8;
    private FauxTile[,] _tiles = new FauxTile[WIDTH, HEIGHT];
    private List<FauxPiece> _blackPieces = new List<FauxPiece>();
    private List<FauxPiece> _whitePieces = new List<FauxPiece>();

    private FauxPiece _movedPiece;
    private FauxPiece _takenPiece;
    private FauxTile _fromTile;

    private bool _hasMadeMove;
    public FauxBoard(ulong pawnBoard, ulong rookBoard, ulong knightBoard, ulong bishopBoard, ulong queenBoard, ulong kingBoard, ulong blackBoard, ulong whiteBoard)
    {
        _hasMadeMove = false;
        string binary;
        //GenerateFalseTiles()
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                binary = new string('0', 64);
                binary = binary.Substring(((i * WIDTH) + j) + 1) + "1" + binary.Substring(0, ((i * WIDTH) + j));
                FauxTile tile = new FauxTile();
                _tiles[j, i] = tile;
                //tile.name = $"Tile {row}{col}({rank},{file})";
                tile.Name = $"{(char)(97 + j)}{(char)(49 + i)}";
                tile.BitBoard = BoardManager.Instance.ConvertStringToBitBoard(binary);
                tile.Position = new Vector2 (i, j );
            }
        }

        #region SetupPieces
        //Pawns
        var blackPawnTiles = ConvertBitBoardToFauxTile(pawnBoard & blackBoard);
        var whitePawnTiles = ConvertBitBoardToFauxTile(pawnBoard & whiteBoard);
        foreach (var tile in blackPawnTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.PAWN, 0, tile);
            piece.Score = 100;
            _blackPieces.Add(piece);
        }
        foreach (var tile in whitePawnTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.PAWN, 1, tile);
            piece.Score = 100;
            _whitePieces.Add(piece);
        }
        //bishops
        var blackBishopTiles = ConvertBitBoardToFauxTile(bishopBoard & blackBoard);
        var whiteBishopTiles = ConvertBitBoardToFauxTile(bishopBoard & whiteBoard);
        foreach (var tile in blackBishopTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.BISHOP, 0, tile);
            piece.Score = 330;
            _blackPieces.Add(piece);
        }
        foreach (var tile in whiteBishopTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.BISHOP, 1, tile);
            piece.Score = 330;
            _whitePieces.Add(piece);
        }
        //Rooks
        var blackRookTiles = ConvertBitBoardToFauxTile(rookBoard & blackBoard);
        var whiteRookTiles = ConvertBitBoardToFauxTile(rookBoard & whiteBoard);
        foreach (var tile in blackRookTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.CASTLE, 0, tile);
            piece.Score = 500;
            _blackPieces.Add(piece);
        }
        foreach (var tile in whiteRookTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.CASTLE, 1, tile);
            piece.Score = 500;
            _whitePieces.Add(piece);
        }
        //Knights
        var blackKnightTiles = ConvertBitBoardToFauxTile(knightBoard & blackBoard);
        var whiteKnightTiles = ConvertBitBoardToFauxTile(knightBoard & whiteBoard);
        foreach (var tile in blackKnightTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.KNIGHT, 0, tile);
            piece.Score = 320;
            _blackPieces.Add(piece);
        }
        foreach (var tile in whiteKnightTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.KNIGHT, 1, tile);
            piece.Score = 320;
            _whitePieces.Add(piece);
        }
        //Queen
        var blackQueenTiles = ConvertBitBoardToFauxTile(queenBoard & blackBoard);
        var whiteQueenTiles = ConvertBitBoardToFauxTile(queenBoard & whiteBoard);
        foreach (var tile in blackQueenTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.QUEEN, 0, tile);
            piece.Score = 900;
            _blackPieces.Add(piece);
        }
        foreach (var tile in whiteQueenTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.QUEEN, 1, tile);
            piece.Score = 900;
            _whitePieces.Add(piece);
        }
        //King
        var blackKingTiles = ConvertBitBoardToFauxTile(kingBoard & blackBoard);
        var whiteKingTiles = ConvertBitBoardToFauxTile(kingBoard & whiteBoard);
        foreach (var tile in blackKingTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.KING, 0, tile);
            piece.Score = 2000;
            _blackPieces.Add(piece);
        }
        foreach (var tile in whiteKingTiles)
        {
            //Create new piece and place it in position
            FauxPiece piece = new FauxPiece(PieceType.KING, 1, tile);
            piece.Score = 2000;
            _whitePieces.Add(piece);
        }
        #endregion
    }

    public List<FauxTile> ConvertBitBoardToFauxTile(ulong board)
    {
        List<FauxTile> tiles = new List<FauxTile>();
        string stringBoard = Convert.ToString((long)board, 2).PadLeft(64, '0');
        for (int i = 0; i < stringBoard.Length; i++)
        {
            if (stringBoard[i] == '1')
            {
                FauxTile t = _tiles[7 - (i / 8), 7 - (i % 8)];
                tiles.Add(t);
            }
        }
        tiles.Reverse();
        return tiles;
    }

    public FauxTile GetTileAtPosition(int x, int y)
    {
        if (x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT) return _tiles[x, y];
        return null;
    }

    public ulong CalculateTileBitBoard(List<FauxTile> tiles)
    {
        ulong board = 0;
        foreach (var tile in tiles)
        {
            board |= tile.BitBoard;
        }

        return board;
    }

    public Dictionary<FauxPiece, ulong> GetPossibleMoves(int color)
    {
        Dictionary<FauxPiece, ulong> legalMoves = new Dictionary<FauxPiece, ulong>();
        var pieces = color == 0 ? _blackPieces : _whitePieces;

        foreach (var piece in pieces)
        {
            if (piece.PieceType == PieceType.KING)
                continue;
            if (!piece.IsSameColor(color))
                continue;
            var tiles = GetMoveTiles(piece);
            legalMoves.Add(piece, tiles);
        }

        return legalMoves;
    }

    #region Faux Move Generation
    public ulong GetMoveTiles(FauxPiece piece)
    {
        switch (piece.PieceType)
        {
            case PieceType.PAWN:
                return GetPawnMoveTiles(piece, piece.Tile);
            case PieceType.CASTLE:
                return GetSlidingMoveTiles(piece, piece.Tile);
            case PieceType.KNIGHT:
                return GetKnightMoves(piece, piece.Tile);
            case PieceType.BISHOP:
                return GetSlidingMoveTiles(piece, piece.Tile);
            case PieceType.QUEEN:
                return GetSlidingMoveTiles(piece, piece.Tile);
            case PieceType.KING:
                return GetKingMoveTiles(piece, piece.Tile);
            default:
                break;
        }
        return 0x64;
    }
    public  ulong GetAttackTiles(FauxPiece piece, FauxTile fromPos)
    {
        switch (piece.PieceType)
        {
            case PieceType.PAWN:
                return GetPawnAttackTiles(piece, fromPos);
            case PieceType.CASTLE:
                return GetSlidingAttackTiles(piece, fromPos);
            case PieceType.KNIGHT:
                return GetKnightAttackMoves(piece, fromPos);
            case PieceType.BISHOP:
                return GetSlidingAttackTiles(piece, fromPos);
            case PieceType.QUEEN:
                return GetSlidingAttackTiles(piece, fromPos);
            case PieceType.KING:
                return GetSlidingAttackTiles(piece, fromPos);
            default:
                break;
        }
        return 0x64;
    }

    private ulong GetPawnMoveTiles(FauxPiece pawn, FauxTile fromPos)
    {
        List<FauxTile> tiles = new List<FauxTile>();
        int direction = pawn.Color == 0 ? -1 : 1;
        Vector2 pos = fromPos.Position;

        //Check the tile directly in front of the pawn and the two diagonals
        FauxTile infront = GetTileAtPosition((int)pos.y + direction, (int)pos.x);
        FauxTile infront2 = GetTileAtPosition((int)pos.y + (direction * 2), (int)pos.x);
        FauxTile leftDiag = GetTileAtPosition((int)pos.y + direction, (int)pos.x - direction);
        FauxTile rightDiag = GetTileAtPosition((int)pos.y + direction, (int)pos.x + direction);

        if (infront != null && infront.Piece == null)
        {
            tiles.Add(infront);
            if (infront2 != null && infront2.Piece == null && !pawn.HasMadeFirstMove)
            {
                tiles.Add(infront2);
            }
        }
        if (rightDiag != null)
        {
            if (rightDiag.Piece != null && !rightDiag.Piece.IsSameColor(pawn) && rightDiag.Piece.PieceType != PieceType.KING)
            {
                tiles.Add(rightDiag);
            }
        }
        if (leftDiag != null)
        {
            if (leftDiag.Piece != null && !leftDiag.Piece.IsSameColor(pawn) && leftDiag.Piece.PieceType != PieceType.KING)
            {
                tiles.Add(leftDiag);
            }
        }
        return CalculateTileBitBoard(tiles);
    }
    private ulong GetPawnAttackTiles(FauxPiece pawn, FauxTile fromPos)
    {
        List<FauxTile> tiles = new List<FauxTile>();
        int direction = pawn.Color == 0 ? -1 : 1;

        var pos = fromPos.Position;

        //Pawns can only attack diagonally
        FauxTile leftDiag = GetTileAtPosition((int)pos.y + direction, (int)pos.x - direction);
        FauxTile rightDiag = GetTileAtPosition((int)pos.y + direction, (int)pos.x + direction);

        if (rightDiag != null)
            tiles.Add(rightDiag);
        if (leftDiag != null)
            tiles.Add(leftDiag);

        return CalculateTileBitBoard(tiles);
    }

    private ulong GetKnightMoves(FauxPiece piece, FauxTile fromPos)
    {
        List<FauxTile> knightTiles = new List<FauxTile>();

        int[] dx = { 2, 2, 1, 1, -1, -1, -2, -2 };
        int[] dy = { 1, -1, 2, -2, 2, -2, 1, -1 };

        for (int i = 0; i < dx.Length; i++)
        {
            FauxTile t = GetTileAtPosition((int)fromPos.Position.y + dx[i], (int)fromPos.Position.x + dy[i]);
            if (t != null && (t.Piece == null || !t.Piece.IsSameColor(piece)))
            {
                knightTiles.Add(t);
            }
        }
        return CalculateTileBitBoard(knightTiles);
    }
    private ulong GetKnightAttackMoves(FauxPiece piece, FauxTile fromPos)
    {
        List<FauxTile> knightTiles = new List<FauxTile>();

        int[] dx = { 2, 2, 1, 1, -1, -1, -2, -2 };
        int[] dy = { 1, -1, 2, -2, 2, -2, 1, -1 };

        for (int i = 0; i < dx.Length; i++)
        {
            FauxTile t = GetTileAtPosition((int)fromPos.Position.y + dx[i], (int)fromPos.Position.x + dy[i]);
            if (t != null)
            {
                knightTiles.Add(t);
            }
        }
        return CalculateTileBitBoard(knightTiles);
    }

    private ulong GetSlidingMoveTiles(FauxPiece piece, FauxTile fromPos)
    {
        int numRows = 8;
        int numCols = 8;

        List<FauxTile> horizontal = new List<FauxTile>();
        List<FauxTile> vertical = new List<FauxTile>();
        List<FauxTile> diagonalDownwards = new List<FauxTile>();
        List<FauxTile> diagonalUpwards = new List<FauxTile>();

        #region Horizontal sliding movements
        for (int i = 0; i < numRows; i++)
        {
            FauxTile tile = GetTileAtPosition((int)fromPos.Position.y, i);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (fromPos.Position.x > i)
                {
                    horizontal.Clear();
                    if (tile.Piece.IsSameColor(piece))
                    {
                        continue;
                    }
                    else
                    {
                        horizontal.Add(tile);
                    }
                }
                else
                {
                    if (!tile.Piece.IsSameColor(piece))
                    {
                        horizontal.Add(tile);
                    }
                    break;
                }
            }
            if (tile.Piece == piece)
            {
                continue;
            }
            horizontal.Add(tile);
        }
        #endregion

        #region  Vertical sliding movements
        for (int i = 0; i < numRows; i++)
        {
            FauxTile tile = GetTileAtPosition(i, (int)fromPos.Position.x);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (i < fromPos.Position.y)
                {
                    vertical.Clear();
                    if (tile.Piece.IsSameColor(piece))
                    {
                        continue;
                    }
                    else
                    {
                        vertical.Add(tile);
                    }
                }
                else
                {
                    if (!tile.Piece.IsSameColor(piece))
                    {
                        vertical.Add(tile);
                    }
                    break;
                }
            }
            if (tile.Piece == piece)
            {
                continue;
            }
            vertical.Add(tile);
        }
        #endregion

        #region Diagonal sliding movements - left to right (upwards)
        for (int i = 0; i < numRows; i++)
        {
            int j = (i - (int)fromPos.Position.y) + (int)fromPos.Position.x;
            if (j >= 0 && j < numCols)
            {
                FauxTile tile = GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (i < fromPos.Position.y)
                    {
                        diagonalUpwards.Clear();
                        if (tile.Piece.IsSameColor(piece))
                        {
                            continue;
                        }
                        else
                        {
                            diagonalUpwards.Add(tile);
                        }
                    }
                    else
                    {
                        if (!tile.Piece.IsSameColor(piece))
                        {
                            diagonalUpwards.Add(tile);
                        }
                        break;
                    }
                }
                if (tile.Piece == piece)
                {
                    continue;
                }
                diagonalUpwards.Add(tile);
            }
        }
        #endregion

        #region Diagonal sliding movements - left to right (downwards)
        for (int i = 0; i < numRows; i++)
        {
            int j = (int)fromPos.Position.y + (int)fromPos.Position.x - i;
            if (j >= 0 && j < numCols)
            {
                FauxTile tile = GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (fromPos.Position.x < j)
                    {
                        diagonalDownwards.Add(tile);
                        if (tile.Piece.IsSameColor(piece))
                        {
                            diagonalDownwards.Clear();
                            continue;
                        }
                        else
                        {
                            diagonalDownwards.Clear();
                            diagonalDownwards.Add(tile);
                            continue;
                        }
                    }
                    else
                    {
                        if (!tile.Piece.IsSameColor(piece))
                        {
                            diagonalDownwards.Add(tile);
                        }
                        break;
                    }
                }
                if (tile.Piece == piece)
                {
                    continue;
                }
                diagonalDownwards.Add(tile);
            }
        }
        #endregion

        if (piece.PieceType == PieceType.CASTLE)
            return CalculateTileBitBoard(horizontal.Concat(vertical).ToList());

        if (piece.PieceType == PieceType.BISHOP)
            return CalculateTileBitBoard(diagonalUpwards.Concat(diagonalDownwards).ToList());

        if (piece.PieceType == PieceType.QUEEN)
            return CalculateTileBitBoard(horizontal.Concat(vertical).Concat(diagonalUpwards).Concat(diagonalDownwards).ToList());

        return 0x64;
    }
    private ulong GetSlidingAttackTiles(FauxPiece piece, FauxTile fromPos)
    {
        int numRows = 8;
        int numCols = 8;

        List<FauxTile> horizontal = new List<FauxTile>();
        List<FauxTile> vertical = new List<FauxTile>();
        List<FauxTile> diagonalDownwards = new List<FauxTile>();
        List<FauxTile> diagonalUpwards = new List<FauxTile>();

        #region Horizontal sliding movements
        for (int i = 0; i < numRows; i++)
        {
            FauxTile tile = GetTileAtPosition((int)fromPos.Position.y, i);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (fromPos.Position.x > i)
                {
                    horizontal.Clear();
                    if (tile.Piece.IsSameColor(piece))
                    {
                        horizontal.Add(tile);
                        continue;
                    }
                }
                else
                {
                    if (!tile.Piece.IsSameColor(piece))
                    {
                        horizontal.Add(tile);
                    }
                    break;
                }
            }
            if (tile.Piece == piece)
            {
                continue;
            }
            horizontal.Add(tile);
        }
        #endregion

        #region  Vertical sliding movements
        for (int i = 0; i < numRows; i++)
        {
            FauxTile tile = GetTileAtPosition(i, (int)fromPos.Position.x);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (i < fromPos.Position.y)
                {
                    vertical.Clear();
                    if (tile.Piece.IsSameColor(piece))
                    {
                        continue;
                    }
                    else
                    {
                        vertical.Add(tile);
                    }
                }
                else
                {
                    if (!tile.Piece.IsSameColor(piece))
                    {
                        vertical.Add(tile);
                    }
                    break;
                }
            }
            if (tile.Piece == piece)
            {
                continue;
            }
            vertical.Add(tile);
        }
        #endregion

        #region Diagonal sliding movements - left to right (upwards)
        for (int i = 0; i < numRows; i++)
        {
            int j = (i - (int)fromPos.Position.y) + (int)fromPos.Position.x;
            if (j >= 0 && j < numCols)
            {
                FauxTile tile = GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (i < fromPos.Position.y)
                    {
                        diagonalUpwards.Clear();
                        if (tile.Piece.IsSameColor(piece))
                        {
                            continue;
                        }
                        else
                        {
                            diagonalUpwards.Add(tile);
                        }
                    }
                    else
                    {
                        if (!tile.Piece.IsSameColor(piece))
                        {
                            diagonalUpwards.Add(tile);
                        }
                        break;
                    }
                }
                if (tile.Piece == piece)
                {
                    continue;
                }
                diagonalUpwards.Add(tile);
            }
        }
        #endregion

        #region Diagonal sliding movements - left to right (downwards)
        for (int i = 0; i < numRows; i++)
        {
            int j = (int)fromPos.Position.y + (int)fromPos.Position.x - i;
            if (j >= 0 && j < numCols)
            {
                FauxTile tile = GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (fromPos.Position.x < j)
                    {
                        diagonalDownwards.Add(tile);
                        if (tile.Piece.IsSameColor(piece))
                        {
                            diagonalDownwards.Clear();
                            continue;
                        }
                        else
                        {
                            diagonalDownwards.Clear();
                            diagonalDownwards.Add(tile);
                            continue;
                        }
                    }
                    else
                    {
                        if (!tile.Piece.IsSameColor(piece))
                        {
                            diagonalDownwards.Add(tile);
                        }
                        break;
                    }
                }
                if (tile.Piece == piece)
                {
                    continue;
                }
                diagonalDownwards.Add(tile);
            }
        }
        #endregion

        if (piece.PieceType == PieceType.CASTLE)
            return CalculateTileBitBoard(horizontal.Concat(vertical).ToList());

        if (piece.PieceType == PieceType.BISHOP)
            return CalculateTileBitBoard(diagonalUpwards.Concat(diagonalDownwards).ToList());

        if (piece.PieceType == PieceType.QUEEN)
            return CalculateTileBitBoard(horizontal.Concat(vertical).Concat(diagonalUpwards).Concat(diagonalDownwards).ToList());

        return 0x64;
    }

    private ulong GetKingMoveTiles(FauxPiece piece, FauxTile fromPos)
    {
        List<FauxTile> kingTiles = new List<FauxTile>();

        //Get all 8 surrounding tiles
        int yMin = (int)fromPos.Position.y - 1 < 0 ? (int)fromPos.Position.y : (int)fromPos.Position.y - 1;
        int yMax = (int)fromPos.Position.y + 1 > 8 ? (int)fromPos.Position.y : (int)fromPos.Position.y + 1;
        int xMin = (int)fromPos.Position.x - 1 < 0 ? (int)fromPos.Position.x : (int)fromPos.Position.x - 1;
        int xMax = (int)fromPos.Position.x + 1 > 8 ? (int)fromPos.Position.x : (int)fromPos.Position.x + 1;
        for (int i = yMin; i <= yMax; i++)
        {
            for (int j = xMin; j <= xMax; j++)
            {
                if (i != (int)fromPos.Position.y || j != (int)fromPos.Position.x)
                {
                    FauxTile tile = GetTileAtPosition(i, j);

                    if (tile == null)
                        continue;

                    if (tile.Piece == null || !tile.Piece.IsSameColor(piece))
                        kingTiles.Add(tile);
                }
            }
        }

        //Get the attacked tiles around the king
        int color = piece.Color;
        ulong attackedSquares = 0;// = BoardManager.Instance.GetAttackSquares(color == 0 ? 1 : 0);
        var kingMoveBoard = CalculateTileBitBoard(kingTiles);
        var possibleMoveSquares = kingMoveBoard & ~attackedSquares;
        return possibleMoveSquares;
    }
    #endregion

    public FauxBoard MovePiece(FauxPiece piece, FauxTile targetTile)
    {
        FauxTile currentTile = piece.Tile;
        piece.HasMadeFirstMove = true;

        if (targetTile.Piece != null)
        {
            _takenPiece = targetTile.Piece;

            if (targetTile.Piece.Color == 0)
            {
                _blackPieces.Remove(targetTile.Piece);
            }
            else
            {
                _whitePieces.Remove(targetTile.Piece);
            }
        }  
        targetTile.Piece = null;

        currentTile.Piece = null;
        piece.Tile = targetTile;

        _movedPiece = piece;
        _fromTile = currentTile;

        _hasMadeMove = true;

        return this;

        //Update bitboard
        //UpdatePieceBitBoard(piece);
    }

    public void UndoMove()
    {
        if (_hasMadeMove)
            return;
        if (_takenPiece != null)
        {
            FauxTile takenTile = _movedPiece.Tile;
            takenTile.Piece = _takenPiece;

            if (_takenPiece.Color == 0)
            {
                _blackPieces.Add(_takenPiece);
            }
            else
            {
                _whitePieces.Add(_takenPiece);
            }
        }

        _movedPiece.Tile = _fromTile;

        _takenPiece = null;
        _movedPiece = null;
        _fromTile = null;

        _hasMadeMove = false;
    }

    public int Evaluate()
    {
        int scoreWhite = 0;
        int scoreBlack = 0;
        scoreWhite += GetPiecesScore(1);
        scoreBlack += GetPiecesScore(0);

        int evaluation = scoreBlack - scoreWhite;

        int prespective = (GameManager.State == GameState.WHITETURN) ? -1 : 1;
        Debug.Log($"Calculated point is {evaluation * prespective}");
        return evaluation * prespective;
    }

    private int GetPiecesScore(int color)
    {
        int score = 0;

        if (color == 0)
        {
            foreach (var piece in _blackPieces)
            {
                score += piece.Score;
            }
            return score;
        }

        foreach (var piece in _whitePieces)
        {
            score += piece.Score;
        }

        return score;
    }
}
