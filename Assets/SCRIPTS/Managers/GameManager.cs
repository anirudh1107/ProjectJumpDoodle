using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    public static event Action<GameState> OnGameStateChanged;


    [Header("Scene Settings")]
    [SerializeField] private LevelManager levelManager;

    private GameState previousState;
    private float previousScore;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {   CurrentState = GameState.None;
        ChangeState(GameState.MainMenu);
        Debug.Log("Game started.");
    }

    public void ChangeState(GameState newState)
    {
        previousState = CurrentState;
        CurrentState = newState;
        
        // 1. Handle the specific logic for entering the state
        HandleStateTransition(newState);
        if (previousState != GameState.None)
            UnloadState(previousState);
        
        // 2. Broadcast the state change to the rest of the game
        OnGameStateChanged?.Invoke(newState);
    }

    private void HandleStateTransition(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                // If returning from a game, you would asynchronously unload the level scene here
                MainUIManager.Instance.ShowMainMenu();
                break;

            case GameState.GameStart:
                Time.timeScale = 1f;
                levelManager.Initialize();
                break;

            case GameState.Pause:
                Time.timeScale = 0f; // Freeze physics and time-dependent logic
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                previousScore = levelManager.GetCurrentScore();
                MainUIManager.Instance.UpdateGameOverScore(((int)previousScore));
                MainUIManager.Instance.ShowGameoverPanel();
                break;

            case GameState.Leaderboard:
                Time.timeScale = 1f;
                // Fetch high scores from server logic
                break;
        }
    }

     private void UnloadState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                MainUIManager.Instance.HideMainMenuPanel();
                break;

            case GameState.GameStart:
                break;

            case GameState.Pause:
                break;

            case GameState.GameOver:
                MainUIManager.Instance.HideGameoverPanel();
                // Calculate final score, save data, etc.
                break;

            // case GameState.Leaderboard:
            //     Time.timeScale = 1f;
            //     // Fetch high scores from server logic
            //     break;
        }
    }

    public void SetPreviousScore(float score)
    {
        previousScore = score;
    }

}
    

public enum GameState 
{ 
    None,
    MainMenu, 
    GameStart, // Represents playing Level 1
    Pause, 
    GameOver, 
    Leaderboard 
}

