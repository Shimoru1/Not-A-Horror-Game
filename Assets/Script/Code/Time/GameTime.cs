using UnityEngine;

public class GameTime : MonoBehaviour
{
    [Header("Game Time")]
    public float currentTime = 0f;

    // 1 ชั่วโมงในเกม = 60 วินาทีจริง
    public float secondsPerHour = 60f;

    private bool gameEnded = false;

    private void Update()
    {
        if (gameEnded)
            return;

        currentTime += Time.deltaTime / secondsPerHour;

        // ถึง 6 โมงเช้า = ชนะ
        if (currentTime >= 6f)
        {
            currentTime = 6f;
            WinGame();
        }
    }

    private void WinGame()
    {
        gameEnded = true;

        Debug.Log("YOU WIN!");
    }

    // หยุดเวลา
    public void StopTime()
    {
        gameEnded = true;

        Debug.Log("TIME STOPPED!");
    }

    public string GetGameTime()
    {
        int hour = Mathf.FloorToInt(currentTime);

        return hour.ToString("00") + ":00";
    }
}