using UnityEngine;
using UnityEngine.UI;

public class CCTVDirtOverlay : MonoBehaviour
{
	[Header("Dirt Overlay")]
	public RawImage[] dirtImages;

	[Header("Current Camera")]
	public CCTVCleaningTarget currentCamera;

	[Header("Settings")]
	[Range(0f, 1f)] public float maxAlpha = 0.85f;
	public float fadeSpeed = 6f;

	private float shownAlpha = 0f;

	private void Start()
	{
		shownAlpha = 0f;
		ApplyAlpha(0f);
	}

	private void Update()
	{
		float target = 0f;

		if (currentCamera != null)
			target = currentCamera.GetDirtPercent() * maxAlpha;

		// ค่อยๆ จางลง/เข้มขึ้น
		shownAlpha = Mathf.MoveTowards(
			shownAlpha, target, fadeSpeed * Time.deltaTime);

		ApplyAlpha(shownAlpha);
	}

	public void SetCamera(CCTVCleaningTarget target)
	{
		currentCamera = target;

		// เปลี่ยนกล้อง/ออกจากกล้อง ให้แสดงผลทันที ไม่ต้องค่อยๆ เปลี่ยน
		shownAlpha = (target != null)
			? target.GetDirtPercent() * maxAlpha
			: 0f;

		ApplyAlpha(shownAlpha);
	}

	private void ApplyAlpha(float alpha)
	{
		if (dirtImages == null) return;

		bool visible = alpha > 0.001f;

		foreach (RawImage image in dirtImages)
		{
			if (image == null) continue;

			Color c = image.color;
			c.a = alpha;
			image.color = c;

			if (image.gameObject.activeSelf != visible)
				image.gameObject.SetActive(visible);
		}
	}
}