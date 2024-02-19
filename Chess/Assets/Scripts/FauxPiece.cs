public class FauxPiece
{
    public FauxTile Tile
    {
        get
        {
            return _tile;
        }
        set
        {
            _tile = value;
            _tile.Piece = this;
            BitBoard = _tile.BitBoard;
        }
    }
    public bool IsTaken { get; set; }
    public bool HasMadeFirstMove { get; set; }
    public PieceType PieceType { get; private set; }
    public ulong BitBoard { get; /*private*/ set; }
    public int Score { get; set; }
    public int Color { get; private set; }//0 => Black, 1 => White
    private FauxTile _tile;

    public FauxPiece(PieceType pieceType, int color, FauxTile tile)
    {
        PieceType = pieceType;
        Tile = tile;
        Color = color;
    }

    public bool IsSameColor(FauxPiece piece)
    {
        return Color == piece.Color;
    }
    public bool IsSameColor(int color)
    {
        return Color == color;
    }

}
