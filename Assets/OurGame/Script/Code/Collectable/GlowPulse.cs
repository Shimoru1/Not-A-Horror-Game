using UnityEngine;

public class GlowPulse : MonoBehaviour
{
	[Header("Glow")]
	public Color glowColor = new Color(0.2f, 0.7f, 1f);
	public float pulseSpeed = 2f;

	[Header("Emission (ตัวกล่อง)")]
	public float minEmission = 0.5f;
	public float maxEmission = 3f;

	[Header("Light (แสงรอบกล่อง)")]
	public bool useLight = true;
	public float lightRange = 4f;
	public float minLightIntensity = 0.5f;
	public float maxLightIntensity = 3f;

	private Renderer[] renderers;
	private MaterialPropertyBlock block;
	private Light glowLight;

	private static readonly int EmissionColorId =
		Shader.PropertyToID("_EmissionColor");

	private void Awake()
	{
		renderers = GetComponentsInChildren<Renderer>();
		block = new MaterialPropertyBlock();

		// เปิดใช้ Emission ให้วัสดุ (ต้องทำกับ instance ของวัสดุ)
		foreach (Renderer r in renderers)
		{
			foreach (Material m in r.materials)
				m.EnableKeyword("_EMISSION");
		}

		if (useLight)
		{
			GameObject lightObj = new GameObject("GlowLight");
			lightObj.transform.SetParent(transform, false);
			lightObj.transform.localPosition = Vector3.up * 0.3f;

			glowLight = lightObj.AddComponent<Light>();
			glowLight.type = LightType.Point;
			glowLight.color = glowColor;
			glowLight.range = lightRange;
		}
	}

	private void Update()
	{
		// 0 -> 1 -> 0 วนต่อเนื่อง
		float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

		float emission = Mathf.Lerp(minEmission, maxEmission, t);

		foreach (Renderer r in renderers)
		{
			r.GetPropertyBlock(block);
			block.SetColor(EmissionColorId, glowColor * emission);
			r.SetPropertyBlock(block);
		}

		if (glowLight != null)
			glowLight.intensity =
				Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
	}
}