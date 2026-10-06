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

    [Header("Dirt System")]
    public CCTVCleaningTarget[] cleaningTargets;
    public CCTVDirtOverlay dirtOverlay;

    private Dictionary<Camera, bool> cameraStates =
        new Dictionary<Camera, bool>();

    private int currentCameraIndex = 0;

    [Header("Computer Interaction")]
    [SerializeField]
    private float computerInteractionDistance = 2.5f;

	[SerializeField]
	private GameObject eToUseUI;

	[Header("Power Off Screen")]
    public GameObject blackScreen;

    [Header("Repair UI")]
    public GameObject repairIcon;

    private Transform playerTransform;

    public static CCTVManager Instance;

    private bool powerOff = false;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        DisableAllCCTVCameras();

        isWatchingCCTV = false;

        if (cctvUI != null)
            cctvUI.SetActive(false);

		if (eToUseUI != null)
			eToUseUI.SetActive(false);

		LockMouse();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "CCTVManager: Player tag not found."
            );
        }

        Debug.Log("CCTV Manager Ready");
    }


    private void Update()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying())
        {
            return;
        }

        LockMouse();

		if (!isWatchingCCTV)
		{
			bool nearComputer = IsNearComputer();

			if (eToUseUI != null)
				eToUseUI.SetActive(nearComputer);
		}
		else
		{
			if (eToUseUI != null)
				eToUseUI.SetActive(false);
		}

		// ENTER / EXIT CCTV
		if (Input.GetKeyDown(KeyCode.E))
        {
            if (isWatchingCCTV)
            {
                ToggleCCTV();
            }
            else
            {
                if (IsNearComputer())
                {
                    Debug.Log(
                        "Press E near Computer -> Enter CCTV"
                    );

                    ToggleCCTV();
                }
                else
                {
                    Debug.Log(
                        "Press E, but player is not near Computer."
                    );
                }
            }
        }

        // CCTV CAMERA SWITCHING
        if (isWatchingCCTV)
        {
            if (powerOff)
                return;

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
    // COMPUTER INTERACTION
    // =========================================================

    private bool IsNearComputer()
    {
        if (playerTransform == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                return false;
            }
        }

        GameObject[] computers;

        try
        {
            computers =
                GameObject.FindGameObjectsWithTag("Computer");
        }
        catch (UnityException)
        {
            Debug.LogError(
                "CCTVManager: Computer tag not found."
            );

            return false;
        }

        foreach (GameObject computer in computers)
        {
            if (computer == null ||
                !computer.activeInHierarchy)
            {
                continue;
            }

            Collider col =
                computer.GetComponent<Collider>();

            Vector3 targetPosition =
                col != null
                ? col.bounds.center
                : computer.transform.position;

            float distance =
                Vector3.Distance(
                    playerTransform.position,
                    targetPosition
                );

            if (distance <= computerInteractionDistance)
            {
                return true;
            }
        }

        return false;
    }


    // =========================================================
    // ENTER CCTV
    // =========================================================

    private void EnterCCTV()
    {
        Debug.Log("ENTER CCTV");

        isWatchingCCTV = true;

        if (repairIcon != null)
            repairIcon.SetActive(false);

        SaveCameraStates();

        DisableNormalCameras();

        // POWER OFF
        if (powerOff)
        {
            ActivateCurrentCamera();

            if (blackScreen != null)
                blackScreen.SetActive(true);

            if (cctvUI != null)
                cctvUI.SetActive(true);

            if (cameraWeapon != null)
                cameraWeapon.DisableWeapon();

            return;
        }

        // POWER NORMAL
        ActivateCurrentCamera();

        if (cctvUI != null)
            cctvUI.SetActive(true);

        if (blackScreen != null)
            blackScreen.SetActive(false);

        if (cameraWeapon != null)
            cameraWeapon.EnableWeapon();
    }


    // =========================================================
    // EXIT CCTV
    // =========================================================

    private void ExitCCTV()
    {
        Debug.Log("EXIT CCTV");

        isWatchingCCTV = false;

        DisableAllCCTVCameras();

        if (cctvUI != null)
            cctvUI.SetActive(false);

        if (blackScreen != null)
            blackScreen.SetActive(false);

        if (cameraWeapon != null)
            cameraWeapon.DisableWeapon();

        if (dirtOverlay != null)
            dirtOverlay.SetCamera(null);

        RestoreCameraStates();

        if (repairIcon != null)
            repairIcon.SetActive(powerOff);
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
        foreach (KeyValuePair<Camera, bool> pair
                 in cameraStates)
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
        if (cctvCameras == null ||
            cctvCameras.Length == 0)
        {
            return;
        }

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
        {
            return;
        }

        if (cctvCameras[currentCameraIndex] != null)
        {
            cctvCameras[currentCameraIndex]
                .SetActive(false);
        }

        currentCameraIndex += direction;

        if (currentCameraIndex >= cctvCameras.Length)
        {
            currentCameraIndex = 0;
        }
        else if (currentCameraIndex < 0)
        {
            currentCameraIndex =
                cctvCameras.Length - 1;
        }

        ActivateCurrentCamera();
    }


    // =========================================================
    // ACTIVATE CURRENT CCTV
    // =========================================================

    private void ActivateCurrentCamera()
    {
        if (cctvCameras == null ||
            cctvCameras.Length == 0)
        {
            Debug.LogError("CCTV: No cameras.");
            return;
        }

        if (currentCameraIndex < 0 ||
            currentCameraIndex >= cctvCameras.Length)
        {
            Debug.LogError("CCTV: Invalid camera index.");
            return;
        }

        GameObject obj =
            cctvCameras[currentCameraIndex];

        if (obj == null)
        {
            Debug.LogError(
                "CCTV: Camera object is NULL."
            );

            return;
        }

        // Disable every CCTV camera
        for (int i = 0;
             i < cctvCameras.Length;
             i++)
        {
            if (cctvCameras[i] == null)
                continue;

            Camera otherCam =
                cctvCameras[i]
                    .GetComponent<Camera>();

            if (otherCam != null)
            {
                otherCam.enabled = false;
            }

            cctvCameras[i].SetActive(false);
        }

        // Enable current camera
        obj.SetActive(true);

        Camera currentCam =
            obj.GetComponent<Camera>();

        if (currentCam == null)
        {
            Debug.LogError(
                obj.name +
                " has no Camera Component!"
            );

            return;
        }

        currentCam.enabled = true;

        currentCam.targetDisplay = 0;


        // =====================================================
        // CONNECT CAMERA TO DIRT SYSTEM
        // =====================================================

        if (dirtOverlay != null)
        {
            if (cleaningTargets != null &&
                currentCameraIndex <
                cleaningTargets.Length)
            {
                CCTVCleaningTarget target =
                    cleaningTargets[currentCameraIndex];

                dirtOverlay.SetCamera(target);

                Debug.Log(
                    "Dirt connected to Camera " +
                    (currentCameraIndex + 1)
                );
            }
            else
            {
                dirtOverlay.SetCamera(null);
            }
        }


        Debug.Log(
            "Opened CCTV: " +
            obj.name +
            " | Camera Index = " +
            currentCameraIndex
        );

        UpdateCameraUI();
    }


    // =========================================================
    // DISABLE ALL CCTV
    // =========================================================

    private void DisableAllCCTVCameras()
    {
        if (cctvCameras == null)
            return;

        foreach (GameObject obj in cctvCameras)
        {
            if (obj == null)
                continue;

            Camera cam =
                obj.GetComponent<Camera>();

            if (cam != null)
            {
                cam.enabled = false;
            }

            obj.SetActive(false);
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


    // =========================================================
    // POWER OFF
    // =========================================================

    public void OnPowerOff()
    {
        Debug.Log("CCTV POWER OFF");

        powerOff = true;

        if (isWatchingCCTV)
        {
            ActivateCurrentCamera();

            if (blackScreen != null)
                blackScreen.SetActive(true);

            if (cameraWeapon != null)
                cameraWeapon.DisableWeapon();

            Debug.Log(
                "Power OFF -> CCTV still active but black screen."
            );
        }
    }


    // =========================================================
    // POWER RESTORED
    // =========================================================

    public void OnPowerRestored()
    {
        Debug.Log("CCTV POWER RESTORED");

        powerOff = false;

        if (blackScreen != null)
            blackScreen.SetActive(false);

        if (isWatchingCCTV)
        {
            ActivateCurrentCamera();

            if (cameraWeapon != null)
                cameraWeapon.EnableWeapon();

            Debug.Log(
                "CCTV power restored."
            );
        }
    }
}