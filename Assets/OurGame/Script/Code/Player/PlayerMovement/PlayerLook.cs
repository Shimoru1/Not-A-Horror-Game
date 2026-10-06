using UnityEngine;

public class PlayerLook : MonoBehaviour
{
	[Header("References")]
	public Transform orientation;
	public Transform cameraTransform;
	public CCTVManager cctvManager;

	[Header("Mouse Settings")]
	public float mouseSensitivity = 2.5f;

	[Header("Vertical Limits")]
	public float lookUpLimit = 80f;
	public float lookDownLimit = 80f;

	private float verticalRotation = 0f;


	// =========================================================
	// START
	// =========================================================

	private void Start()
	{
		if (cctvManager == null)
		{
			cctvManager = FindFirstObjectByType<CCTVManager>();
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}


	// =========================================================
	// UPDATE
	// =========================================================

	private void Update()
	{
		// ==============================
		// PAUSE
		// ==============================

		if (PauseMenu.Instance != null &&
			PauseMenu.Instance.IsPaused)
		{
			return;
		}

		// ==============================
		// CCTV
		// ==============================

		if (cctvManager != null &&
			cctvManager.isWatchingCCTV)
		{
			return;
		}

		LookAround();
	}


	// =========================================================
	// LOOK AROUND
	// =========================================================

	private void LookAround()
	{
		// Mouse X = หันซ้าย / ขวา
		// Mouse Y = มองขึ้น / ลง

		float mouseX =
			Input.GetAxisRaw("MouseX") *
			mouseSensitivity;

		float mouseY =
			Input.GetAxisRaw("MouseY") *
			mouseSensitivity;


		// =====================================================
		// หันซ้าย / ขวา
		// =====================================================

		if (orientation != null)
		{
			orientation.Rotate(
				Vector3.up * mouseX,
				Space.Self
			);
		}


		// =====================================================
		// มองขึ้น / ลง
		// =====================================================

		verticalRotation -= mouseY;

		verticalRotation = Mathf.Clamp(
			verticalRotation,
			-lookUpLimit,
			lookDownLimit
		);


		if (cameraTransform != null)
		{
			cameraTransform.localRotation =
				Quaternion.Euler(
					verticalRotation,
					0f,
					0f
				);
		}
	}
}
