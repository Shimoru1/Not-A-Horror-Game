using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public enum GameState
	{
		Playing,
		Paused,
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


	// =========================================================
	// AWAKE
	// =========================================================

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		Time.timeScale = 1f;
	}


	// =========================================================
	// PAUSE
	// =========================================================

	public void PauseGame()
	{
		// Pause ได้เฉพาะตอนกำลังเล่น
		if (currentState != GameState.Playing)
			return;

		currentState = GameState.Paused;

		Time.timeScale = 0f;

		UnlockMouse();

		if (showDebugLog)
		{
			Debug.Log("GAME PAUSED");
		}
	}


	// =========================================================
	// RESUME
	// =========================================================

	public void ResumeGame()
	{
		// Resume ได้เฉพาะตอน Pause
		if (currentState != GameState.Paused)
			return;

		currentState = GameState.Playing;

		Time.timeScale = 1f;

		if (showDebugLog)
		{
			Debug.Log("GAME RESUMED");
		}
	}


	// =========================================================
	// WIN
	// =========================================================

	public void WinGame()
	{
		if (currentState != GameState.Playing)
			return;

		currentState = GameState.Win;
		gameOverReason = GameOverReason.None;

		StopGame();

		if (showDebugLog)
		{
			Debug.Log("================================");
			Debug.Log("YOU WIN!");
			Debug.Log("GAME STOPPED!");
			Debug.Log("================================");
		}
	}


	// =========================================================
	// GAME OVER
	// =========================================================

	public void GameOver()
	{
		GameOver(GameOverReason.TooManyAnomalies);
	}


	// =========================================================
	// GAME OVER WITH REASON
	// =========================================================

	public void GameOver(GameOverReason reason)
	{
		if (currentState != GameState.Playing)
			return;

		currentState = GameState.GameOver;
		gameOverReason = reason;

		StopGame();

		if (showDebugLog)
		{
			Debug.Log("================================");
			Debug.Log("GAME OVER!");
			Debug.Log("Reason: " + reason);
			Debug.Log("GAME STOPPED!");
			Debug.Log("================================");
		}
	}


	// =========================================================
	// STOP GAME
	// =========================================================

	private void StopGame()
	{
		Time.timeScale = 0f;

		UnlockMouse();
	}


	// =========================================================
	// GET GAME STATE
	// =========================================================

	public GameState GetGameState()
	{
		return currentState;
	}


	// =========================================================
	// CHECK PLAYING
	// =========================================================

	public bool IsPlaying()
	{
		return currentState == GameState.Playing;
	}


	// =========================================================
	// CHECK PAUSED
	// =========================================================

	public bool IsPaused()
	{
		return currentState == GameState.Paused;
	}


	// =========================================================
	// CHECK WIN
	// =========================================================

	public bool IsWin()
	{
		return currentState == GameState.Win;
	}


	// =========================================================
	// CHECK GAME OVER
	// =========================================================

	public bool IsGameOver()
	{
		return currentState == GameState.GameOver;
	}


	// =========================================================
	// GET GAME OVER REASON
	// =========================================================

	public GameOverReason GetGameOverReason()
	{
		return gameOverReason;
	}


	// =========================================================
	// MOUSE
	// =========================================================

	public void UnlockMouse()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}


	public void LockMouse()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}
