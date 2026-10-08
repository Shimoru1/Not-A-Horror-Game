using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("ระบบจะหา Player ให้อัตโนมัติ")]
    public Transform player;

    [Header("Prompt UI")]
    public TextMeshProUGUI promptText;

    [Header("Interaction Distance")]
    public float interactionDistance = 2.5f;


    private void Start()
    {
        // หา Player อัตโนมัติ
        if (player == null)
        {
            GameObject playerObject =
                GameObject.Find("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;

                Debug.Log(
                    "[InteractionPrompt] พบ Player แล้ว"
                );
            }
            else
            {
                Debug.LogWarning(
                    "[InteractionPrompt] หา GameObject ชื่อ Player ไม่เจอ!"
                );
            }
        }


        // ซ่อน Prompt ตอนเริ่ม
        if (promptText != null)
        {
            promptText.text = "[E] Fix";
            promptText.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if (player == null ||
            promptText == null)
        {
            return;
        }


        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );


        if (distance <= interactionDistance)
        {
            promptText.gameObject.SetActive(true);
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
}