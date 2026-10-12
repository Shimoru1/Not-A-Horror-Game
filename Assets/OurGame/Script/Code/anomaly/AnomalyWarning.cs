using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnomalyWarning : MonoBehaviour
{
	[Header("References")]
	public AnomalyCounter counter;

	[Header("Warning UI (วางไว้ใน Canvas ที่เห็นทั้งตอนดูกล้องและออกกล้อง)")]
	[SerializeField] private Image warningImage;
	[SerializeField] private TextMeshProUGUI warningText;

	[Tooltip("ความเร็วการ fade เข้า-ออก")]
	public float pulseSpeed = 4f;
	[Range(0f, 1f)] public float imageMaxAlpha = 0.6f;

	[Header("Sound")]
	public string warningSoundId = "TooManyAnomalySound";
	public float soundInterval = 1.5f;

	[Header("Red Lights")]
	public Color warningLightColor = Color.red;
	[Tooltip("ความสว่างต่ำสุดตอน fade (คูณกับความสว่างเดิม)")]
	[Range(0f, 1f)] public float minIntensityMultiplier = 0.15f;
	[Tooltip("ไฟที่ไม่ต้องให้เปลี่ยนสี")]
	public Light[] excludeLights;

	private bool isWarning = false;
	private float soundTimer = 0f;

	private Light[] lights;
	private Color[] originalColors;
	private float[] originalIntensity;


	private void Start()
	{
		if (counter == null)
			counter = FindFirstObjectByType<AnomalyCounter>();

		SetAlpha(0f);
	}

	private void Update()
	{
		if (GameManager.Instance != null)
		{
			if (GameManager.Instance.IsGameOver() ||
				GameManager.Instance.IsWin())
			{
				if (isWarning) StopWarning();
				return;
			}

			// Pause / Starting: ค้างไว้เฉยๆ
			if (!GameManager.Instance.IsPlaying())
				return;
		}

		bool shouldWarn = counter != null && counter.IsWarning();

		if (!shouldWarn)
		{
			if (isWarning) StopWarning();
			return;
		}

		if (!isWarning)
			StartWarning();

		// เสียงเตือนวนซ้ำ
		soundTimer -= Time.deltaTime;

		if (soundTimer <= 0f)
		{
			if (AudioManager.Instance != null)
				AudioManager.Instance.PlaySFX(warningSoundId);

			soundTimer = soundInterval;
		}

		// fade in / out
		float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

		SetAlpha(t);
		ApplyLights(t);
	}


	// ==================================================
	// START / STOP
	// ==================================================

	private void StartWarning()
	{
		isWarning = true;
		soundTimer = 0f;
		CacheLights();
	}

	private void StopWarning()
	{
		isWarning = false;
		soundTimer = 0f;

		SetAlpha(0f);
		RestoreLights();
	}


	// ==================================================
	// UI
	// ==================================================

	private void SetAlpha(float t)
	{
		if (warningImage != null)
		{
			bool show = t > 0f;

			if (warningImage.gameObject.activeSelf != show)
				warningImage.gameObject.SetActive(show);

			Color c = warningImage.color;
			c.a = t * imageMaxAlpha;
			warningImage.color = c;
		}

		if (warningText != null)
		{
			bool show = t > 0f;

			if (warningText.gameObject.activeSelf != show)
				warningText.gameObject.SetActive(show);

			Color c = warningText.color;
			c.a = t;
			warningText.color = c;
		}
	}


	// ==================================================
	// LIGHTS
	// ==================================================

	private void CacheLights()
	{
		Light[] all = FindObjectsByType<Light>(
			FindObjectsInactive.Include,
			FindObjectsSortMode.None
		);

		System.Collections.Generic.List<Light> list =
			new System.Collections.Generic.List<Light>();

		foreach (Light l in all)
		{
			if (l == null) continue;
			if (System.Array.IndexOf(excludeLights, l) >= 0) continue;

			list.Add(l);
		}

		lights = list.ToArray();
		originalColors = new Color[lights.Length];
		originalIntensity = new float[lights.Length];

		for (int i = 0; i < lights.Length; i++)
		{
			originalColors[i] = lights[i].color;
			originalIntensity[i] = lights[i].intensity;
		}
	}

	private void ApplyLights(float t)
	{
		if (lights == null) return;

		float mul = Mathf.Lerp(minIntensityMultiplier, 1f, t);

		for (int i = 0; i < lights.Length; i++)
		{
			if (lights[i] == null) continue;

			lights[i].color = warningLightColor;
			lights[i].intensity = originalIntensity[i] * mul;
		}
	}

	private void RestoreLights()
	{
		if (lights == null) return;

		for (int i = 0; i < lights.Length; i++)
		{
			if (lights[i] == null) continue;

			lights[i].color = originalColors[i];
			lights[i].intensity = originalIntensity[i];
		}

		lights = null;
	}
}