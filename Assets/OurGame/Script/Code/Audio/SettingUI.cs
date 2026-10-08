using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private GameObject settingsPanel;
	[SerializeField] private Slider volumeSlider;

	private void Start()
	{
		if (settingsPanel != null)
			settingsPanel.SetActive(false);

		if (volumeSlider != null)
		{
			volumeSlider.minValue = 0f;
			volumeSlider.maxValue = 1f;

			float volume = (AudioManager.Instance != null)
				? AudioManager.Instance.GetMusicVolume()
				: 1f;

			volumeSlider.SetValueWithoutNotify(volume);
			volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
		}
	}

	public void OpenSettings()
	{
		if (settingsPanel != null)
			settingsPanel.SetActive(true);

		if (volumeSlider != null && AudioManager.Instance != null)
		{
			volumeSlider.SetValueWithoutNotify(
				AudioManager.Instance.GetMusicVolume()
			);
		}
	}

	public void CloseSettings()
	{
		if (settingsPanel != null)
			settingsPanel.SetActive(false);
	}

	private void OnVolumeChanged(float value)
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.SetMusicVolume(value);
		}
	}

	private void OnDestroy()
	{
		if (volumeSlider != null)
		{
			volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
		}
	}
}
