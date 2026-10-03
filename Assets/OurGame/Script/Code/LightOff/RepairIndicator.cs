using UnityEngine;

public class RepairIndicator : MonoBehaviour
{
	public Transform target;
	public Camera playerCamera;

	public float screenEdgePadding = 80f;

	private void Update()
	{
		if (target == null ||
			playerCamera == null)
			return;

		Vector3 screenPos =
			playerCamera.WorldToScreenPoint(
				target.position
			);

		// เป้าหมายอยู่ด้านหลัง
		if (screenPos.z < 0)
		{
			screenPos.x =
				Screen.width - screenPos.x;

			screenPos.y =
				Screen.height - screenPos.y;
		}

		screenPos.x =
			Mathf.Clamp(
				screenPos.x,
				screenEdgePadding,
				Screen.width - screenEdgePadding
			);

		screenPos.y =
			Mathf.Clamp(
				screenPos.y,
				screenEdgePadding,
				Screen.height - screenEdgePadding
			);

		transform.position = screenPos;
	}
}
