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

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        resultShown = false;
    }

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

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    public void LeaveGame() 
    {
		SceneManager.LoadScene("MainMenu");
	}
}
