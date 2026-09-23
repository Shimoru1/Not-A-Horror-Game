using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Playing,
        Win,
        GameOver
    }

    public enum GameOverReason
    {
        None,
        TooManyAnomalies,
        OutOfAmmo
    }

    [Header("GAME STATE")]
    [SerializeField]
    private GameState currentState = GameState.Playing;

    [Header("GAME OVER REASON")]
    [SerializeField]
    private GameOverReason gameOverReason = GameOverReason.None;

    [Header("TEST")]
    public bool showDebugLog = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ==========================================
    // WIN
    // ==========================================

    public void WinGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Win;
        gameOverReason = GameOverReason.None;

        if (showDebugLog)
        {
            Debug.Log("================================");
            Debug.Log("YOU WIN!");
            Debug.Log("================================");
        }
    }

    // ==========================================
    // GAME OVER - Too Many Anomalies
    // ==========================================

    public void GameOver()
    {
        GameOver(GameOverReason.TooManyAnomalies);
    }

    // ==========================================
    // GAME OVER - With Reason
    // ==========================================

    public void GameOver(GameOverReason reason)
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.GameOver;
        gameOverReason = reason;

        if (showDebugLog)
        {
            Debug.Log("================================");
            Debug.Log("GAME OVER!");
            Debug.Log("Reason: " + reason);
            Debug.Log("================================");
        }
    }

    // ==========================================
    // GET GAME STATE
    // ==========================================

    public GameState GetGameState()
    {
        return currentState;
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    public bool IsWin()
    {
        return currentState == GameState.Win;
    }

    public bool IsGameOver()
    {
        return currentState == GameState.GameOver;
    }

    // ==========================================
    // GET GAME OVER REASON
    // ==========================================

    public GameOverReason GetGameOverReason()
    {
        return gameOverReason;
    }
}
