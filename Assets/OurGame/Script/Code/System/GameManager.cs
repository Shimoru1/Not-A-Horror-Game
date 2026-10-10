using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	private GameState stateBeforePause = GameState.Playing;

	[Header("AUDIO")]
	[SerializeField] private AudioSource musicSource;
	[SerializeField] private AudioSource sfxSource;

	[Header("START / END MESSAGES")]
	[SerializeField] private TextMeshProUGUI waitingText;
	[SerializeField] private TextMeshProUGUI gameStartedText;

	[SerializeField] private string messageSoundId = "TaskSound";
	[SerializeField] private float messageDelay = 1f;

	private bool messagesHidden = false;
	private readonly Dictionary<TextMeshProUGUI, bool> desiredActive = new Dictionary<TextMeshProUGUI, bool>();

	private bool isMessageVisible = false;

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
    public bool IsTestMode => isTest;

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

		AutoAssignAudioSources();

		Time.timeScale = 1f;
	}

	// =========================================================
	// START
	// =========================================================

	private void Start()
	{
		Debug.Log(
			$"[GameManager] Start | isTest={isTest}, " +
			$"delay={gameStartDelay}, timeScale={Time.timeScale}"
		);

		if (startText != null)
			startText.gameObject.SetActive(false);

		if (startTextCanvasGroup != null)
			startTextCanvasGroup.alpha = 0f;

		if (isTest)
		{
			Debug.Log("[GameManager] Test mode: start immediately");

			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.StopMusic();
			}

			StartGameImmediately();

			StartCoroutine(ShowTestStartMessage());

			return;
		}

		currentState = GameState.Starting;

		Debug.Log("[GameManager] Starting countdown coroutine");

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
			startText.gameObject.SetActive(false);

		if (startTextCanvasGroup != null)
			startTextCanvasGroup.alpha = 0f;

		// เล่นเพลงระหว่างรอ
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayMusic("Gallery");
		}
		else
		{
			Debug.LogError("[GameManager] AudioManager.Instance is NULL!");
		}

		if (showDebugLog)
		{
			Debug.Log("GAME STARTING...");
			Debug.Log("Game will start in " + gameStartDelay + " seconds.");
		}

		// =====================================================
		// หลังเริ่มระบบ 1 วินาที: ขึ้นข้อความและเล่นเสียง
		// =====================================================

		yield return new WaitForSeconds(1f);

		ShowStartMessage(waitingText, true);

		SetMessageActive(startText, true);
		startText.text = "Explore The Gallery";

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX("TaskSound");
		}

		// =====================================================
		// รอจนเหลือ 5 วินาที (ระบบเดิม)
		// =====================================================

		remainingTime = Mathf.Max(0f, gameStartDelay - 1f);

		while (remainingTime > warningTime)
		{
			remainingTime -= Time.deltaTime;
			yield return null;
		}

		// =====================================================
		// เหลือ 5 วินาที: แสดง Text เดิม + Fade In
		// =====================================================

		SetMessageActive(waitingText, false);
		SetMessageActive(gameStartedText, false);

		if (startText != null)
		{
			startText.gameObject.SetActive(true);
			startText.text = "Wait.. what was that..";
		}

		if (AudioManager.Instance != null)
		{
			StartCoroutine(
				AudioManager.Instance.FadeOutMusic(warningTime)
			);
		}

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
		// รอจนหมดเวลา
		// =====================================================

		while (remainingTime > 0f)
		{
			remainingTime -= Time.deltaTime;
			yield return null;
		}

		// =====================================================
		// หมด 1 นาที: เปลี่ยนข้อความ + เล่นเสียงเดิม
		// =====================================================

		if (startText != null)
			startText.gameObject.SetActive(false);

		if (startTextCanvasGroup != null)
			startTextCanvasGroup.alpha = 0f;

		ShowStartMessage(gameStartedText, true);

		StartGameImmediately();
	}

	// =========================================================
	// START GAME
	// =========================================================

	private void StartGameImmediately()
	{
		currentState = GameState.Playing;
		Time.timeScale = 1f;

		if (showDebugLog)
		{
			Debug.Log("================================");
			Debug.Log("GAME STARTED!");
			Debug.Log("ALL GAME SYSTEMS ARE ACTIVE!");
			Debug.Log("================================");
		}
	}

	private System.Collections.IEnumerator FadeOutMusic(float duration)
	{
		if (musicSource == null)
			yield break;

		float startVolume = musicSource.volume;
		float t = 0f;

		while (t < duration)
		{
			t += Time.deltaTime;
			musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
			yield return null;
		}

		musicSource.Stop();
		musicSource.volume = startVolume;
	}

	private void PlayEndSound(string sfxId)
	{
		if (musicSource != null)
			musicSource.Stop();

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(sfxId);
		}
		else
		{
			Debug.LogWarning(
				"[GameManager] AudioManager.Instance is NULL!"
			);
		}
	}

	// =========================================================
	// PAUSE
	// =========================================================

	public void PauseGame()
	{
		// pause ได้ทั้งตอน Starting และ Playing
		if (currentState != GameState.Playing &&
			currentState != GameState.Starting)
			return;

		stateBeforePause = currentState;
		currentState = GameState.Paused;

		Time.timeScale = 0f;

		UnlockMouse();
		if (musicSource != null) musicSource.Pause();

		if (showDebugLog)
			Debug.Log("GAME PAUSED (from " + stateBeforePause + ")");
	}

	public void ResumeGame()
	{
		if (currentState != GameState.Paused)
			return;

		// กลับไปสถานะเดิม (Starting หรือ Playing)
		currentState = stateBeforePause;

		Time.timeScale = 1f;
		if (musicSource != null) musicSource.UnPause();

		if (showDebugLog)
			Debug.Log("GAME RESUMED (to " + currentState + ")");
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

		HideStartMessage();
		StopGame();
		PlayEndSound("WinGameSound");

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

		HideStartMessage();
		StopGame();
		PlayEndSound("LoseGameSound");


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

	private void AutoAssignAudioSources()
	{
		// ถ้าไม่ได้กำหนด Music Source ให้ค้นหาอัตโนมัติ
		if (musicSource == null)
		{
			AudioManager audioManager = AudioManager.Instance;

			if (audioManager != null)
			{
				musicSource = audioManager.GetComponent<AudioSource>();
			}

			if (musicSource == null)
			{
				Debug.LogWarning(
					"[GameManager] Music AudioSource is NULL. " +
					"Music will be controlled through AudioManager."
				);
			}
		}

		// ถ้าไม่ได้กำหนด SFX Source ให้ค้นหาอัตโนมัติ
		if (sfxSource == null)
		{
			AudioManager audioManager = AudioManager.Instance;

			if (audioManager != null)
			{
				AudioSource[] sources =
					audioManager.GetComponents<AudioSource>();

				if (sources.Length >= 2)
					sfxSource = sources[1];
			}

			if (sfxSource == null)
			{
				Debug.Log(
					"[GameManager] SFX AudioSource will use AudioManager."
				);
			}
		}
	}

	private void ShowStartMessage(TextMeshProUGUI targetText, bool playSound)
	{
		// ปิด Text ทั้งสองตัวก่อน
		SetMessageActive(waitingText, false);
		SetMessageActive(gameStartedText, false);
		if (targetText != null) SetMessageActive(targetText, true);

		isMessageVisible = true;

		if (playSound && AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(messageSoundId);
		}
	}

	private void HideStartMessage()
	{
		isMessageVisible = false;

		SetMessageActive(waitingText, false);
		SetMessageActive(gameStartedText, false);
		SetMessageActive(startText, false);
	}

	private System.Collections.IEnumerator ShowTestStartMessage()
	{
		yield return new WaitForSeconds(messageDelay);

		ShowStartMessage(gameStartedText, true);
	}
	public void SetStartTextVisible(bool visible)
	{
		SetMessageActive(startText, visible);
	}

	private void SetMessageActive(TextMeshProUGUI t, bool active)
	{
		if (t == null) return;

		if (messagesHidden)
		{
			desiredActive[t] = active;   // จดไว้ก่อน ยังไม่แสดง
			return;
		}

		t.gameObject.SetActive(active);
	}

	// เรียกตอนเข้า CCTV
	public void HideMessagesForCCTV()
	{
		if (messagesHidden) return;

		TextMeshProUGUI[] all = { waitingText, gameStartedText, startText };

		foreach (var t in all)
		{
			if (t == null) continue;
			desiredActive[t] = t.gameObject.activeSelf;
			t.gameObject.SetActive(false);
		}

		messagesHidden = true;
	}

	// เรียกตอนออกจาก CCTV
	public void RestoreMessagesAfterCCTV()
	{
		if (!messagesHidden) return;

		messagesHidden = false;

		foreach (var pair in desiredActive)
		{
			if (pair.Key != null)
				pair.Key.gameObject.SetActive(pair.Value);
		}

		desiredActive.Clear();
	}
}
