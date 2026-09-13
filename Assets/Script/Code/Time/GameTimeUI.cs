using UnityEngine;
using TMPro;

public class GameTimeUI : MonoBehaviour
{
    public GameTime gameTime;
    public TMP_Text timeText;

    private void Update()
    {
        timeText.text = gameTime.GetGameTime();
    }
}