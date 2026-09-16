using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	private const string VolumeKey = "MasterVolume";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		LoadVolume();
	}

	public void PlayMusic(AudioSource music)
	{
		if (music == null)
		{
			Debug.LogWarning("Music AudioSource is null!");
			return;
		}

		music.Play();
	}

	public void StopMusic(AudioSource music)
	{
		if (music == null)
		{
			Debug.LogWarning("Music AudioSource is null!");
			return;
		}

		music.Stop();
	}

	public void SetVolume(float volume)
	{
		AudioListener.volume = volume;

		PlayerPrefs.SetFloat(VolumeKey, volume);
		PlayerPrefs.Save();
	}

	public float GetVolume()
	{
		return PlayerPrefs.GetFloat(VolumeKey, 1f);
	}

	private void LoadVolume()
	{
		AudioListener.volume = GetVolume();
	}
}
