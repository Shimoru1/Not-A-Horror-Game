using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CCTVManager : MonoBehaviour
{
    [Header("CCTV Settings")]
    public bool isWatchingCCTV = false;

    [Header("CCTV Cameras")]
    public GameObject[] cctvCameras;

    [Header("Room Settings")]
    public string[] roomNames;
    public TextMeshProUGUI roomNameText;

    [Header("Weapon")]
    public WeaponController cameraWeapon;

	[Header("CCTV UI")]
	public GameObject cctvUI;

	// เก็บสถานะกล้องก่อนเข้า CCTV
	private Dictionary<Camera, bool> cameraStates =
        new Dictionary<Camera, bool>();

    private int currentCameraIndex = 0;


    private void Start()
    {
        DisableAllCCTVCameras();

        // เริ่มเกมโดยออกจาก CCTV
        isWatchingCCTV = false;

		if (cctvUI != null)
		{
			cctvUI.SetActive(false);
		}

		LockMouse();

		Debug.Log("CCTV Manager Ready");
    }


    private void Update()
    {
		if (GameManager.Instance != null &&
		!GameManager.Instance.IsPlaying())
		{
			return;
		}

		// กด E เพื่อเข้า / ออกจาก CCTV
		if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("กด E แล้ว!");

            ToggleCCTV();
        }

        // ถ้าอยู่ใน CCTV ให้เปลี่ยนกล้องได้
        if (isWatchingCCTV)
        {
            HandleCameraSwitching();
        }
    }
	private void LockMouse()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}


	private void ToggleCCTV()
    {
        if (isWatchingCCTV)
        {
            ExitCCTV();
        }
        else
        {
            EnterCCTV();
        }
    }


    // =========================================================
    // ENTER CCTV
    // =========================================================

    private void EnterCCTV()
    {
        Debug.Log("ENTER CCTV");

        isWatchingCCTV = true;

        // จำสถานะของกล้องทั้งหมดก่อนเข้า CCTV
        SaveCameraStates();

        // ปิดกล้องปกติทั้งหมด
        DisableNormalCameras();

        // เปิดกล้อง CCTV ปัจจุบัน
        ActivateCurrentCamera();

		if (cctvUI != null)
		{
			cctvUI.SetActive(true);
		}

		// เปิดปืน
		if (cameraWeapon != null)
        {
            cameraWeapon.EnableWeapon();
        }
    }


    // =========================================================
    // EXIT CCTV
    // =========================================================

    private void ExitCCTV()
    {
        Debug.Log("EXIT CCTV");

        isWatchingCCTV = false;

        // ปิด CCTV ทั้งหมด
        DisableAllCCTVCameras();

		if (cctvUI != null)
		{
			cctvUI.SetActive(false);
		}

		// ปิดปืน
		if (cameraWeapon != null)
        {
            cameraWeapon.DisableWeapon();
        }

        // เปิดกล้องเดิมกลับมา
        RestoreCameraStates();
    }


    // =========================================================
    // SAVE CAMERA STATES
    // =========================================================

    private void SaveCameraStates()
    {
        cameraStates.Clear();

        Camera[] allCameras =
            FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Camera cam in allCameras)
        {
            if (cam == null)
                continue;

            // ไม่ต้องจำ CCTV Camera
            if (IsCCTVCamera(cam.gameObject))
                continue;

            cameraStates[cam] = cam.enabled;
        }
    }


    // =========================================================
    // DISABLE NORMAL CAMERAS
    // =========================================================

    private void DisableNormalCameras()
    {
        Camera[] allCameras =
            FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Camera cam in allCameras)
        {
            if (cam == null)
                continue;

            // ห้ามปิด CCTV ตรงนี้
            if (IsCCTVCamera(cam.gameObject))
                continue;

            cam.enabled = false;
        }
    }


    // =========================================================
    // RESTORE NORMAL CAMERAS
    // =========================================================

    private void RestoreCameraStates()
    {
        foreach (KeyValuePair<Camera, bool> pair in cameraStates)
        {
            if (pair.Key != null)
            {
                pair.Key.enabled = pair.Value;
            }
        }

        cameraStates.Clear();
    }


    // =========================================================
    // CAMERA SWITCHING
    // =========================================================

    private void HandleCameraSwitching()
    {
        if (cctvCameras == null || cctvCameras.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchCamera(-1);
        }

        else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchCamera(1);
        }
    }


    private void SwitchCamera(int direction)
    {
        if (cctvCameras == null ||
            cctvCameras.Length == 0)
            return;

        // ปิดกล้องปัจจุบัน
        if (cctvCameras[currentCameraIndex] != null)
        {
            cctvCameras[currentCameraIndex].SetActive(false);
        }

        // เปลี่ยน Index
        currentCameraIndex += direction;

        // วนกลับเมื่อถึงสุด
        if (currentCameraIndex >= cctvCameras.Length)
        {
            currentCameraIndex = 0;
        }

        else if (currentCameraIndex < 0)
        {
            currentCameraIndex =
                cctvCameras.Length - 1;
        }

        // เปิดกล้องใหม่
        ActivateCurrentCamera();
    }


    // =========================================================
    // ACTIVATE CURRENT CCTV
    // =========================================================

    private void ActivateCurrentCamera()
    {
        if (cctvCameras == null ||
            cctvCameras.Length == 0)
            return;

        if (currentCameraIndex >= cctvCameras.Length)
            return;

        GameObject currentCamera =
            cctvCameras[currentCameraIndex];

        if (currentCamera != null)
        {
            currentCamera.SetActive(true);

            UpdateCameraUI();
        }
    }


    // =========================================================
    // DISABLE ALL CCTV
    // =========================================================

    private void DisableAllCCTVCameras()
    {
        if (cctvCameras == null)
            return;

        foreach (GameObject cam in cctvCameras)
        {
            if (cam != null)
            {
                cam.SetActive(false);
            }
        }
    }


    // =========================================================
    // CHECK CCTV CAMERA
    // =========================================================

    private bool IsCCTVCamera(GameObject obj)
    {
        if (cctvCameras == null)
            return false;

        foreach (GameObject cam in cctvCameras)
        {
            if (cam == obj)
                return true;
        }

        return false;
    }

    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }

    // =========================================================
    // UPDATE ROOM UI
    // =========================================================

    private void UpdateCameraUI()
    {
        if (roomNameText == null)
            return;

        if (roomNames == null)
            return;

        if (currentCameraIndex >= roomNames.Length)
            return;

        roomNameText.text =
            roomNames[currentCameraIndex];
    }
}