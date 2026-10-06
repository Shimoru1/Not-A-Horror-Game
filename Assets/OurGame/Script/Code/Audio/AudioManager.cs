using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	private const string MusicVolumeKey = "MusicVolume";
	private const string SFXVolumeKey = "SFXVolume";

	private float musicVolume = 1f;
	private float sfxVolume = 1f;

	[Header("Main Menu Music")]
	[SerializeField] private AudioSource musicAudioSource;
	[SerializeField] private AudioClip mainMenuMusic;

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

		// ตั้งค่า AudioSource
		if (musicAudioSource != null)
		{
			musicAudioSource.loop = true;
			musicAudioSource.playOnAwake = false;
			musicAudioSource.volume = musicVolume;

			if (mainMenuMusic != null)
			{
				musicAudioSource.clip = mainMenuMusic;
			}
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void Start()
	{
		// ตรวจ Scene ที่เปิดอยู่ครั้งแรก
		HandleSceneMusic(SceneManager.GetActiveScene());
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	// =========================================
	// SCENE MUSIC
	// =========================================

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		HandleSceneMusic(scene);
	}

	private void HandleSceneMusic(Scene scene)
	{
		if (scene.name == "MainMenu")
		{
			PlayMainMenuMusic();
		}
		else
		{
			StopMainMenuMusic();
		}
	}

	// =========================================
	// MAIN MENU MUSIC
	// =========================================

	private void PlayMainMenuMusic()
	{
		if (musicAudioSource == null)
		{
			Debug.LogError("AudioManager: Music AudioSource ยังไม่ได้ใส่!");
			return;
		}

		if (mainMenuMusic == null)
		{
			Debug.LogError("AudioManager: Main Menu Music ยังไม่ได้ใส่!");
			return;
		}

		musicAudioSource.clip = mainMenuMusic;
		musicAudioSource.volume = musicVolume;
		musicAudioSource.loop = true;

		if (!musicAudioSource.isPlaying)
		{
			musicAudioSource.Play();

			Debug.Log("MAIN MENU MUSIC PLAY");
		}
	}

	private void StopMainMenuMusic()
	{
		if (musicAudioSource == null)
			return;

		if (musicAudioSource.isPlaying)
		{
			musicAudioSource.Stop();

			Debug.Log("MAIN MENU MUSIC STOP");
		}
	}

	// =========================================
	// MUSIC VOLUME
	// =========================================

	public void SetMusicVolume(float volume)
	{
		musicVolume = Mathf.Clamp01(volume);

		if (musicAudioSource != null)
		{
			musicAudioSource.volume = musicVolume;
		}

		PlayerPrefs.SetFloat(
			MusicVolumeKey,
			musicVolume
		);

		PlayerPrefs.Save();
	}

	public float GetMusicVolume()
	{
		return PlayerPrefs.GetFloat(
			MusicVolumeKey,
			1f
		);
	}

	// =========================================
	// SFX VOLUME
	// =========================================

	public void SetSFXVolume(float volume)
	{
		sfxVolume = Mathf.Clamp01(volume);

		PlayerPrefs.SetFloat(
			SFXVolumeKey,
			sfxVolume
		);

		PlayerPrefs.Save();
	}

	public float GetSFXVolume()
	{
		return PlayerPrefs.GetFloat(
			SFXVolumeKey,
			1f
		);
	}

	// =========================================
	// LOAD
	// =========================================

	private void LoadVolume()
	{
		musicVolume = GetMusicVolume();
		sfxVolume = GetSFXVolume();
	}
}
