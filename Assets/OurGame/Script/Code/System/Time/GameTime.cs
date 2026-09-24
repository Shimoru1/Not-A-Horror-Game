using UnityEngine;

public class GameTime : MonoBehaviour
{
    [Header("GAME TIME SETTING")]

    [Tooltip("เวลาเล่นจริงต่อ 1 เกม หน่วยเป็นวินาที")]
    public float gameDurationSeconds = 300f;

    [Tooltip("เวลาเริ่มต้นในเกม")]
    public float startGameHour = 0f;

    [Tooltip("เวลาที่ถือว่าชนะ")]
    public float endGameHour = 6f;


    [Header("CURRENT GAME TIME")]

    [Tooltip("เวลาปัจจุบันในเกม")]
    [SerializeField]
    private float currentTime = 0f;

    [Tooltip("เวลาปัจจุบันที่อ่านง่าย")]
    [SerializeField]
    private string currentGameTime = "00:00";


    [Header("TEST MODE")]

    [Tooltip("เปิดเพื่อทดสอบ Win ได้เร็วขึ้น")]
    public bool testMode = false;

    [Tooltip("เวลาจบเกมตอน Test Mode หน่วยเป็นวินาที")]
    public float testDurationSeconds = 30f;


    private bool gameEnded = false;


    // ==========================================
    // START
    // ==========================================

    private void Start()
    {
        currentTime = startGameHour;
        UpdateInspectorTime();
    }


    // ==========================================
    // UPDATE
    // ==========================================

    private void Update()
    {
        if (gameEnded)
            return;


        // เลือกว่าจะใช้เวลาเล่นจริงหรือ Test Mode
        float duration = testMode
            ? testDurationSeconds
            : gameDurationSeconds;


        // ป้องกันการใส่ค่า 0
        if (duration <= 0f)
            duration = 1f;


        // คำนวณว่า 1 ชั่วโมงในเกมใช้กี่วินาทีจริง
        float secondsPerGameHour =
            duration / (endGameHour - startGameHour);


        // เดินเวลา
        currentTime +=
            Time.deltaTime / secondsPerGameHour;


        // จำกัดไม่ให้เกินเวลาจบ
        if (currentTime >= endGameHour)
        {
            currentTime = endGameHour;

            UpdateInspectorTime();

            WinGame();

            return;
        }


        UpdateInspectorTime();
    }


    // ==========================================
    // UPDATE INSPECTOR
    // ==========================================

    private void UpdateInspectorTime()
    {
        currentGameTime = GetGameTime();
    }


    // ==========================================
    // WIN
    // ==========================================

    private void WinGame()
    {
        gameEnded = true;

		Debug.Log("================================");
        Debug.Log("YOU WIN!");
        Debug.Log("Game Time Reached: " + GetGameTime());
        Debug.Log("================================");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }


    // ==========================================
    // STOP TIME
    // ==========================================

    public void StopTime()
    {
        gameEnded = true;

        Debug.Log("TIME STOPPED!");
    }


    // ==========================================
    // GET GAME TIME
    // ==========================================

    public string GetGameTime()
    {
        int hour = Mathf.FloorToInt(currentTime);

        int minute =
            Mathf.FloorToInt(
                (currentTime - hour) * 60f
            );

        return hour.ToString("00")
            + ":"
            + minute.ToString("00");
    }


    // ==========================================
    // GET CURRENT TIME VALUE
    // ==========================================

    public float GetCurrentTime()
    {
        return currentTime;
    }


    // ==========================================
    // CHECK GAME ENDED
    // ==========================================

    public bool IsGameEnded()
    {
        return gameEnded;
    }
}