using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10000)]
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

        // ปิดการควบคุม Player
        SetPlayerControl(false);

        // เปิดเมาส์
        UnlockMouse();

        // ล้างปุ่มเดิม
        nextButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        // เชื่อมปุ่ม
        nextButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(PreviousPage);
        closeButton.onClick.AddListener(CloseTutorial);

        UpdateTutorial();
    }

    void Update()
    {
        if (tutorialPanel != null && tutorialPanel.activeSelf)
        {
            UnlockMouse();
        }
    }

    void LateUpdate()
    {
        // สำคัญมาก
        // ทำหลังจาก Camera / Player Script ทำงานเสร็จ
        if (tutorialPanel != null && tutorialPanel.activeSelf)
        {
            UnlockMouse();
        }
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UpdateTutorial()
    {
        if (tutorialPages == null || tutorialPages.Length == 0)
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

        UnlockMouse();
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateTutorial();
        }

        UnlockMouse();
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);

        // เปิด Player กลับมา
        SetPlayerControl(true);

        // เข้าเกมจริง
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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