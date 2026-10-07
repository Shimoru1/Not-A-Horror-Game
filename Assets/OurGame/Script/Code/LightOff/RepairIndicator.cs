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

		if (screenPos.z < 0f)
		{
			// อยู่ด้านหลัง: ดูว่าอยู่ซ้ายหรือขวาของกล้อง
			Vector3 local =
				playerCamera.transform.InverseTransformPoint(
					target.position
				);

			// ติดขอบซ้าย/ขวาทันที (หันหลังตรง ๆ แล้ววาปข้าม)
			screenPos.x = local.x >= 0f
				? Screen.width - screenEdgePadding
				: screenEdgePadding;

			// ความสูงตาม station (กลับด้านเพราะอยู่หลังกล้อง)
			screenPos.y = Screen.height - screenPos.y;
		}

		screenPos.x = Mathf.Clamp(
			screenPos.x,
			screenEdgePadding,
			Screen.width - screenEdgePadding
		);

		screenPos.y = Mathf.Clamp(
			screenPos.y,
			screenEdgePadding,
			Screen.height - screenEdgePadding
		);

		screenPos.z = 0f;

		transform.position = screenPos;
	}
}