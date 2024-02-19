using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public delegate void OnMoveComplete(Piece piece, Tile fromTile);
    public delegate void OnMoveFailed(Piece piece, Tile toTile);
    public delegate void OnPieceTaken(Piece piece);
    public static event OnMoveComplete onMoveComplete;
    public static event OnMoveFailed onMoveFailed;
    public static event OnPieceTaken onPieceTaken;

    public Tile Tile
    {
        get
        {
            return _tile;
        }
        set
        {
            _tile = value;
            _tile.Piece = this;
            BitBoard = _tile.BitBoard | BitBoard;
        }
    }

    public bool HasMadeFirstMove { get; private set; }
    public PieceType PieceType { get; private set; }
    public ulong BitBoard { get; /*private*/ set; }

    public int BoardIndex
    { 
        get
        {
            return (Tile.Position[0] * 8) + Tile.Position[1];
        } 
    }

    public int Score { get; private set; }
    [SerializeField] private Sprite[] _pieceTypeSpriteBlack;//[pawn, castle, knight, bishop, queen, king]
    [SerializeField] private Sprite[] _pieceTypeSpriteWhite;//[pawn, castle, knight, bishop, queen, king]
    [SerializeField] private int _pieceColor;//0 => Black, 1 => White

    [SerializeField] private Tile _tile;

    private SpriteRenderer _spriteRenderer;

    private bool _isPicked;
    private Vector2 _currentPosition;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isPicked = false;
    }

    public void SetPiece(PieceType pieceType, int pieceColor)
    {
        _currentPosition = transform.position;

        PieceType = pieceType;
        _pieceColor = pieceColor;

        Tile = BoardManager.Instance.GetTileAtPosition((int)_currentPosition.y, (int)_currentPosition.x);

        switch (pieceType)
        {
            case PieceType.PAWN:
                _spriteRenderer.sprite = _pieceColor == 0 ? _pieceTypeSpriteBlack[0] : _pieceTypeSpriteWhite[0];
                Score = 100;
                break;
            case PieceType.CASTLE:
                _spriteRenderer.sprite = _pieceColor == 0 ? _pieceTypeSpriteBlack[1] : _pieceTypeSpriteWhite[1];
                Score = 500;
                break;
            case PieceType.KNIGHT:
                _spriteRenderer.sprite = _pieceColor == 0 ? _pieceTypeSpriteBlack[2] : _pieceTypeSpriteWhite[2];
                Score = 320;
                break;
            case PieceType.BISHOP:
                _spriteRenderer.sprite = _pieceColor == 0 ? _pieceTypeSpriteBlack[3] : _pieceTypeSpriteWhite[3];
                Score = 330;
                break;
            case PieceType.QUEEN:
                _spriteRenderer.sprite = _pieceColor == 0 ? _pieceTypeSpriteBlack[4] : _pieceTypeSpriteWhite[4];
                Score = 900;
                break;
            case PieceType.KING:
                _spriteRenderer.sprite = _pieceColor == 0 ? _pieceTypeSpriteBlack[5] : _pieceTypeSpriteWhite[5];
                Score = 2000;
                break;

        }
    }

    //public void Castle(Piece king, Piece rook)
    //{
        

    //    //Update bitboard
    //    BoardManager.Instance.UpdatePieceBitBoard(this);
    //    BoardManager.Instance.ClearBitBoardVisual(BitBoard);

    //    //Restore BitBoard from temp
    //    BitBoard = Tile.BitBoard;

    //    onMoveComplete?.Invoke(this, currentTile);
    //}

    public bool MovePiece(Vector2 fromPosition, Vector2 toPosition)
    {
        Vector2 roundedPos = new Vector2(Mathf.RoundToInt(toPosition.x), Mathf.RoundToInt(toPosition.y));
        Tile targetTile = BoardManager.Instance.GetTileAtPosition((int)roundedPos.y, (int)roundedPos.x);

        Tile currentTile = Tile;

        if (!IsTileValid(targetTile, fromPosition))
        {
            transform.position = _currentPosition;
            onMoveFailed?.Invoke(this, targetTile);
            return false;
        }


        //Update position
        transform.position = roundedPos;
        _currentPosition = transform.position;
        HasMadeFirstMove = true;

        //update tile
        if (targetTile.Piece != null && targetTile.Piece != this)
        {
            onPieceTaken?.Invoke(targetTile.Piece);
            Destroy(targetTile.Piece?.gameObject);
        }

        targetTile.Piece = null;


        Tile.Piece = null;
        Tile = targetTile;

        //Update bitboard
        BoardManager.Instance.UpdatePieceBitBoard(this);
        BoardManager.Instance.ClearBitBoardVisual(BitBoard);

        //Restore BitBoard from temp
        BitBoard = Tile.BitBoard;
        

        foreach (var tile in moveables)
        {
            tile.HideHighlight();
        }
        moveables.Clear();

        onMoveComplete?.Invoke(this, currentTile);
        return true;
    }

    private bool IsTileValid(Tile targetTile, Vector2 fromPos)
    {
        if (targetTile == null) return false;
        ulong moveTiles = MoveGenerator.GetMoveTiles(this, fromPos);

        bool amongMoveTiles = (moveTiles & targetTile.BitBoard) > 0;
        bool isPinnedPiece = false;

        //Check if King in check
        bool kingChecked = MoveGenerator.IsKingChecked(_pieceColor, out List<ulong> kingCheckedBoard);
        if (kingChecked)
        {
            List<Piece> piecesCheckingKing = GetPiecesCheckingKing(kingCheckedBoard);

            //Release king to move
            if (PieceType == PieceType.KING)
                kingChecked = false;
            //Check if piece can block check
            else
            {
                if (GetBlockTiles(targetTile, moveTiles, piecesCheckingKing) > 0)
                {
                    amongMoveTiles &= true;
                    kingChecked = false;
                }
                else
                {
                    kingChecked = true;
                }
            }
        }
        //Stop pinned pieces
        var opponentBitBoard = IsSameColor(0) ? BoardManager.Instance.GetWhiteBoard() : BoardManager.Instance.GetBlackBoard();
        var opponentPieces = BoardManager.Instance.ConvertBitBoardToPiece(opponentBitBoard);
        var kingTile = BoardManager.Instance.GetKingBoard(GetColor());

        foreach (var piece in opponentPieces)
        {
            var xrayTiles = MoveGenerator.GetAllSlidingTiles(piece, piece.transform.position);
            if ((BitBoard & kingTile & xrayTiles) > 0)
            {
                Debug.Log($"I and my king are under attack");
                //Both you and your king is under xray attack
                int onp = piece.BitBoard.CompareTo(BitBoard);
                Debug.Log($"Compared to the x ray piece {onp}");
                int pnk = BitBoard.CompareTo(kingTile);
                Debug.Log($"Compared to my king piece {pnk}");


                Debug.Log($"Comparison evaluation {pnk + onp}");
                if (Mathf.Abs(onp + pnk) != 2)
                {
                    isPinnedPiece = true; //Tile is pinned
                }
            }
        }
        

        bool tileValid = amongMoveTiles && !kingChecked && !isPinnedPiece;
        return tileValid;
    }

    private static List<Piece> GetPiecesCheckingKing(List<ulong> kingCheckedBoard)
    {
        List<Piece> piecesCheckingKing = new List<Piece>();
        //Get all the pieces putting the king in check
        foreach (var checkBoard in kingCheckedBoard)
        {
            piecesCheckingKing.Add(BoardManager.Instance.ConvertBitBoardToPiece(checkBoard)[0]);
        }

        return piecesCheckingKing;
    }

    private ulong GetBlockTiles(Tile targetTile, ulong moveTiles, List<Piece> piecesCheckingKing)
    {
        ulong availableTiles = 0;
        if (piecesCheckingKing.Count == 1)
        {
            //Can either block, take checking piece, or evade with king
            var p = piecesCheckingKing[0];
            availableTiles = moveTiles & targetTile.BitBoard & MoveGenerator.GetAttackTiles(p, p.transform.position);
        }

        return availableTiles;
    }

    public int GetColor()
    {
        return _pieceColor;
    }

    public bool IsSameColor(Piece piece)
    {
        return GetColor() == piece.GetColor();
    }

    public bool IsSameColor(int color)
    {
        return GetColor() == color;
    }


    private void Update()
    {
        if (!_isPicked)
            return;
        Vector2 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = newPos;
    }

    private void OnMouseDown()
    {
        if (IsAIPlaying()) return;
        _isPicked = true;
    }

    List<Tile> moveables = new List<Tile>();
    private void OnMouseEnter()
    {
        if (IsAIPlaying()) return;
        var moveLocations = MoveGenerator.GetMoveTiles(this, _currentPosition);
        moveables = BoardManager.Instance.ConvertBitBoardToTile(moveLocations);
        foreach (var tile in moveables)
        {
            tile.ShowHighlight();
        }

        BoardManager.Instance._tileBitboardText.text = BoardManager.Instance.ConvertUInt64ToBinary(BitBoard);
    }
    private void OnMouseExit()
    {
        if (IsAIPlaying()) return;
        foreach (var tile in moveables)
        {
            tile.HideHighlight();
        }
        moveables.Clear();
        BoardManager.Instance._tileBitboardText.text = "";
    }
    private void OnMouseUp()
    {
        if (IsAIPlaying()) return;
        _isPicked = false;
        MovePiece(_currentPosition, transform.position);
    }

    private void OnDestroy()
    {
        //When the piece is taken, clear from board
        BoardManager.Instance.UpdatePieceBitBoard(this);
        BitBoard = 0;

        foreach (var tile in moveables)
        {
            tile.HideHighlight();
        }
        moveables.Clear();
        BoardManager.Instance._tileBitboardText.text = "";
    }

    private bool IsAIPlaying()
    {
        //return false;
        return !(GameManager.State == GameState.WHITETURN )|| GetColor() == 0;
    }
}
