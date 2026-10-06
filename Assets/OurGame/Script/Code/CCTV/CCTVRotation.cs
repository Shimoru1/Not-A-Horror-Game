using UnityEngine;

public class CCTVRotation : MonoBehaviour
{
	[Header("Mouse Sensitivity")]
	public float mouseSensitivity = 2.5f;

	[Header("Rotation Limits")]
	public float horizontalLimit = 80f;
	public float verticalUpLimit = 45f;
	public float verticalDownLimit = 45f;

	private float horizontalRotation = 0f;
	private float verticalRotation = 0f;

	private Vector3 startRotation;


	private void Awake()
	{
		startRotation = transform.localEulerAngles;

		horizontalRotation = 0f;
		verticalRotation = 0f;
	}


	private void OnEnable()
	{
		horizontalRotation = 0f;
		verticalRotation = 0f;

		// ไม่ Lock Mouse ที่นี่
		// ให้ PauseMenu / CCTVManager เป็นคนจัดการ
	}


	private void OnDisable()
	{
		// ไม่ Unlock Mouse ที่นี่
	}


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
		// GAME OVER / WIN
		// ==============================

		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
		{
			return;
		}

		if (!gameObject.activeInHierarchy)
			return;

		RotateCamera();
	}


	private void RotateCamera()
	{
		float mouseX =
			Input.GetAxis("MouseX") * mouseSensitivity;

		float mouseY =
			Input.GetAxis("MouseY") * mouseSensitivity;


		horizontalRotation += mouseX;

		horizontalRotation =
			Mathf.Clamp(
				horizontalRotation,
				-horizontalLimit,
				horizontalLimit
			);


		verticalRotation -= mouseY;

		verticalRotation =
			Mathf.Clamp(
				verticalRotation,
				-verticalUpLimit,
				verticalDownLimit
			);


		transform.localRotation =
			Quaternion.Euler(
				startRotation.x + verticalRotation,
				startRotation.y + horizontalRotation,
				startRotation.z
			);
	}
}
