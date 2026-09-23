using UnityEngine;

public class AnomalyCounter : MonoBehaviour
{
    [Header("Anomaly Settings")]

    [Tooltip("จำนวน Anomaly สูงสุดที่อยู่ในฉากพร้อมกัน")]
    public int maxAnomaly = 6;

    [Tooltip("จำนวน Anomaly ที่อยู่ในฉากตอนนี้")]
    [SerializeField]
    private int currentAnomaly = 0;


    [Header("Elimination Statistics")]

    [Tooltip("จำนวน Anomaly ที่กำจัดได้ทั้งหมดในคืนนี้")]
    [SerializeField]
    private int totalAnomaliesDefeated = 0;


    [Header("Game Time")]

    public GameTime gameTime;


    private bool gameOver = false;


    // ==================================================
    // ANOMALY SPAWNED
    // ==================================================

    public void AnomalySpawned()
    {
        if (gameOver)
            return;


        currentAnomaly++;


        Debug.Log(
            "ANOMALY SPAWNED → Current: " +
            currentAnomaly +
            "/" +
            maxAnomaly
        );


        // 6/6 ยังเล่นต่อได้
        // ถ้าเกิน 6 ถึงจะ Game Over
        if (currentAnomaly > maxAnomaly)
        {
            LoseGame();
        }
    }


    // ==================================================
    // ANOMALY REMOVED
    // ==================================================

    public void AnomalyRemoved()
    {
        // ลดจำนวนที่อยู่ในฉาก
        currentAnomaly--;


        if (currentAnomaly < 0)
            currentAnomaly = 0;


        // ==============================================
        // เพิ่มจำนวนที่กำจัดได้ทั้งหมด
        // ==============================================

        totalAnomaliesDefeated++;


        Debug.Log(
            "ANOMALY ELIMINATED!"
        );


        Debug.Log(
            "Current Anomaly: " +
            currentAnomaly +
            "/" +
            maxAnomaly
        );


        Debug.Log(
            "Total Eliminated Tonight: " +
            totalAnomaliesDefeated
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


        // หยุดเวลา
        if (gameTime != null)
        {
            gameTime.StopTime();
        }


        Debug.Log(
            "GAME OVER! Too many anomalies!"
        );


        // ส่งไป GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(
                GameManager.GameOverReason.TooManyAnomalies
            );
        }
    }


    // ==================================================
    // GET CURRENT ANOMALY
    // ==================================================

    public int GetCurrentAnomaly()
    {
        return currentAnomaly;
    }


    // ==================================================
    // GET TOTAL ELIMINATED
    // ==================================================

    public int GetTotalAnomaliesDefeated()
    {
        return totalAnomaliesDefeated;
    }
}