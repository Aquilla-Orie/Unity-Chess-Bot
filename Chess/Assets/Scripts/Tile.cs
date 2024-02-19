using UnityEngine;

public class Tile : MonoBehaviour
{
    public delegate void TileSelected(Tile t);
    public delegate void TileDeselected(Tile t);

    public static event TileSelected tileSelected;
    public static event TileDeselected tileDeselected;

    public Piece Piece
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
    public int[] Position { get; set; }
    public string StringBitBoard { get; set; }
    public ulong BitBoard { get; set; }
    [SerializeField] private Piece _piece;

    [SerializeField] private GameObject _highlight;

    private bool _selected;

    private void Awake()
    {
        _selected = false;
    }

    public void ShowHighlight()
    {
        _highlight.SetActive(true);
    }
    public void HideHighlight()
    {
        _highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        _selected = !_selected;
        if (_selected)
            tileSelected?.Invoke(this);
        else
            tileDeselected?.Invoke(this);
    }

    private void OnMouseEnter()
    {
        ShowHighlight();
        BoardManager.Instance._tileBitboardText.text = $"{StringBitBoard}\n{BitBoard}";
        var tiles = BoardManager.Instance.ConvertBitBoardToTile(BitBoard);
        foreach (var tile in tiles)
        {
            tile.ShowHighlight();
        }
    }
    private void OnMouseExit()
    {
        HideHighlight();
        BoardManager.Instance._tileBitboardText.text = $"";
        var tiles = BoardManager.Instance.ConvertBitBoardToTile(BitBoard);
        foreach (var tile in tiles)
        {
            tile.HideHighlight();
        }
    }
}
