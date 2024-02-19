using TMPro;
using UnityEngine;

public enum GameState
{
    WHITETURN,
    BLACKTURN,
    BLACKCHECKED,
    WHITECHECKED,
    CHECKMATE
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private AIBrain _ai;
    [SerializeField] private GameObject _gameoverObject;
    [SerializeField] private TMP_Text _gameoverText;
    public static GameState State { get; private set; }
    [SerializeField] private int AIColor;
    //Handles Setting up the player and AI profiles
    //Handles the player/AI turn exectution

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    public void Init()
    {
        AIColor = 0;
        _ai?.Init(AIColor);
        State = GameState.WHITETURN;
    }

    private void OnEnable()
    {
        Piece.onMoveComplete += ResolveCompletedMove;
        Piece.onMoveFailed += ResolveFailedMove;
    }

    private void OnDisable()
    {
        Piece.onMoveComplete -= ResolveCompletedMove;
        Piece.onMoveFailed -= ResolveFailedMove;
    }

    private void ResolveCompletedMove(Piece piece, Tile fromTile)
    {
        //Debug.Log($"Piece {piece.name} has completed a move from {fromTile.name} to {piece.Tile.name}");
        if (piece.IsSameColor(AIColor))//Assumes AI is always black. Will fix this later
        {
            aiReplay = false;
            State = GameState.WHITETURN;
        }
        else
        {
            State = GameState.BLACKTURN;
        }
        Debug.Log($"Game State is {State}");
        ResolveGameState(State);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResolveBlackTurn();
        }
    }

    bool aiReplay = false;
    private void ResolveFailedMove(Piece piece, Tile toTile)
    {
        //Debug.Log($"Piece {piece.name} has failed trying to make a move from {piece.Tile.name} to {toTile.name}");
        if (piece.GetColor() == AIColor)//Assumes AI is always black. Will fix this later
        {
            State = GameState.BLACKTURN;
            aiReplay = true;
        }
        else
        {
            State = GameState.WHITETURN;
        }
        Debug.Log($"Game State remains {State} because move {piece.name} {piece.Tile.name} to {toTile.name} failed");
        ResolveGameState(State);
    }
    public void NewGameState(GameState newState)
    {
        ResolveGameState(newState);
    }
    private void ResolveGameState(GameState state)
    {
        switch (state)
        {
            case GameState.WHITETURN:
                ResolveWhiteTurn();
                break;
            case GameState.BLACKTURN:
                ResolveBlackTurn();
                break;
            case GameState.BLACKCHECKED:
                break;
            case GameState.WHITECHECKED:
                break;
            case GameState.CHECKMATE:
                ResolveCheckMate();
                break;
        }
    }

    private void ResolveCheckMate()
    {
        BoardManager.Instance.DisableAllPieces();
        _gameoverObject.SetActive(true);
    }

    private void ResolveWhiteTurn()
    {
        
    }

    private void ResolveBlackTurn()
    {
        _ai.MakeMove(aiReplay);
    }
}
