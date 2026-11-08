using System.Collections.Generic;
using UnityEngine;

public class AnomalyGlow : MonoBehaviour
{
	private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

	private AnomalyTarget target;

	private readonly List<Material> mats = new List<Material>();
	private readonly List<Color> originalColors = new List<Color>();
	private readonly List<bool> originalKeyword = new List<bool>();

	private Light glowLight;
	private Color glowColor = Color.yellow;
	private float intensity = 2.5f;
	private float speed = 4f;
	private bool glowing;

	public bool IsGlowing => glowing;

	public void StartGlow(Color color, float emissionIntensity, float pulseSpeed, bool addLight)
	{
		if (glowing) return;

		target = GetComponentInParent<AnomalyTarget>();
		glowColor = color;
		intensity = emissionIntensity;
		speed = pulseSpeed;

		Bounds bounds = new Bounds(transform.position, Vector3.zero);
		bool hasBounds = false;

		foreach (Renderer r in GetComponentsInChildren<Renderer>(false))
		{
			if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
				continue;

			if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
			else bounds.Encapsulate(r.bounds);

			foreach (Material m in r.materials)
			{
				if (m == null || !m.HasProperty(EmissionColorId))
					continue;

				mats.Add(m);
				originalColors.Add(m.GetColor(EmissionColorId));
				originalKeyword.Add(m.IsKeywordEnabled("_EMISSION"));

				m.EnableKeyword("_EMISSION");
			}
		}

		if (addLight)
		{
			GameObject go = new GameObject("AnomalyGlowLight");
			go.transform.SetParent(transform, false);
			go.transform.position = hasBounds ? bounds.center : transform.position;

			glowLight = go.AddComponent<Light>();
			glowLight.type = LightType.Point;
			glowLight.color = color;
			glowLight.range = Mathf.Max(3f, bounds.extents.magnitude * 3f);
			glowLight.shadows = LightShadows.None;
			glowLight.intensity = 2f;
		}

		glowing = true;
	}

	private void Update()
	{
		if (!glowing) return;

		// anomaly ถูกกำจัด/กลับเป็นปกติแล้ว -> หยุดเรืองแสง
		if (target != null && !target.IsActive)
		{
			StopGlow();
			return;
		}

		float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
		Color e = glowColor * (intensity * Mathf.Lerp(0.25f, 1f, t));

		for (int i = 0; i < mats.Count; i++)
		{
			if (mats[i] != null)
				mats[i].SetColor(EmissionColorId, e);
		}

		if (glowLight != null)
			glowLight.intensity = Mathf.Lerp(0.5f, 3f, t);
	}

	public void StopGlow()
	{
		if (!glowing) return;
		glowing = false;

		for (int i = 0; i < mats.Count; i++)
		{
			if (mats[i] == null) continue;

			mats[i].SetColor(EmissionColorId, originalColors[i]);

			if (!originalKeyword[i])
				mats[i].DisableKeyword("_EMISSION");
		}

		mats.Clear();
		originalColors.Clear();
		originalKeyword.Clear();

		if (glowLight != null)
			Destroy(glowLight.gameObject);

		glowLight = null;
	}

	private void OnDestroy()
	{
		StopGlow();
	}
}