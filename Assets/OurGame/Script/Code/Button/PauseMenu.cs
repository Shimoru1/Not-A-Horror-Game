using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	public static PauseMenu Instance;

	[Header("Pause UI")]
	[SerializeField] private GameObject pausePanel;

	[Header("Setting UI")]
	[SerializeField] private GameObject settingPanel;

	[Header("Button Sound")]
	[SerializeField] private AudioSource buttonClickAudio;

	public bool IsPaused { get; private set; } = false;


	// =========================================================
	// AWAKE
	// =========================================================

	private void Awake()
	{
		Instance = this;
	}


	// =========================================================
	// START
	// =========================================================

	private void Start()
	{
		Time.timeScale = 1f;

		IsPaused = false;

		if (pausePanel != null)
			pausePanel.SetActive(false);

		if (settingPanel != null)
			settingPanel.SetActive(false);

		LockMouse();
	}


	// =========================================================
	// UPDATE
	// =========================================================

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// ถ้าเกมจบแล้ว ไม่ให้ Pause
			if (GameManager.Instance != null &&
				(GameManager.Instance.IsWin() ||
				 GameManager.Instance.IsGameOver()))
			{
				return;
			}

			TogglePause();
		}
	}


	// =========================================================
	// TOGGLE
	// =========================================================

	public void TogglePause()
	{
		if (IsPaused)
		{
			ContinueGame();
		}
		else
		{
			PauseGame();
		}
	}


	// =========================================================
	// PAUSE
	// =========================================================

	public void PauseGame()
	{
		if (GameManager.Instance == null)
			return;

		if (!GameManager.Instance.IsPlaying() &&
			!GameManager.Instance.IsStarting())
			return;

		IsPaused = true;

		GameManager.Instance.PauseGame();

		if (pausePanel != null)
			pausePanel.SetActive(true);

		if (settingPanel != null)
			settingPanel.SetActive(false);

		UnlockMouse();

		Debug.Log("PAUSE MENU OPEN");
	}


	// =========================================================
	// CONTINUE
	// =========================================================

	public void ContinueGame()
	{
		if (!IsPaused)
			return;

		PlayButtonSound();

		IsPaused = false;

		if (GameManager.Instance != null)
		{
			GameManager.Instance.ResumeGame();
		}

		if (pausePanel != null)
			pausePanel.SetActive(false);

		if (settingPanel != null)
			settingPanel.SetActive(false);

		LockMouse();

		Debug.Log("GAME RESUMED");
	}



	// =========================================================
	// SETTING
	// =========================================================

	public void OpenSetting()
	{
		if (!IsPaused)
			return;

		PlayButtonSound();

		if (pausePanel != null)
			pausePanel.SetActive(false);

		if (settingPanel != null)
			settingPanel.SetActive(true);

		UnlockMouse();

		Debug.Log("SETTING OPEN");
	}



	// =========================================================
	// CLOSE SETTING
	// =========================================================

	public void CloseSetting()
	{
		if (!IsPaused)
			return;

		PlayButtonSound();

		if (settingPanel != null)
			settingPanel.SetActive(false);

		if (pausePanel != null)
			pausePanel.SetActive(true);

		UnlockMouse();

		Debug.Log("SETTING CLOSED");
	}



	// =========================================================
	// EXIT
	// =========================================================

	public void ExitGame()
	{
		PlayButtonSound();

		// คืนเวลา
		Time.timeScale = 1f;

		// ปิดสถานะ Pause
		IsPaused = false;

		// ปลดล็อก Mouse ก่อนเปลี่ยน Scene
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// โหลด Main Menu
		SceneManager.LoadScene("MainMenu");
	}

	private void PlayButtonSound()
	{
		if (buttonClickAudio != null)
		{
			buttonClickAudio.Play();
		}
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
