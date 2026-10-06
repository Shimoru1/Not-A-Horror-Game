using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CCTVCleaningManager : MonoBehaviour
{
    [Header("Cleaning UI")]
    public GameObject cleaningPanel;
    public Image dirtImage;
    public Slider cleaningSlider;
    public TextMeshProUGUI cleaningText;
    public TextMeshProUGUI cameraNameText;

    [Header("Cleaning Settings")]
    public float cleaningSpeed = 35f;

    [Header("Player Control")]
    public MonoBehaviour[] playerControls;

    private CCTVCleaningTarget currentCamera;

    private float cleaningProgress;
    private bool isCleaning;

    public static bool IsCleaning { get; private set; }

    private void Start()
    {
        if (cleaningPanel != null)
            cleaningPanel.SetActive(false);

        if (cleaningSlider != null)
        {
            cleaningSlider.minValue = 0f;
            cleaningSlider.maxValue = 100f;
            cleaningSlider.value = 0f;
        }

        IsCleaning = false;
    }

    private void Update()
    {
        if (!isCleaning)
            return;

        if (Input.GetMouseButton(0))
        {
            cleaningProgress += cleaningSpeed * Time.deltaTime;

            cleaningProgress =
                Mathf.Clamp(cleaningProgress, 0f, 100f);

            UpdateCleaningUI();

            if (cleaningProgress >= 100f)
            {
                FinishCleaning();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitCleaning();
        }
    }

    public void StartCleaning(CCTVCleaningTarget camera)
    {
        if (camera == null)
            return;

        if (!camera.IsDirty())
            return;

        currentCamera = camera;

        cleaningProgress = 0f;
        isCleaning = true;
        IsCleaning = true;

        if (cleaningPanel != null)
            cleaningPanel.SetActive(true);

        if (cameraNameText != null)
        {
            cameraNameText.text =
                "Cleaning " + camera.gameObject.name;
        }

        if (dirtImage != null)
            dirtImage.fillAmount = 1f;

        UpdateCleaningUI();

        SetPlayerControl(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("เริ่มเช็ด " + camera.gameObject.name);
    }

    private void UpdateCleaningUI()
    {
        if (cleaningSlider != null)
            cleaningSlider.value = cleaningProgress;

        if (cleaningText != null)
        {
            cleaningText.text =
                "Cleaning " +
                Mathf.RoundToInt(cleaningProgress) +
                "%";
        }

        if (dirtImage != null)
        {
            dirtImage.fillAmount =
                1f - (cleaningProgress / 100f);
        }
    }

    private void FinishCleaning()
    {
        if (currentCamera != null)
        {
            currentCamera.CleanCamera();
        }

        Debug.Log("เช็ดกล้องเสร็จ!");

        ExitCleaning();
    }

    private void ExitCleaning()
    {
        isCleaning = false;
        IsCleaning = false;

        if (cleaningPanel != null)
            cleaningPanel.SetActive(false);

        currentCamera = null;

        SetPlayerControl(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerControls == null)
            return;

        foreach (MonoBehaviour control in playerControls)
        {
            if (control != null)
            {
                control.enabled = enabled;
            }
        }
    }
}