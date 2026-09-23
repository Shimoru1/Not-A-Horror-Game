using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public Image tutorialImage;
    public Sprite[] tutorialPages;

    [Header("Buttons")]
    public Button backButton;
    public Button nextButton;
    public Button closeButton;

    [Header("Player Control")]
    public MonoBehaviour[] playerControls;

    private int currentPage = 0;

    void Start()
    {
        tutorialPanel.SetActive(true);

        currentPage = 0;

        // ปิดการควบคุม Player ตอน Tutorial เปิด
        SetPlayerControl(false);

        // แสดงเมาส์
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // ล้าง OnClick เดิม ป้องกันกดแล้วข้าม
        nextButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // เชื่อมปุ่ม
        nextButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(PreviousPage);
        closeButton.onClick.AddListener(CloseTutorial);

        UpdateTutorial();
    }

    void UpdateTutorial()
    {
        if (tutorialPages.Length == 0)
            return;

        tutorialImage.sprite = tutorialPages[currentPage];

        backButton.interactable = currentPage > 0;

        nextButton.interactable =
            currentPage < tutorialPages.Length - 1;
    }

    public void NextPage()
    {
        if (currentPage < tutorialPages.Length - 1)
        {
            currentPage++;
            UpdateTutorial();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateTutorial();
        }
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);

        // เปิดการควบคุม Player กลับมา
        SetPlayerControl(true);

        // ล็อกเมาส์กลับเข้าเกม
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SetPlayerControl(bool state)
    {
        foreach (MonoBehaviour control in playerControls)
        {
            if (control != null)
            {
                control.enabled = state;
            }
        }
    }
}