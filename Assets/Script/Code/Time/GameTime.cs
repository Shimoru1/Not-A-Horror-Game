using UnityEngine;

public class GameTime : MonoBehaviour
{
    [Header("Game Time")]

    // เวลาในเกม
    // 0 = 00:00
    // 6 = 06:00
    public float currentTime = 0f;

    [Tooltip("จำนวนวินาทีจริงที่ใช้ต่อ 1 ชั่วโมงในเกม")]
    public float secondsPerHour = 50f;

    private bool gameEnded = false;

    private void Update()
    {
        // ถ้าเกมจบแล้ว ไม่ต้องเดินเวลา
        if (gameEnded)
            return;

        // เดินเวลา
        currentTime += Time.deltaTime / secondsPerHour;

        // ถึง 06:00
        if (currentTime >= 6f)
        {
            currentTime = 6f;

            WinGame();
        }
    }

    private void WinGame()
    {
        gameEnded = true;

        Debug.Log("YOU WIN! ได้เวลากลับบ้านแล้ว!");
    }

    // เรียกใช้เมื่ออยากหยุดเวลา
    public void StopTime()
    {
        gameEnded = true;

        Debug.Log("TIME STOPPED!");
    }

    // คืนค่าเวลาเป็นรูปแบบ 00:00
    public string GetGameTime()
    {
        int hour = Mathf.FloorToInt(currentTime);

        int minute = Mathf.FloorToInt(
            (currentTime - hour) * 60f
        );

        return hour.ToString("00") + ":" + minute.ToString("00");
    }
}