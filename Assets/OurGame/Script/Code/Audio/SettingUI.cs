using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    private ButtonManager buttonManager;
	[Header("Settings")]
	[SerializeField] private GameObject settingsPanel;
	[SerializeField] private Slider volumeSlider;

	private void Start()
	{
		buttonManager = GetComponent<ButtonManager>();
		settingsPanel.SetActive(false);

		volumeSlider.minValue = 0f;
		volumeSlider.maxValue = 1f;

		volumeSlider.value = AudioManager.Instance.GetVolume();

		volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
	}

	public void OpenSettings()
	{
		settingsPanel.SetActive(true);
		buttonManager.OnPlayClick();

		volumeSlider.value = AudioManager.Instance.GetVolume();
	}

	public void CloseSettings()
	{
		settingsPanel.SetActive(false);
		buttonManager.ShowButtonsImmediately();
	}

	private void OnVolumeChanged(float value)
	{
		AudioManager.Instance.SetVolume(value);
	}

	private void OnDestroy()
	{
		volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
	}
}
