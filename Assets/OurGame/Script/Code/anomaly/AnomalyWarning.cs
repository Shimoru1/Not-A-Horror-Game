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

	private bool isWarning = false;


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

		// fade in / out
		float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

		SetAlpha(t);
	}


	// ==================================================
	// START / STOP
	// ==================================================

	private void StartWarning()
	{
		isWarning = true;

		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayLoopSFX(warningSoundId);
	}

	private void StopWarning()
	{
		isWarning = false;

		SetAlpha(0f);

		if (AudioManager.Instance != null)
			AudioManager.Instance.StopSFX(warningSoundId);
	}

	private void OnDisable()
	{
		if (isWarning)
			StopWarning();
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
}