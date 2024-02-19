using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveGenerator : MonoBehaviour
{
    public static ulong GetMoveTiles(Piece piece, Vector2 fromPos)
    {
        switch (piece.PieceType)
        {
            case PieceType.PAWN:
                return GetPawnMoveTiles(piece, fromPos);
            case PieceType.CASTLE:
                //return GetRookMoves(piece.BitBoard, piece.Tile.Position[0], piece.Tile.Position[1]);
                return GetSlidingMoveTiles(piece, fromPos);
            case PieceType.KNIGHT:
                return GetKnightMoves(piece, fromPos);
            case PieceType.BISHOP:
                return GetSlidingMoveTiles(piece, fromPos);
            case PieceType.QUEEN:
                return GetSlidingMoveTiles(piece, fromPos);
            case PieceType.KING:
                return GetKingMoveTiles(piece, fromPos);
            default:
                break;
        }
        return 0x64;
    }
    public static ulong GetAllSlidingTiles(Piece piece, Vector2 fromPos)
    {
        switch (piece.PieceType)
        {
            case PieceType.CASTLE:
                return GetAllSlidingMoveTiles(piece, fromPos);
            case PieceType.BISHOP:
                return GetAllSlidingMoveTiles(piece, fromPos);
            case PieceType.QUEEN:
                return GetAllSlidingMoveTiles(piece, fromPos);
            default:
                break;
        }
        return 0x64;
    }
    public static ulong GetAttackTiles(Piece piece, Vector2 fromPos)
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
                return GetKingAttackTiles(piece, fromPos);
            default:
                break;
        }
        return 0x64;
    }

    private static ulong GetPawnMoveTiles(Piece pawn, Vector2 fromPos)
    {
        List<Tile> tiles = new List<Tile>();
        int direction = pawn.GetColor() == 0 ? -1 : 1;
        Vector2 pos = fromPos;

        //Check the tile directly in front of the pawn and the two diagonals
        Tile infront = BoardManager.Instance.GetTileAtPosition((int)pos.y + direction, (int)pos.x);
        Tile infront2 = BoardManager.Instance.GetTileAtPosition((int)pos.y + (direction * 2), (int)pos.x);
        Tile leftDiag = BoardManager.Instance.GetTileAtPosition((int)pos.y + direction, (int)pos.x - direction);
        Tile rightDiag = BoardManager.Instance.GetTileAtPosition((int)pos.y + direction, (int)pos.x + direction);

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
        return BoardManager.Instance.CalculateTileBitBoard(tiles);
    }
    private static ulong GetPawnAttackTiles(Piece pawn, Vector2 fromPos)
    {
        List<Tile> tiles = new List<Tile>();
        int direction = pawn.GetColor() == 0 ? -1 : 1;
        Vector2 pos = fromPos;

        //Pawns can only attack diagonally
        Tile leftDiag = BoardManager.Instance.GetTileAtPosition((int)pos.y + direction, (int)pos.x - direction);
        Tile rightDiag = BoardManager.Instance.GetTileAtPosition((int)pos.y + direction, (int)pos.x + direction);

        if (rightDiag != null)
            tiles.Add(rightDiag);
        if (leftDiag != null)
            tiles.Add(leftDiag);
        
        return BoardManager.Instance.CalculateTileBitBoard(tiles);
    }

    private static ulong GetKnightMoves(Piece piece, Vector2 fromPos)
    {
        List<Tile> knightTiles = new List<Tile>();

        int[] dx = { 2, 2, 1, 1, -1, -1, -2, -2 };
        int[] dy = { 1, -1, 2, -2, 2, -2, 1, -1 };

        for (int i = 0; i < dx.Length; i++)
        {
            Tile t = BoardManager.Instance.GetTileAtPosition((int)fromPos.y + dx[i], (int)fromPos.x + dy[i]);
            if (t != null && (t.Piece == null || !t.Piece.IsSameColor(piece)))
            {
                knightTiles.Add(t);
            }
        }
        return BoardManager.Instance.CalculateTileBitBoard(knightTiles);
    }
    private static ulong GetKnightAttackMoves(Piece piece, Vector2 fromPos)
    {
        List<Tile> knightTiles = new List<Tile>();

        int[] dx = { 2, 2, 1, 1, -1, -1, -2, -2 };
        int[] dy = { 1, -1, 2, -2, 2, -2, 1, -1 };

        for (int i = 0; i < dx.Length; i++)
        {
            Tile t = BoardManager.Instance.GetTileAtPosition((int)fromPos.y + dx[i], (int)fromPos.x + dy[i]);
            if (t != null)
            {
                knightTiles.Add(t);
            }
        }
        return BoardManager.Instance.CalculateTileBitBoard(knightTiles);
    }

    private static ulong GetSlidingMoveTiles(Piece piece, Vector2 fromPos)
    {
        int numRows = 8;
        int numCols = 8;

        List<Tile> horizontal = new List<Tile>();
        List<Tile> vertical = new List<Tile>();
        List<Tile> diagonalDownwards = new List<Tile>();
        List<Tile> diagonalUpwards = new List<Tile>();

        #region Horizontal sliding movements
        for (int i = 0; i < numRows; i++)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition((int)fromPos.y, i);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (fromPos.x > i)
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
            Tile tile = BoardManager.Instance.GetTileAtPosition(i, (int)fromPos.x);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (i < fromPos.y)
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
            int j = (i - (int)fromPos.y) + (int)fromPos.x;
            if (j >= 0 && j < numCols)
            {
                Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (i < fromPos.y)
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
            int j = (int)fromPos.y + (int)fromPos.x - i;
            if (j >= 0 && j < numCols)
            {
                Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (fromPos.x < j)
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
            return BoardManager.Instance.CalculateTileBitBoard(horizontal.Concat(vertical).ToList());

        if (piece.PieceType == PieceType.BISHOP)
            return BoardManager.Instance.CalculateTileBitBoard(diagonalUpwards.Concat(diagonalDownwards).ToList());

        if (piece.PieceType == PieceType.QUEEN)
            return BoardManager.Instance.CalculateTileBitBoard(horizontal.Concat(vertical).Concat(diagonalUpwards).Concat(diagonalDownwards).ToList());

        return 0x64;
    }
    private static ulong GetAllSlidingMoveTiles(Piece piece, Vector2 fromPos)
    {
        int numRows = 8;
        int numCols = 8;

        List<Tile> horizontal = new List<Tile>();
        List<Tile> vertical = new List<Tile>();
        List<Tile> diagonalDownwards = new List<Tile>();
        List<Tile> diagonalUpwards = new List<Tile>();

        #region Horizontal sliding movements
        int consecutiveCount = 0;
        for (int i = 0; i < numRows; i++)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition((int)fromPos.y, i);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (tile.Piece.IsSameColor(piece))
                {
                    if (fromPos.x > i)
                    {
                        horizontal.Clear();
                        horizontal.Add(tile);
                        continue;
                    }
                    else
                    {
                        horizontal.Add(tile);
                        break;
                    }
                }
                else
                {
                    consecutiveCount++;
                    if (consecutiveCount > 2)
                    {
                        continue;
                    }
                    horizontal.Add(tile);
                    continue;  // Continue to the next square if it's an opponent's piece
                }
            }
            horizontal.Add(tile);
        }
        consecutiveCount = 0;
        #endregion

        #region  Vertical sliding movements
        for (int i = 0; i < numRows; i++)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition(i, (int)fromPos.x);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (tile.Piece.IsSameColor(piece))
                {
                    if (i < fromPos.y)
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
                        consecutiveCount++;
                        if (consecutiveCount > 2)
                        {
                            continue;
                        }
                        vertical.Add(tile);
                        break;
                    }
                }
                else
                {
                    vertical.Add(tile);
                    continue;  // Continue to the next square if it's an opponent's piece
                }
            }
            vertical.Add(tile);
        }
        consecutiveCount = 0;
        #endregion

        #region Diagonal sliding movements - left to right (upwards)
        for (int i = 0; i < numRows; i++)
        {
            int j = (i - (int)fromPos.y) + (int)fromPos.x;
            if (j >= 0 && j < numCols)
            {
                Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (tile.Piece.IsSameColor(piece))
                    {
                        if (j < fromPos.y)
                        {
                            diagonalUpwards.Clear();
                            diagonalUpwards.Add(tile);
                            continue;

                        }
                        else
                        {
                            diagonalUpwards.Add(tile);
                            break;
                        }
                    }
                    else
                    {
                        consecutiveCount++;
                        if (consecutiveCount > 2)
                        {
                            continue;
                        }
                        diagonalUpwards.Add(tile);
                        if (tile.Piece.PieceType == PieceType.BISHOP || tile.Piece.PieceType == PieceType.QUEEN)
                        {
                            continue; // Stop if a piece is encountered, unless it's a bishop or queen
                        }
                    }
                }
                diagonalUpwards.Add(tile);
            }
        }
        consecutiveCount = 0;
        #endregion

        #region Diagonal sliding movements - left to right (downwards)
        for (int i = 0; i < numRows; i++)
        {
            int j = (int)fromPos.y + (int)fromPos.x - i;
            if (j >= 0 && j < numCols)
            {
                Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (tile.Piece.IsSameColor(piece))
                    {
                        if (j > fromPos.x)
                        {
                            diagonalDownwards.Clear();
                            diagonalDownwards.Add(tile);
                            continue;
                        }
                        else
                        {
                            diagonalDownwards.Add(tile);
                            break;
                        }  
                    }
                    else
                    {
                        consecutiveCount++;
                        if (consecutiveCount > 2)
                        {
                            continue;
                        }
                        diagonalDownwards.Add(tile);
                        if (tile.Piece.PieceType == PieceType.BISHOP || tile.Piece.PieceType == PieceType.QUEEN)
                        {
                            continue; // Stop if a piece is encountered, unless it's a bishop or queen
                        }
                    }
                }
                diagonalDownwards.Add(tile);
            }
        }
        #endregion

        if (piece.PieceType == PieceType.CASTLE)
            return BoardManager.Instance.CalculateTileBitBoard(horizontal.Concat(vertical).ToList());

        if (piece.PieceType == PieceType.BISHOP)
            return BoardManager.Instance.CalculateTileBitBoard(diagonalUpwards.Concat(diagonalDownwards).ToList());

        if (piece.PieceType == PieceType.QUEEN)
            return BoardManager.Instance.CalculateTileBitBoard(horizontal.Concat(vertical).Concat(diagonalUpwards).Concat(diagonalDownwards).ToList());

        return 0x64;
    }

    private static ulong GetSlidingAttackTiles(Piece piece, Vector2 fromPos)
    {
        int numRows = 8;
        int numCols = 8;

        List<Tile> horizontal = new List<Tile>();
        List<Tile> vertical = new List<Tile>();
        List<Tile> diagonalDownwards = new List<Tile>();
        List<Tile> diagonalUpwards = new List<Tile>();

        #region Horizontal sliding movements
        for (int i = 0; i < numRows; i++)
        {
            Tile tile = BoardManager.Instance.GetTileAtPosition((int)fromPos.y, i);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (fromPos.x > i)
                {
                    horizontal.Clear();

                    horizontal.Add(tile);
                    continue;
                }
                else
                {
                    horizontal.Add(tile);
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
            Tile tile = BoardManager.Instance.GetTileAtPosition(i, (int)fromPos.x);
            if (tile.Piece != null && tile.Piece != piece)
            {
                if (i < fromPos.y)
                {
                    vertical.Clear();

                    vertical.Add(tile);
                }
                else
                {
                    vertical.Add(tile);
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
            int j = (i - (int)fromPos.y) + (int)fromPos.x;
            if (j >= 0 && j < numCols)
            {
                Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (i < fromPos.y)
                    {
                        diagonalUpwards.Clear();
                        diagonalUpwards.Add(tile);
                    }
                    else
                    {
                        diagonalUpwards.Add(tile);
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
            int j = (int)fromPos.y + (int)fromPos.x - i;
            if (j >= 0 && j < numCols)
            {
                Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);
                if (tile.Piece != null && tile.Piece != piece)
                {
                    if (fromPos.x < j)
                    {
                        diagonalDownwards.Clear();
                        diagonalDownwards.Add(tile);
                        continue;

                    }
                    else
                    {
                        diagonalDownwards.Add(tile);
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
            return BoardManager.Instance.CalculateTileBitBoard(horizontal.Concat(vertical).ToList());

        if (piece.PieceType == PieceType.BISHOP)
            return BoardManager.Instance.CalculateTileBitBoard(diagonalUpwards.Concat(diagonalDownwards).ToList());

        if (piece.PieceType == PieceType.QUEEN)
            return BoardManager.Instance.CalculateTileBitBoard(horizontal.Concat(vertical).Concat(diagonalUpwards).Concat(diagonalDownwards).ToList());

        return 0x64;
    }

    private static ulong GetKingMoveTiles(Piece piece, Vector2 fromPos)
    {
        List<Tile> kingTiles = new List<Tile>();
        
        //Get all 8 surrounding tiles
        int yMin = (int)fromPos.y - 1 < 0 ? (int)fromPos.y : (int)fromPos.y - 1;
        int yMax = (int)fromPos.y + 1 > 8 ? (int)fromPos.y : (int)fromPos.y + 1;
        int xMin = (int)fromPos.x - 1 < 0 ? (int)fromPos.x : (int)fromPos.x - 1;
        int xMax = (int)fromPos.x + 1 > 8 ? (int)fromPos.x : (int)fromPos.x + 1;
        for (int i = yMin; i <= yMax; i++)
        {
            for (int j = xMin; j <= xMax; j++)
            {
                if (i != (int)fromPos.y || j != (int)fromPos.x)
                {
                    Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);

                    if (tile == null)
                        continue;

                    if (tile.Piece == null || !tile.Piece.IsSameColor(piece))
                        kingTiles.Add(tile);
                }
            }
        }

        //Get the attacked tiles around the king
        int color = piece.GetColor();
        ulong attackedSquares = BoardManager.Instance.GetAttackSquares(color == 0 ? 1 : 0);
        var kingMoveBoard = BoardManager.Instance.CalculateTileBitBoard(kingTiles);
        // Check for x-ray attacks
        ulong xraySquares = BoardManager.Instance.GetAllXRaySquares(color == 0 ? 1 : 0);
        var xrayAttackedSquares = kingMoveBoard & xraySquares;

        // Check for shielding pieces
        foreach (var tile in kingTiles)
        {
            ulong shieldingSquares = BoardManager.Instance.GetShieldingSquares(tile, piece);
            if ((shieldingSquares & attackedSquares) != 0)
            {
                // Remove x-ray attacks on this square
                xrayAttackedSquares &= ~BoardManager.Instance.CalculateTileBitBoard(new List<Tile> { tile });
            }
        }

        // Remove squares under x-ray attack
        kingMoveBoard &= ~xrayAttackedSquares;

        return kingMoveBoard;
    }
    private static ulong GetKingAttackTiles(Piece piece, Vector2 fromPos)
    {
        List<Tile> kingTiles = new List<Tile>();
        
        //Get all 8 surrounding tiles
        int yMin = (int)fromPos.y - 1 < 0 ? (int)fromPos.y : (int)fromPos.y - 1;
        int yMax = (int)fromPos.y + 1 > 8 ? (int)fromPos.y : (int)fromPos.y + 1;
        int xMin = (int)fromPos.x - 1 < 0 ? (int)fromPos.x : (int)fromPos.x - 1;
        int xMax = (int)fromPos.x + 1 > 8 ? (int)fromPos.x : (int)fromPos.x + 1;
        for (int i = yMin; i <= yMax; i++)
        {
            for (int j = xMin; j <= xMax; j++)
            {
                if (i != (int)fromPos.y || j != (int)fromPos.x)
                {
                    Tile tile = BoardManager.Instance.GetTileAtPosition(i, j);

                    if (tile == null)
                        continue;

                    kingTiles.Add(tile);
                }
            }
        }

        var kingAttackBoard = BoardManager.Instance.CalculateTileBitBoard(kingTiles);
        var possibleMoveSquares = kingAttackBoard;
        return possibleMoveSquares;
    }

    public static bool IsKingChecked(int color, out  List<ulong> checkedBoard)
    {
        checkedBoard = new List<ulong>();
        var kingPosition = BoardManager.Instance.GetKingBoard(color);
        //Get all opposite attacked tiles
        var allAttackedTiles = BoardManager.Instance.GetEachAttackSquares(color == 0 ? 1 : 0);
        //Get tiles that attack the king directly
        foreach (var attackedTile in allAttackedTiles)
        {
            if ((attackedTile.Value & kingPosition) > 0)
            {
                checkedBoard.Add(attackedTile.Key.BitBoard);
            }
        }

        return checkedBoard.Count > 0;
    }
}
