using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public enum GameState
	{
		Starting,
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
	private GameState currentState = GameState.Starting;

	[Header("GAME OVER REASON")]
	[SerializeField]
	private GameOverReason gameOverReason = GameOverReason.None;

	[Header("GAME START")]
	[Tooltip("เวลารอก่อนเริ่มเกม")]
	[SerializeField] private float gameStartDelay = 60f;

	[Tooltip("เริ่มแสดงข้อความก่อนเกมเริ่มกี่วินาที")]
	[SerializeField] private float warningTime = 5f;

	[Tooltip("ติ๊กเพื่อข้ามเวลารอ 1 นาทีสำหรับการ Test")]
	[SerializeField] private bool isTest = false;

	[Tooltip("Text ที่แสดงตอนใกล้เริ่มเกม")]
	[SerializeField] private TextMeshProUGUI startText;

	[Header("START TEXT FADE")]
	[SerializeField] private CanvasGroup startTextCanvasGroup;
	[SerializeField] private float fadeInDuration = 1f;
	[SerializeField] private float fadeOutDuration = 1f;

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
	// START
	// =========================================================

	private void Start()
	{
		// ซ่อน Text ก่อน
		if (startText != null)
		{
			startText.gameObject.SetActive(false);
		}

		// =====================================================
		// TEST MODE
		// =====================================================

		if (isTest)
		{
			StartGameImmediately();
			return;
		}

		// =====================================================
		// NORMAL MODE
		// =====================================================

		currentState = GameState.Starting;

		StartCoroutine(GameStartCountdown());
	}

	// =========================================================
	// GAME START COUNTDOWN
	// =========================================================

	private System.Collections.IEnumerator GameStartCountdown()
	{
		float remainingTime = gameStartDelay;

		// ซ่อน Text ตอนเริ่ม
		if (startText != null)
		{
			startText.gameObject.SetActive(false);
		}

		if (startTextCanvasGroup != null)
		{
			startTextCanvasGroup.alpha = 0f;
		}

		if (showDebugLog)
		{
			Debug.Log("GAME STARTING...");
			Debug.Log("Game will start in " + gameStartDelay + " seconds.");
		}

		// =====================================================
		// รอจนเหลือ 5 วินาที
		// =====================================================

		while (remainingTime > warningTime)
		{
			remainingTime -= Time.deltaTime;
			yield return null;
		}

		// =====================================================
		// แสดง Text
		// =====================================================

		if (startText != null)
		{
			startText.gameObject.SetActive(true);
			startText.text = "Wait.. what was that..";
		}

		// =====================================================
		// FADE IN
		// =====================================================

		if (startTextCanvasGroup != null)
		{
			float timer = 0f;

			while (timer < fadeInDuration)
			{
				timer += Time.deltaTime;

				startTextCanvasGroup.alpha =
					Mathf.Clamp01(timer / fadeInDuration);

				yield return null;
			}

			startTextCanvasGroup.alpha = 1f;
		}

		// =====================================================
		// รอจนถึงเวลาเริ่มเกม
		// =====================================================

		while (remainingTime > 0f)
		{
			remainingTime -= Time.deltaTime;
			yield return null;
		}

		// =====================================================
		// FADE OUT
		// =====================================================

		if (startTextCanvasGroup != null)
		{
			float timer = 0f;
			float startAlpha = startTextCanvasGroup.alpha;

			while (timer < fadeOutDuration)
			{
				timer += Time.deltaTime;

				startTextCanvasGroup.alpha =
					Mathf.Lerp(
						startAlpha,
						0f,
						timer / fadeOutDuration
					);

				yield return null;
			}

			startTextCanvasGroup.alpha = 0f;
		}

		// =====================================================
		// เริ่มเกม
		// =====================================================

		StartGameImmediately();
	}

	// =========================================================
	// START GAME
	// =========================================================

	private void StartGameImmediately()
	{
		currentState = GameState.Playing;

		Time.timeScale = 1f;

		if (startText != null)
		{
			startText.gameObject.SetActive(false);
		}

		if (startTextCanvasGroup != null)
		{
			startTextCanvasGroup.alpha = 0f;
		}

		if (showDebugLog)
		{
			Debug.Log("================================");
			Debug.Log("GAME STARTED!");
			Debug.Log("ALL GAME SYSTEMS ARE ACTIVE!");
			Debug.Log("================================");
		}
	}

	// =========================================================
	// PAUSE
	// =========================================================

	public void PauseGame()
	{
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
	// CHECK STARTING
	// =========================================================

	public bool IsStarting()
	{
		return currentState == GameState.Starting;
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
