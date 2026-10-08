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

    [Header("Dirt Spots")]
    public CCTVDirtSpot[] dirtSpots;

    [Header("Cleaning Settings")]
    [Range(1f, 100f)]
    public float requiredCleanPercent = 80f;

    [Header("Player Control")]
    public MonoBehaviour[] playerControls;

    private CCTVCleaningTarget currentCamera;

    private int cleanedSpots = 0;
    private int totalSpots = 0;

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
        isCleaning = false;
    }

    private void Update()
    {
        if (!isCleaning)
            return;

        // Keep the mouse available while cleaning.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Input.GetKeyDown(KeyCode.Escape))
            ExitCleaning();
    }

    public void StartCleaning(CCTVCleaningTarget camera)
    {
        if (camera == null)
        {
            Debug.LogWarning("⚠️ StartCleaning: camera is null.");
            return;
        }

        if (!camera.IsDirty())
        {
            Debug.Log("📺 กล้องยังสะอาดอยู่ ไม่ต้องทำความสะอาด");
            return;
        }

        currentCamera = camera;
        cleanedSpots = 0;

        // เปิด Panel ก่อน แล้วค่อยค้นหา Dirt Spot ที่อยู่ใน Panel
        if (cleaningPanel != null)
            cleaningPanel.SetActive(true);

        FindDirtSpots();

        // ต้องมีคราบอย่างน้อย 1 จุด ถึงจะเริ่มระบบเช็ด
        if (totalSpots <= 0)
        {
            Debug.LogWarning(
                "⚠️ Cleaning เริ่มไม่ได้: ไม่พบ CCTVDirtSpot ใน Cleaning Panel/Scene"
            );

            if (cleaningPanel != null)
                cleaningPanel.SetActive(false);

            currentCamera = null;
            return;
        }

        isCleaning = true;
        IsCleaning = true;

        if (cameraNameText != null)
            cameraNameText.text = "Cleaning " + camera.gameObject.name;

        if (dirtImage != null)
            dirtImage.fillAmount = 1f;

        ResetDirtSpots();
        UpdateCleaningUI();

        SetPlayerControl(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log(
            "🧽 เริ่ม Cleaning: " +
            camera.gameObject.name +
            " | คราบทั้งหมด = " +
            totalSpots
        );
    }

    private void FindDirtSpots()
    {
        if (dirtSpots != null && dirtSpots.Length > 0)
        {
            totalSpots = 0;

            foreach (CCTVDirtSpot spot in dirtSpots)
            {
                if (spot != null)
                    totalSpots++;
            }

            return;
        }

        // Auto-find รวมถึง Object ที่ inactive
        dirtSpots = FindObjectsByType<CCTVDirtSpot>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        totalSpots = 0;

        foreach (CCTVDirtSpot spot in dirtSpots)
        {
            if (spot != null)
                totalSpots++;
        }
    }

    private void ResetDirtSpots()
    {
        if (dirtSpots == null)
            return;

        foreach (CCTVDirtSpot spot in dirtSpots)
        {
            if (spot != null)
                spot.ResetSpot();
        }
    }

    public void CleanOneSpot(CCTVDirtSpot spot)
    {
        if (!isCleaning)
            return;

        if (spot == null)
            return;

        cleanedSpots++;

        UpdateCleaningUI();

        Debug.Log(
            "🧽 เช็ดคราบแล้ว " +
            cleanedSpots +
            "/" +
            totalSpots
        );

        float percent = GetCleaningPercent();

        if (percent >= requiredCleanPercent)
            FinishCleaning();
    }

    private float GetCleaningPercent()
    {
        if (totalSpots <= 0)
            return 0f;

        return ((float)cleanedSpots / totalSpots) * 100f;
    }

    private void UpdateCleaningUI()
    {
        float percent = GetCleaningPercent();

        if (cleaningSlider != null)
            cleaningSlider.value = percent;

        if (cleaningText != null)
        {
            cleaningText.text =
                "Cleaning " +
                Mathf.RoundToInt(percent) +
                "%";
        }

        if (dirtImage != null)
            dirtImage.fillAmount = 1f - (percent / 100f);
    }

    private void FinishCleaning()
    {
        if (currentCamera != null)
            currentCamera.CleanCamera();

        Debug.Log(
            "🧽 Cleaning สำเร็จ! " +
            Mathf.RoundToInt(GetCleaningPercent()) +
            "%"
        );

        ExitCleaning();
    }

    public void ExitCleaning()
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
            if (control == null)
                continue;

            // CCTVCleaningInteraction must stay enabled so F can still
            // exit Cleaning Mode while the player controls are disabled.
            if (control == this || control is CCTVCleaningInteraction)
                continue;

            control.enabled = enabled;
        }
    }
}
