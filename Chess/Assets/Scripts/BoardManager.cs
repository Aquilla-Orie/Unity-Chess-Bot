using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PieceType
{
    PAWN = 0,
    CASTLE = 1,
    KNIGHT = 2,
    BISHOP = 3,
    QUEEN = 4,
    KING = 5
}

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    private const int HEIGHT = 8;
    private const int WIDTH = 8;

    private Tile[,] _tiles = new Tile[HEIGHT, WIDTH];

    [SerializeField] private Piece _piecePrefab;

    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Color _evenTileColor;
    [SerializeField] private Color _oddTileColor;

    private GameObject _tilesHolder;
    private GameObject _piecesHolder;


    private List<Tile> _selectedTiles = new List<Tile>();
    private ulong finalBoard;

    [SerializeField] public TMP_Text _tileBitboardText;

    [Space(50)]
    [Header("Piece BitBoard Visual")]
    [SerializeField] public bool _showBlackPawns;
    [SerializeField] public bool _showWhitePawns;
    [SerializeField] public bool _showBlackKnights;
    [SerializeField] public bool _showWhiteKnights;
    [SerializeField] public bool _showBlackBishops;
    [SerializeField] public bool _showWhiteBishops;
    [SerializeField] public bool _showBlackRooks;
    [SerializeField] public bool _showWhiteRooks;
    [SerializeField] public bool _showBlackQueen;
    [SerializeField] public bool _showWhiteQueen;
    [SerializeField] public bool _showBlackKing;
    [SerializeField] public bool _showWhiteKing;

    private ulong _whitePawn = 0, _whiteKnight = 0, _whiteBishop = 0, _whiteRook = 0, _whiteQueen = 0, _whiteKing = 0;//Bitboard for white pieces
    private ulong _blackPawn = 0, _blackKnight = 0, _blackBishop = 0, _blackRook = 0, _blackQueen = 0, _blackKing = 0;//Bitboard for black pieces

    private string[][] _board = new string[][]
    {
        //lowercase for black, uppercase for white
        new string[] {"R", "N", "B", "Q", "K", "B", "N", "R"},
        new string[] {"P", "P", "P", "P", "P", "P", "P", "P"},
        new string[] {" ", " ", " ", " ", " ", " ", " ", " "},
        new string[] {" ", " ", " ", " ", " ", " ", " ", " "},
        new string[] {" ", " ", " ", " ", " ", " ", " ", " "},
        new string[] {" ", " ", " ", " ", " ", " ", " ", " "},
        new string[] {"p", "p", "p", "p", "p", "p", "p", "p"},
        new string[] {"r", "n", "b", "q", "k", "b", "n", "r"},
    };


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;

        _tilesHolder = new GameObject("Tiles Holder");
        _piecesHolder = new GameObject("Pieces Holder");
    }

    private void Start()
    {
        InitializeBoard();
        GameManager.Instance.Init();

        //GetAllLegalMoves(0, GetWhiteBoard() | GetBlackBoard());
    }

    private void InitializeBoard()
    {
        ConvertArrayToBitBoard();
        GenerateBoardTiles();
        GenerateBoardPieces();
    }
    
    private void ConvertArrayToBitBoard()
    {
        string binary;
        for (int i = 0; i < WIDTH * HEIGHT; i++)
        {
            binary = new string('0', 64);
            binary = binary.Substring(i + 1) + "1" + binary.Substring(0, i);

            switch (_board[i / 8][i % 8])
            {
                case "P":
                    _whitePawn += ConvertStringToBitBoard(binary);
                    break;
                case "R":
                    _whiteRook += ConvertStringToBitBoard(binary);
                    break;
                case "B":
                    _whiteBishop += ConvertStringToBitBoard(binary);
                    break;
                case "N":
                    _whiteKnight += ConvertStringToBitBoard(binary);
                    break;
                case "Q":
                    _whiteQueen += ConvertStringToBitBoard(binary);
                    break;
                case "K":
                    _whiteKing += ConvertStringToBitBoard(binary);
                    break;
                case "p":
                    _blackPawn += ConvertStringToBitBoard(binary);
                    break;
                case "r":
                    _blackRook += ConvertStringToBitBoard(binary);
                    break;
                case "b":
                    _blackBishop += ConvertStringToBitBoard(binary);
                    break;
                case "n":
                    _blackKnight += ConvertStringToBitBoard(binary);
                    break;
                case "q":
                    _blackQueen += ConvertStringToBitBoard(binary);
                    break;
                case "k":
                    _blackKing += ConvertStringToBitBoard(binary);
                    break;
            }
        }
    }
    
    private void GenerateBoardTiles()
    {
        string binary;
        for (int file = 0; file < WIDTH; file++)
        {
            for (int rank = 0; rank < HEIGHT; rank++)
            {
                binary = new string('0', 64);
                binary = binary.Substring(((file * WIDTH) + rank) + 1) + "1" + binary.Substring(0, ((file * WIDTH) + rank));
                var tile = Instantiate(_tilePrefab, _tilesHolder.transform);
                tile.transform.position = new Vector2(rank, file);

                tile.StringBitBoard = binary;
                tile.BitBoard = ConvertStringToBitBoard(tile.StringBitBoard);

                var tileSprite = tile.GetComponent<SpriteRenderer>();

                tileSprite.color = ((file + rank) % 2 == 0) ? _evenTileColor : _oddTileColor;
                char col = (char)(97 + rank);
                char row = (char)(49 + file);
                //tile.name = $"Tile {row}{col}({rank},{file})";
                tile.name = $"{col}{row}";
                tile.Position = new int[]{ file, rank };


                _tiles[file, rank] = tile;
            }
        }
    }

    private void GenerateBoardPieces()
    {
        for (int i = 0; i < WIDTH * HEIGHT; i++)
        {
            if (((_whitePawn >> i) & 1) == 1)
                SetPiecePosition(PieceType.PAWN,i % 8, i / 8, 1);
            if (((_whiteRook >> i) & 1) == 1)
                SetPiecePosition(PieceType.CASTLE, i % 8, i / 8, 1);
            if (((_whiteBishop >> i) & 1) == 1)
                SetPiecePosition(PieceType.BISHOP, i % 8, i / 8, 1);
            if (((_whiteKnight >> i) & 1) == 1)
                SetPiecePosition(PieceType.KNIGHT, i % 8, i / 8, 1);
            if (((_whiteQueen >> i) & 1) == 1)
                SetPiecePosition(PieceType.QUEEN, i % 8, i / 8, 1);
            if (((_whiteKing >> i) & 1) == 1)
                SetPiecePosition(PieceType.KING, i % 8, i / 8, 1);
            if (((_blackPawn >> i) & 1) == 1)
                SetPiecePosition(PieceType.PAWN, i % 8, i / 8, 0);
            if (((_blackRook >> i) & 1) == 1)
                SetPiecePosition(PieceType.CASTLE, i % 8, i / 8, 0);
            if (((_blackBishop >> i) & 1) == 1)
                SetPiecePosition(PieceType.BISHOP, i % 8, i / 8, 0);
            if (((_blackKnight >> i) & 1) == 1)
                SetPiecePosition(PieceType.KNIGHT, i % 8, i / 8, 0);
            if (((_blackQueen >> i) & 1) == 1)
                SetPiecePosition(PieceType.QUEEN, i % 8, i / 8, 0);
            if (((_blackKing >> i) & 1) == 1)
                SetPiecePosition(PieceType.KING, i % 8, i / 8, 0);
        }
    }


    public ulong ConvertStringToBitBoard(string binary)
    {
        if (binary[0] == '0')//Number is not negative
            return Convert.ToUInt64(binary, 2);

        return Convert.ToUInt64("1" + binary.Substring(2), 2) * 2;
    }

    public ulong CalculateTileBitBoard(List<Tile> tiles)
    {
        ulong tileBoards = 0;
        //Loop through each tile and get its position in the bitboard
        foreach (Tile tile in tiles)
        {
            tileBoards |= tile.BitBoard;
        }
        return tileBoards;
    }

    private void SetPiecePosition(PieceType type, int x, int y, int color)
    {
        var piece = Instantiate(_piecePrefab, _piecesHolder.transform);
        piece.transform.position = new Vector2(x, y);

        piece.SetPiece(type, color);
        piece.name = $"{type}";
    }

    public Tile GetTileAtPosition(int x, int y)
    {
        if (x >= 0 && x < WIDTH && y >=0 && y < HEIGHT) return _tiles[x, y];
        return null;
    }

    public Tile GetTileByName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length != 2) return null;
        char file = name[0];
        char rank = name[1];

        int col = (char)(file - 97);
        int row = (char)(rank - 49);

        return GetTileAtPosition(row, col);
    }

    public List<Tile> ConvertBitBoardToTile(ulong board)
    {
        List<Tile> tiles = new List<Tile>();
        string stringBoard = Convert.ToString((long)board, 2).PadLeft(64, '0');
        for (int i = 0; i < stringBoard.Length; i++)
        {
            if (stringBoard[i] == '1')
            {
                Tile t = GetTileAtPosition(7 - (i/8), 7 - (i%8));
                tiles.Add(t);
            }
        }

        return tiles;
    }

    public List<Piece> ConvertBitBoardToPiece(ulong board)
    {
        List<Piece> pieces = new List<Piece>();
        string stringBoard = Convert.ToString((long)board, 2).PadLeft(64, '0');
        for (int i = 0; i < stringBoard.Length; i++)
        {
            if (stringBoard[i] == '1')
            {
                Tile t = GetTileAtPosition(7 - (i / 8), 7 - (i % 8));
                pieces.Add(t.Piece);
            }
        }

        return pieces;
    }

    public string ConvertUInt64ToBinary(ulong input)
    {
        return $"{Convert.ToString((long)input, 2).PadLeft(64, '0')}";
    }

    public int GetPiecesScore(int color, ulong board)
    {
        int score = 0;
        int pieceValue = 0;
        var pieces = ConvertBitBoardToPiece(board);
        foreach (Piece piece in pieces)
        {
            if (!piece.IsSameColor(color))
                continue;

            pieceValue = piece.Score;
            pieceValue *= (piece.GetColor() == 0) ? -1 : 1;

            score += pieceValue;
        }
        return score;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetAttackSquares(1);
        }
        //HandleVisual();

    }

    private void HandleVisual()
    {
        if (_showBlackPawns)
        {
            ShowBitBoard(_blackPawn);
        }
        else
        {
            ClearBitBoardVisual(_blackPawn);
        }
        if (_showWhitePawns)
        {
            ShowBitBoard(_whitePawn);
        }
        else
        {
            ClearBitBoardVisual(_whitePawn);
        }
        if (_showBlackKnights)
        {
            ShowBitBoard(_blackKnight);
        }
        else
        {
            ClearBitBoardVisual(_blackKnight);
        }
        if (_showWhiteKnights)
        {
            ShowBitBoard(_whiteKnight);
        }
        else
        {
            ClearBitBoardVisual(_whiteKnight);
        }
        if (_showBlackBishops)
        {
            ShowBitBoard(_blackBishop);
        }
        else
        {
            ClearBitBoardVisual(_blackBishop);
        }
        if (_showWhiteBishops)
        {
            ShowBitBoard(_whiteBishop);
        }
        else
        {
            ClearBitBoardVisual(_whiteBishop);
        }
        if (_showBlackRooks)
        {
            ShowBitBoard(_blackRook);
        }
        else
        {
            ClearBitBoardVisual(_blackRook);
        }
        if (_showWhiteRooks)
        {
            ShowBitBoard(_whiteRook);
        }
        else
        {
            ClearBitBoardVisual(_whiteRook);
        }
        if (_showBlackQueen)
        {
            ShowBitBoard(_blackQueen);
        }
        else
        {
            ClearBitBoardVisual(_blackQueen);
        }
        if (_showWhiteQueen)
        {
            ShowBitBoard(_whiteQueen);
        }
        else
        {
            ClearBitBoardVisual(_whiteQueen);
        }
        if (_showBlackKing)
        {
            ShowBitBoard(_blackKing);
        }
        else
        {
            ClearBitBoardVisual(_blackKing);
        }
        if (_showWhiteKing)
        {
            ShowBitBoard(_whiteKing);
        }
        else
        {
            ClearBitBoardVisual(_whiteKing);
        }
    }

    private void RotateBoard()
    {
        Quaternion newRotation = Camera.main.transform.rotation;
        if ((int)newRotation.z == 0)
        {
            newRotation.z = 180;
        }
        else
        {
            newRotation.z = 0;
        }
        
        Debug.Log($"New rotation x{newRotation.x}, y{newRotation.y}, z{newRotation.z}");
        Camera.main.transform.rotation = newRotation;

        foreach (Transform piece in _piecesHolder.transform)
        {
            piece.rotation = newRotation;
        }
    }

    public ulong GetAttackSquares(int color)
    {
        ulong attackSquares = 0;
        ulong colorBoard = 0;
        //Get all pieces of said color
        if (color == 0)
            colorBoard = _blackPawn | _blackKnight | _blackBishop | _blackRook | _blackQueen | _blackKing;
        if (color == 1)
            colorBoard = _whitePawn | _whiteKnight | _whiteBishop | _whiteRook | _whiteQueen | _whiteKing;

        var pieceTiles = ConvertBitBoardToTile(colorBoard);
        foreach (var pieceTile in pieceTiles)
        {
            attackSquares |= MoveGenerator.GetAttackTiles(pieceTile.Piece, pieceTile.transform.position);
        }

        var attackTiles = ConvertBitBoardToTile(attackSquares);
        //foreach (var attackTile in attackTiles)
        //{
        //    attackTile.ShowHighlight();
        //}

        return attackSquares;
    }
    public Dictionary<Piece, ulong> GetEachAttackSquares(int color)
    {
        Dictionary<Piece, ulong> allAttackSquares = new Dictionary<Piece, ulong>();
        ulong attackSquares = 0;
        ulong colorBoard = 0;
        //Get all pieces of said color
        if (color == 0)
            colorBoard = _blackPawn | _blackKnight | _blackBishop | _blackRook | _blackQueen | _blackKing;
        if (color == 1)
            colorBoard = _whitePawn | _whiteKnight | _whiteBishop | _whiteRook | _whiteQueen | _whiteKing;

        var pieceTiles = ConvertBitBoardToTile(colorBoard);
        foreach (var pieceTile in pieceTiles)
        {
            attackSquares = MoveGenerator.GetAttackTiles(pieceTile.Piece, pieceTile.transform.position);
            allAttackSquares.Add(pieceTile.Piece, attackSquares);
        }

        return allAttackSquares;
    }
    public ulong GetAllXRaySquares(int color)
    {
        ulong xraySquares = 0;
        ulong colorBoard = 0;
        //Get all pieces of said color
        if (color == 0)
            colorBoard = _blackPawn | _blackKnight | _blackBishop | _blackRook | _blackQueen | _blackKing;
        if (color == 1)
            colorBoard = _whitePawn | _whiteKnight | _whiteBishop | _whiteRook | _whiteQueen | _whiteKing;

        var pieceTiles = ConvertBitBoardToTile(colorBoard);
        foreach (var pieceTile in pieceTiles)
        {
            xraySquares |= MoveGenerator.GetAllSlidingTiles(pieceTile.Piece, pieceTile.transform.position);
        }

        var attackTiles = ConvertBitBoardToTile(xraySquares);
        //foreach (var attackTile in attackTiles)
        //{
        //    attackTile.ShowHighlight();
        //}

        return xraySquares;
    }

    public void ClearBitBoardVisual(ulong board)
    {
        //finalBoard = 0;
        _selectedTiles.Clear();
        _selectedTiles = ConvertBitBoardToTile(board);
        foreach (var tile in _selectedTiles)
        {
            tile.HideHighlight();
        }
        _selectedTiles.Clear();
    }

    public void DisableAllPieces()
    {
        foreach (Transform piece in _piecesHolder.transform)
        {
            Destroy(piece.GetComponent<Collider2D>());
        }
    }

    private void ShowBitBoard(ulong board)
    {
        _selectedTiles = ConvertBitBoardToTile(board);
        foreach (var tile in _selectedTiles)
        {
            tile.ShowHighlight();
        }
    }

    public Dictionary<Piece, ulong> GetAllLegalMoves(ulong board)
    {
        Dictionary<Piece, ulong> legalMoves = new Dictionary<Piece, ulong>();
        var pieces = ConvertBitBoardToPiece(board);

        foreach (var piece in pieces)
        {
            legalMoves.Add(piece, MoveGenerator.GetMoveTiles(piece, piece.transform.position));
        }

        return legalMoves;
    }
    public Dictionary<Piece, ulong> GetAllLegalMoves(int color, ulong board)
    {
        Dictionary<Piece, ulong> legalMoves = new Dictionary<Piece, ulong>();
        var pieces = ConvertBitBoardToPiece(board);

        foreach (var piece in pieces)
        {
            if (!piece.IsSameColor(color))
                continue;
            var tiles = MoveGenerator.GetMoveTiles(piece, piece.transform.position);
            legalMoves.Add(piece, tiles);

            //if (!piece.IsSameColor(1))
            //{
            //    continue;
            //}
            //var t = ConvertBitBoardToTile(tiles);

            //foreach (var tile in t)
            //{
            //    tile.ShowHighlight();
            //}
        }

        return legalMoves;
    }
    public ulong GetShieldingSquares(Tile tile, Piece king)
    {
        ulong shieldingSquares = 0;

        Vector2 kingPos = king.Tile.transform.position;
        Vector2 targetPos = tile.transform.position;

        int targetX = (int)targetPos.x;
        int targetY = (int)targetPos.y;

        for (int x = (int)kingPos.x, y = (int)kingPos.y; x != targetX || y != targetY;)
        {
            Tile shieldingTile = GetTileAtPosition(y, x);

            if (shieldingTile != null)
            {
                if (shieldingTile.Piece != null && shieldingTile.Piece != king)
                {
                    int pieceBoardIndex = shieldingTile.Piece.BoardIndex;
                    shieldingSquares |= (1UL << pieceBoardIndex);
                }

                // Move towards the target position
                if (x < targetX)
                    x++;
                else if (x > targetX)
                    x--;

                if (y < targetY)
                    y++;
                else if (y > targetY)
                    y--;
            }
            else
            {
                Debug.LogError($"Tile at position ({x}, {y}) is null!");
                break; // Break the loop if a null tile is encountered
            }
        }
        return shieldingSquares;
    }


    public ulong GenerateBoardAfterMove(ulong board, ulong from, ulong to)
    {
        //SaveBoard(board);
        ulong newBoard = (board ^ from) | to;

        //Switch tile pieces
        Tile fromTile = ConvertBitBoardToTile(from)[0];
        Tile toTile = ConvertBitBoardToTile(to)[0];

        ulong toTileBitBoard = toTile.BitBoard;

        Piece temp = fromTile.Piece;
        fromTile.Piece = null;
        temp.Tile = toTile;

        temp.BitBoard = toTileBitBoard;

        UpdatePieceBitBoard(temp);


        return newBoard;
    }

    List<ulong> savedTiles = new List<ulong>();
    public void SaveBoard(ulong board)
    {
        savedTiles.Add(board);
    }

    public ulong RestoreBoard()
    {
        ulong board = savedTiles[savedTiles.Count - 1];
        savedTiles.RemoveAt(savedTiles.Count - 1);
        return board;
    }

    #region Update Piece BitBoard
    public void UpdatePieceBitBoard(Piece piece)
    {
        if (piece == null)
            return;

        int pieceColor = piece.GetColor();
        ulong pieceBoard = piece.BitBoard;

        if (piece.PieceType == PieceType.PAWN)
        {
            UpdatePawnsBitBoard(pieceColor, pieceBoard);
        }
        if (piece.PieceType == PieceType.KNIGHT)
        {
            UpdateKnightsBitBoard(pieceColor, pieceBoard);
        }
        if (piece.PieceType == PieceType.BISHOP)
        {
            UpdateBishopsBitBoard(pieceColor, pieceBoard);
        }
        if (piece.PieceType == PieceType.CASTLE)
        {
            UpdateCastlesBitBoard(pieceColor, pieceBoard);
        }
        if (piece.PieceType == PieceType.QUEEN)
        {
            UpdateQueensBitBoard(pieceColor, pieceBoard);
        }
        if (piece.PieceType == PieceType.KING)
        {
            UpdateKingsBitBoard(pieceColor, pieceBoard);
        }
    }
    private void UpdatePawnsBitBoard(int color, ulong updateBoard)
    {
        //Update black pawns
        if(color == 0)
        {
            _blackPawn ^= updateBoard;
        }
        //Update white pawns
        if(color == 1)
        {
            _whitePawn ^= updateBoard;
        }
    }
    private void UpdateKnightsBitBoard(int color, ulong updateBoard)
    {
        //Update black knights
        if (color == 0)
        {
            _blackKnight ^= updateBoard;
        }
        //Update white knights
        if (color == 1)
        {
            _whiteKnight ^= updateBoard;
        }
    }
    private void UpdateBishopsBitBoard(int color, ulong updateBoard)
    {
        //Update black bishops
        if(color == 0)
        {
            _blackBishop ^= updateBoard;
        }
        //Update white bishops
        if(color == 1)
        {
            _whiteBishop ^= updateBoard;
        }
    }
    private void UpdateCastlesBitBoard(int color, ulong updateBoard)
    {
        //Update black castles
        if(color == 0)
        {
            _blackRook ^= updateBoard;
        }
        //Update white castles
        if(color == 1)
        {
            _whiteRook ^= updateBoard;
        }
    }
    private void UpdateQueensBitBoard(int color, ulong updateBoard)
    {
        //Update black queens
        if(color == 0)
        {
            _blackQueen ^= updateBoard;
        }
        //Update white queens
        if(color == 1)
        {
            _whiteQueen ^= updateBoard;
        }
    }
    private void UpdateKingsBitBoard(int color, ulong updateBoard)
    {
        //Update black kings
        if(color == 0)
        {
            _blackKing ^= updateBoard;
        }
        //Update white kings
        if(color == 1)
        {
            _whiteKing ^= updateBoard;
        }
    }
    #endregion

    #region Get Piece Board
    public ulong GetPieceBoard(PieceType pieceType, int color)
    {
        if (pieceType == PieceType.PAWN)
            return GetPawnBoard(color);
        if (pieceType == PieceType.BISHOP)
            return GetBishopBoard(color);
        if (pieceType == PieceType.KNIGHT)
            return GetKnightBoard(color);
        if (pieceType == PieceType.CASTLE)
            return GetRookBoard(color);
        if (pieceType == PieceType.QUEEN)
            return GetQueenBoard(color);
        if (pieceType == PieceType.KING)
            return GetKingBoard(color);

        return 0;
    }
    public ulong GetPawnBoard(int color)
    {
        return color == 0 ? _blackPawn : _whitePawn;
    }
    public ulong GetBishopBoard(int color)
    {
        return color == 0 ? _blackBishop : _whiteBishop;
    }
    public ulong GetKnightBoard(int color)
    {
        return color == 0 ? _blackKnight : _whiteKnight;
    }
    public ulong GetRookBoard(int color)
    {
        return color == 0 ? _blackRook : _whiteRook;
    }
    public ulong GetQueenBoard(int color)
    {
        return color == 0 ? _blackQueen : _whiteQueen;
    }
    public ulong GetKingBoard(int color)
    {
        return color == 0 ? _blackKing : _whiteKing;
    }
    public ulong GetWhiteBoard()
    {
        ulong board = GetPawnBoard(1) | GetBishopBoard(1) | GetKnightBoard(1) | GetRookBoard(1) | GetQueenBoard (1) | GetKingBoard(1);

        return board;
    }
    public ulong GetBlackBoard()
    {
        ulong board = GetPawnBoard(0) | GetBishopBoard(0) | GetKnightBoard(0) | GetRookBoard(0) | GetQueenBoard (0) | GetKingBoard(0);

        return board;
    }
    #endregion
}
