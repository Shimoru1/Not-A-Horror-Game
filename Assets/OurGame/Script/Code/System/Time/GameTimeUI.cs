using UnityEngine;
using TMPro;

public class GameTimeUI : MonoBehaviour
{
    [Header("References")]

    public GameTime gameTime;

    public TMP_Text timeText;


    private void Awake()
    {
        // ถ้ายังไม่ได้ใส่ GameTime ใน Inspector
        // ให้ค้นหาให้อัตโนมัติ
        if (gameTime == null)
        {
            gameTime = FindFirstObjectByType<GameTime>();
        }

        // ถ้ายังไม่ได้ใส่ Text ใน Inspector
        // ให้ลองหา TMP_Text จากตัว GameObject นี้
        if (timeText == null)
        {
            timeText = GetComponent<TMP_Text>();
        }

        // ถ้ายังหาไม่เจอ ลองหาจากลูกของ GameObject
        if (timeText == null)
        {
            timeText = GetComponentInChildren<TMP_Text>();
        }
    }


    private void Update()
    {
        // ป้องกัน NullReferenceException
        if (gameTime == null)
            return;

        if (timeText == null)
            return;

        // แสดงเวลา
        timeText.text = "● REC " + gameTime.GetGameTime();
    }
}