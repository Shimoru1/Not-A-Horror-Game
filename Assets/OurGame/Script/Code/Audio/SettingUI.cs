using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
	[Header("Panels")]
	[SerializeField] private GameObject settingsPanel;
	[SerializeField] private GameObject pausePanel;


	[Header("Sliders")]
	[SerializeField] private Slider masterSlider;
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;

	[Header("Scroll (ไม่ใส่ก็ได้)")]
	[SerializeField] private ScrollRect scrollRect;

	private void Start()
	{
		if (settingsPanel != null)
			settingsPanel.SetActive(false);

		if (AudioManager.Instance == null)
		{
			Debug.LogError("SettingUI: ไม่พบ AudioManager ใน Scene");
		}

		SetupSlider(masterSlider, OnMasterChanged);
		SetupSlider(musicSlider, OnMusicChanged);
		SetupSlider(sfxSlider, OnSFXChanged);

		RefreshSliders();
	}

	private void SetupSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
	{
		if (slider == null) return;

		slider.minValue = 0f;
		slider.maxValue = 1f;
		slider.onValueChanged.AddListener(callback);
	}

	private void RefreshSliders()
	{
		AudioManager am = AudioManager.Instance;

		if (masterSlider != null)
			masterSlider.SetValueWithoutNotify(am != null ? am.GetMasterVolume() : 1f);

		if (musicSlider != null)
			musicSlider.SetValueWithoutNotify(am != null ? am.GetMusicVolume() : 1f);

		if (sfxSlider != null)
			sfxSlider.SetValueWithoutNotify(am != null ? am.GetSFXVolume() : 1f);
	}

	public void OpenSettings()
	{
		if (pausePanel != null)
			pausePanel.SetActive(false);

		if (settingsPanel != null)
			settingsPanel.SetActive(true);

		RefreshSliders();

		if (scrollRect != null)
		{
			Canvas.ForceUpdateCanvases();
			scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	public void CloseSettings()
	{
		// ปิดหน้า Settings
		if (settingsPanel != null)
			settingsPanel.SetActive(false);

		// กลับไปหน้า Pause Menu
		if (pausePanel != null)
			pausePanel.SetActive(true);
	}

	private void OnMasterChanged(float v)
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.SetMasterVolume(v);
	}

	private void OnMusicChanged(float v)
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.SetMusicVolume(v);
	}

	private void OnSFXChanged(float v)
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.SetSFXVolume(v);
	}

	private void OnDestroy()
	{
		if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
		if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
		if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
	}
}