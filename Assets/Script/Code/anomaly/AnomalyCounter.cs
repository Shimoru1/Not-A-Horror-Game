using UnityEngine;

public class AnomalyCounter : MonoBehaviour
{
    [Header("Anomaly Settings")]
    public int maxAnomaly = 5;

    [Header("Game Time")]
    public GameTime gameTime;

    private int currentAnomaly = 0;
    private bool gameOver = false;

    // เรียกเมื่อ Anomaly เกิด
    public void AnomalySpawned()
    {
        if (gameOver)
            return;

        currentAnomaly++;

        Debug.Log("Anomaly: " + currentAnomaly + "/" + maxAnomaly);

        // ถ้ามีตัวที่ 6 = แพ้ทันที
        if (currentAnomaly > maxAnomaly)
        {
            LoseGame();
        }
    }

    // เรียกเมื่อ Anomaly ถูกกำจัด
    public void AnomalyRemoved()
    {
        if (gameOver)
            return;

        currentAnomaly--;

        if (currentAnomaly < 0)
            currentAnomaly = 0;

        Debug.Log("Anomaly: " + currentAnomaly + "/" + maxAnomaly);
    }

    private void LoseGame()
    {
        gameOver = true;

        // หยุดเวลา
        gameTime.StopTime();

        Debug.Log("GAME OVER! Too many anomalies!");
    }

    public int GetCurrentAnomaly()
    {
        return currentAnomaly;
    }
}