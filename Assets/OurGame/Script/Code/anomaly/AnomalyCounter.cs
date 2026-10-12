using UnityEngine;

public class AnomalyCounter : MonoBehaviour
{
	[Header("Anomaly Settings")]

	[Tooltip("ถึงจำนวนนี้แพ้ทันที")]
	public int maxAnomaly = 10;

	[Tooltip("ถึงจำนวนนี้เริ่มเตือน")]
	public int warningThreshold = 8;

	[Tooltip("จำนวน Anomaly ที่อยู่ในฉากตอนนี้")]
	[SerializeField]
	private int currentAnomaly = 0;


	[Header("Elimination Statistics")]

	[SerializeField]
	private int totalAnomaliesDefeated = 0;


	[Header("Game Time")]
	public GameTime gameTime;

	private bool gameOver = false;


	// ==================================================
	// SPAWNED
	// ==================================================

	public void AnomalySpawned()
	{
		if (gameOver)
			return;

		currentAnomaly++;

		Debug.Log(
			"ANOMALY SPAWNED → Current: " +
			currentAnomaly + "/" + maxAnomaly
		);

		if (currentAnomaly >= maxAnomaly)
			LoseGame();
	}


	// ==================================================
	// REMOVED (กลับเป็นปกติ / กำจัด / หายเอง)
	// defeated = true  → นับเป็นที่กำจัดได้
	// defeated = false → หายเอง ไม่นับสถิติ
	// ==================================================

	public void AnomalyRemoved(bool defeated = true)
	{
		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
			return;

		currentAnomaly = Mathf.Max(0, currentAnomaly - 1);

		if (defeated)
			totalAnomaliesDefeated++;

		Debug.Log(
			"ANOMALY REMOVED → Current: " +
			currentAnomaly + "/" + maxAnomaly +
			" | Defeated: " + totalAnomaliesDefeated
		);
	}


	// ==================================================
	// GAME OVER
	// ==================================================

	private void LoseGame()
	{
		if (gameOver)
			return;

		gameOver = true;

		if (gameTime != null)
			gameTime.StopTime();

		Debug.Log("GAME OVER! Too many anomalies!");

		if (GameManager.Instance != null)
		{
			GameManager.Instance.GameOver(
				GameManager.GameOverReason.TooManyAnomalies
			);
		}
	}


	// ==================================================
	// GETTERS
	// ==================================================

	public bool IsWarning()
	{
		return !gameOver && currentAnomaly >= warningThreshold;
	}

	public int GetCurrentAnomaly()
	{
		return currentAnomaly;
	}

	public int GetTotalAnomaliesDefeated()
	{
		return totalAnomaliesDefeated;
	}
}