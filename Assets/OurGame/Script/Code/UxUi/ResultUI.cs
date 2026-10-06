using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultUI : MonoBehaviour
{
	[Header("UI")]
	public GameObject resultPanel;
	public TMP_Text resultTitle;
	public TMP_Text resultMessage;

	[Header("Game Manager")]
	public GameManager gameManager;

	private bool resultShown = false;


	// =========================================================
	// START
	// =========================================================

	private void Start()
	{
		// สำคัญ:
		// ป้องกัน Time.timeScale = 0 จาก Scene ก่อนหน้า
		Time.timeScale = 1f;

		if (resultPanel != null)
		{
			resultPanel.SetActive(false);
		}

		resultShown = false;
	}


	// =========================================================
	// UPDATE
	// =========================================================

	private void Update()
	{
		if (gameManager == null)
			return;

		if (resultShown)
			return;

		if (gameManager.IsWin())
		{
			ShowWin();
		}
		else if (gameManager.IsGameOver())
		{
			ShowGameOver();
		}
	}


	// =========================================================
	// SHOW WIN
	// =========================================================

	private void ShowWin()
	{
		if (resultPanel == null)
			return;

		resultShown = true;

		resultPanel.SetActive(true);

		if (resultTitle != null)
			resultTitle.text = "YOU WIN!";

		if (resultMessage != null)
			resultMessage.text = "You survived until 06:00!";
	}


	// =========================================================
	// SHOW GAME OVER
	// =========================================================

	private void ShowGameOver()
	{
		if (resultPanel == null)
			return;

		resultShown = true;

		resultPanel.SetActive(true);

		if (resultTitle != null)
			resultTitle.text = "GAME OVER";

		if (resultMessage != null)
		{
			switch (gameManager.GetGameOverReason())
			{
				case GameManager.GameOverReason.OutOfAmmo:
					resultMessage.text = "Out of ammo!";
					break;

				case GameManager.GameOverReason.TooManyAnomalies:
					resultMessage.text = "Too many anomalies!";
					break;

				default:
					resultMessage.text = "Game Over!";
					break;
			}
		}
	}


	// =========================================================
	// RESTART GAME
	// =========================================================

	public void RestartGame()
	{
		// คืนเวลาให้เกมก่อนเปลี่ยน Scene
		Time.timeScale = 1f;

		Scene currentScene = SceneManager.GetActiveScene();

		SceneManager.LoadScene(currentScene.name);
	}


	// =========================================================
	// LEAVE GAME
	// =========================================================

	public void LeaveGame()
	{
		// คืนเวลาให้เกมก่อนเปลี่ยน Scene
		Time.timeScale = 1f;

		SceneManager.LoadScene("MainMenu");
	}
}
