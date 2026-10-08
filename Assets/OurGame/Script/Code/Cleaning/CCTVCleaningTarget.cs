using UnityEngine;

public class CCTVCleaningTarget : MonoBehaviour
{
	// 0 = สะอาด, 1 = เปื้อนเต็มที่
	private float dirtPercent = 0f;

	public bool IsDirty() => dirtPercent > 0f;

	public float GetDirtPercent() => dirtPercent;

	public void MakeDirty()
	{
		dirtPercent = 1f;
	}

	// คืนค่า true เมื่อเช็ดจนสะอาดในครั้งนี้
	public bool Wipe(float amount)
	{
		if (dirtPercent <= 0f)
			return false;

		dirtPercent = Mathf.Max(0f, dirtPercent - amount);
		return dirtPercent <= 0f;
	}

	public void CleanCamera()
	{
		dirtPercent = 0f;
	}
}