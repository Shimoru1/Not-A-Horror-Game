using UnityEngine;
using UnityEngine.UI;

public class CCTVManager : MonoBehaviour
{
    [Header("CCTV Settings")]
    public bool isWatchingCCTV = false;

    [Header("CCTV Cameras")]
    [Tooltip("ลากกล้อง CCTV ทั้ง 5 ตัวมาใส่ใน Array นี้")]
    public GameObject[] cctvCameras;

    [Header("References")]
    [Tooltip("ใส่ Script WeaponController ของกล้องลงไปที่นี่")]
    public WeaponController cameraWeapon;

    [Tooltip("เชื่อมกับ Text UI เพื่อแสดงว่ากำลังดูกล้องเบอร์อะไร (เว้นว่างไว้ก่อนได้)")]
    public Text cameraIDText;

    private int currentCameraIndex = 0;

    void Start()
    {
        DisableAllCCTVCameras();
    }

    void Update()
    {
        // 1. ตรวจสอบการกดปุ่ม E เพื่อเข้า/ออก โหมดกล้อง
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleCCTV();
        }

        // 2. ถ้ายืนยันว่ากำลังดูกล้องอยู่ ให้สามารถกดสลับกล้องได้
        if (isWatchingCCTV)
        {
            HandleCameraSwitching();
        }
    }

    void ToggleCCTV()
    {
        isWatchingCCTV = !isWatchingCCTV;

        if (isWatchingCCTV)
        {
            Debug.Log("Enter CCTV Mode");
            if (cameraWeapon != null) cameraWeapon.EnableWeapon();
            ActivateCurrentCamera(); // เปิดกล้องตัวล่าสุด
        }
        else
        {
            Debug.Log("Exit CCTV Mode");
            if (cameraWeapon != null) cameraWeapon.DisableWeapon();
            DisableAllCCTVCameras(); // ปิดกล้องทั้งหมด
        }
    }

    void HandleCameraSwitching()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchCamera(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchCamera(1);
        }
    }

    void SwitchCamera(int direction)
    {
        if (cctvCameras.Length == 0) return;

        cctvCameras[currentCameraIndex].SetActive(false); // ปิดตัวเก่า

        currentCameraIndex += direction;

        if (currentCameraIndex >= cctvCameras.Length) currentCameraIndex = 0;
        else if (currentCameraIndex < 0) currentCameraIndex = cctvCameras.Length - 1;

        ActivateCurrentCamera(); // เปิดตัวใหม่
    }

    void ActivateCurrentCamera()
    {
        if (cctvCameras.Length > 0 && currentCameraIndex >= 0 && currentCameraIndex < cctvCameras.Length)
        {
            cctvCameras[currentCameraIndex].SetActive(true);
            UpdateCameraUI();
        }
    }

    void DisableAllCCTVCameras()
    {
        foreach (GameObject cam in cctvCameras)
        {
            if (cam != null) cam.SetActive(false);
        }
    }

    void UpdateCameraUI()
    {
        if (cameraIDText != null)
        {
            cameraIDText.text = "CAM 0" + (currentCameraIndex + 1);
        }
    }
}