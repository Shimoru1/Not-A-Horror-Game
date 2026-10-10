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

			// ถ้าเปิด Setting อยู่ ให้กลับไปหน้า Pause ก่อน
			if (IsPaused &&
				settingPanel != null &&
				settingPanel.activeSelf)
			{
				CloseSetting();
				return;
			}

			// ถ้าไม่ได้เปิด Setting ให้ทำงานตามปกติ
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

		// หยุดเสียงทั้งหมดระหว่าง Pause
		AudioListener.pause = true;

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

		AudioListener.pause = false;

		PlayButtonSound();

		IsPaused = false;

		if (GameManager.Instance != null)
			GameManager.Instance.ResumeGame();

		if (pausePanel != null)
			pausePanel.SetActive(false);

		if (settingPanel != null)
			settingPanel.SetActive(false);

		LockMouse();
		StartCoroutine(LockMouseNextFrame());

		Debug.Log("GAME RESUMED");
	}

	private System.Collections.IEnumerator LockMouseNextFrame()
	{
		// รอให้ Esc ถูกประมวลผลเสร็จก่อน แล้วล็อกซ้ำ
		yield return null;
		LockMouse();

		yield return null;
		LockMouse();
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
		AudioListener.pause = false;

		PlayButtonSound();

		Time.timeScale = 1f;
		IsPaused = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SceneManager.LoadScene("MainMenu");
	}

	private void PlayButtonSound()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX("ButtonSound1");
		}
		else
		{
			Debug.LogWarning(
				"[PauseMenu] AudioManager.Instance is NULL!"
			);
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
