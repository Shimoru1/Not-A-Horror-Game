using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	private const string MasterKey = "MasterVolume";
	private const string MusicKey = "MusicVolume";
	private const string SFXKey = "SFXVolume";

	private const string MasterParam = "MasterVolume";
	private const string MusicParam = "MusicVolume";
	private const string SFXParam = "SFXVolume";

	private float masterVolume = 1f;
	private float musicVolume = 1f;
	private float sfxVolume = 1f;

	[Serializable]
	public class Sound
	{
		public string id;
		public AudioClip clip;

		[Range(0f, 1f)]
		public float volume = 1f;

		public bool loop;
	}

	[Serializable]
	public class SceneMusic
	{
		public string sceneName;
		public string musicId;
	}

	[Header("Sound Lists")]
	public List<Sound> musicList = new List<Sound>();
	public List<Sound> sfxList = new List<Sound>();

	[Header("Scene Music")]
	[SerializeField]
	private List<SceneMusic> sceneMusics =
		new List<SceneMusic>();

	[Header("Audio Mixer")]
	[SerializeField] private AudioMixer mainMixer;
	[SerializeField] private AudioMixerGroup musicGroup;
	[SerializeField] private AudioMixerGroup sfxGroup;
	public AudioMixerGroup MusicOutputGroup => musicGroup;
	public AudioMixerGroup SFXOutputGroup => sfxGroup;

	[Header("Audio Sources")]
	[SerializeField] private AudioSource musicAudioSource;
	[SerializeField] private AudioSource sfxAudioSource;

	private AudioSource repairingAudioSource;
	private string currentSFXId;
	private string currentRepairingSFXId;
	private string currentMusicId;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Debug.LogWarning(
				$"[AudioManager] Duplicate destroyed: {gameObject.name}"
			);

			Destroy(gameObject);
			return;
		}

		Instance = this;

		// AudioManager ควรอยู่บน Root GameObject
		DontDestroyOnLoad(gameObject);

		EnsurePersistentAudioSources();
		LoadVolume();

		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void Start()
	{
		if (mainMixer == null ||
			musicGroup == null ||
			sfxGroup == null)
		{
			Debug.LogError(
				"AudioManager: กรุณากำหนด Main Mixer, Music Group และ SFX Group"
			);
			return;
		}

		musicAudioSource.outputAudioMixerGroup = musicGroup;
		sfxAudioSource.outputAudioMixerGroup = sfxGroup;

		ApplyMixerVolumes();

		// โหมด Test: ไม่เล่นเพลงอัตโนมัติ
		if (GameManager.Instance != null &&
			GameManager.Instance.IsTestMode)
		{
			StopMusic();
			Debug.Log("[AudioManager] Test mode: Auto music disabled.");
			return;
		}

		HandleSceneMusic(SceneManager.GetActiveScene());
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			Instance = null;
		}
	}

	private void LoadVolume()
	{
		masterVolume = Mathf.Clamp01(
			PlayerPrefs.GetFloat(MasterKey, 1f));

		musicVolume = Mathf.Clamp01(
			PlayerPrefs.GetFloat(MusicKey, 1f));

		sfxVolume = Mathf.Clamp01(
			PlayerPrefs.GetFloat(SFXKey, 1f));
	}

	private bool SetMixerVolume(string parameter, float value)
	{
		if (mainMixer == null)
			return false;

		float db = value <= 0.0001f
			? -80f
			: Mathf.Log10(value) * 20f;

		bool success = mainMixer.SetFloat(parameter, db);

		if (!success)
		{
			Debug.LogError(
				$"AudioManager: SetFloat('{parameter}') ล้มเหลว " +
				"ตรวจสอบ Exposed Parameters และ Mixer"
			);
		}

		return success;
	}

	private void ApplyMixerVolumes()
	{
		SetMixerVolume(MasterParam, masterVolume);
		SetMixerVolume(MusicParam, musicVolume);
		SetMixerVolume(SFXParam, sfxVolume);
	}

	public void SetMasterVolume(float value)
	{
		masterVolume = Mathf.Clamp01(value);

		SetMixerVolume(MasterParam, masterVolume);

		PlayerPrefs.SetFloat(MasterKey, masterVolume);
		PlayerPrefs.Save();
	}

	public float GetMasterVolume() => masterVolume;

	public void SetMusicVolume(float value)
	{
		musicVolume = Mathf.Clamp01(value);

		SetMixerVolume(MusicParam, musicVolume);

		PlayerPrefs.SetFloat(MusicKey, musicVolume);
		PlayerPrefs.Save();
	}

	public float GetMusicVolume() => musicVolume;

	public void SetSFXVolume(float value)
	{
		sfxVolume = Mathf.Clamp01(value);

		SetMixerVolume(SFXParam, sfxVolume);

		PlayerPrefs.SetFloat(SFXKey, sfxVolume);
		PlayerPrefs.Save();
	}

	public float GetSFXVolume() => sfxVolume;

	private Sound FindMusic(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
			return null;

		return musicList.Find(s =>
			s != null &&
			string.Equals(
				s.id?.Trim(),
				id.Trim(),
				StringComparison.OrdinalIgnoreCase
			)
		);
	}

	private Sound FindSFX(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
			return null;

		return sfxList.Find(s =>
			s != null &&
			string.Equals(
				s.id?.Trim(),
				id.Trim(),
				StringComparison.OrdinalIgnoreCase
			)
		);
	}
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (Instance != this)
			return;

		StartCoroutine(RefreshAudioAfterSceneLoad(scene));
	}

	private IEnumerator RefreshAudioAfterSceneLoad(Scene scene)
	{
		// รอให้การเปลี่ยนซีนและ Awake ของวัตถุในซีนเสร็จก่อน
		yield return null;

		if (Instance != this)
			yield break;

		if (musicAudioSource == null || sfxAudioSource == null)
		{
			Debug.LogError(
				"[AudioManager] AudioSource is missing after scene load!"
			);
			yield break;
		}

		if (mainMixer == null || musicGroup == null || sfxGroup == null)
		{
			Debug.LogError(
				"[AudioManager] Mixer or Mixer Group is missing!"
			);
			yield break;
		}

		musicAudioSource.outputAudioMixerGroup = musicGroup;
		sfxAudioSource.outputAudioMixerGroup = sfxGroup;

		ApplyMixerVolumes();

		Debug.Log(
			$"[AudioManager] Active Scene={scene.name}, " +
			$"MusicSource={musicAudioSource.isActiveAndEnabled}, " +
			$"SFXSource={sfxAudioSource.isActiveAndEnabled}"
		);
		if (GameManager.Instance != null && GameManager.Instance.IsTestMode)
		{
			StopMusic();
			Debug.Log("[AudioManager] Test mode: Scene music disabled.");
			yield break;
		}

		HandleSceneMusic(scene);
	}

	private void HandleSceneMusic(Scene scene)
	{
		SceneMusic match = sceneMusics.Find(
			m => m != null &&
				 string.Equals(
					 m.sceneName?.Trim(),
					 scene.name,
					 StringComparison.OrdinalIgnoreCase
				 )
		);

		if (match == null)
		{
			// ซีนนี้ไม่มีเพลงที่ AudioManager ต้องเล่น
			// หยุดเฉพาะเพลงที่ AudioManager เป็นเจ้าของ
			StopMusic();
			return;
		}

		if (string.IsNullOrWhiteSpace(match.musicId))
		{
			StopMusic();
			return;
		}

		PlayMusic(match.musicId.Trim());
	}

	public void PlayMusic(string id)
	{
		Sound sound = FindMusic(id);

		if (sound == null)
		{
			Debug.LogError(
				$"[AudioManager] ไม่พบ Music ID '{id}' ใน musicList"
			);
			StopMusic();
			return;
		}

		if (sound.clip == null)
		{
			Debug.LogError(
				$"[AudioManager] Music ID '{id}' ไม่มี AudioClip"
			);
			StopMusic();
			return;
		}

		if (musicAudioSource == null)
		{
			Debug.LogError(
				"[AudioManager] Music AudioSource is missing!"
			);
			return;
		}

		if (musicGroup == null)
		{
			Debug.LogError(
				"[AudioManager] Music Mixer Group is missing!"
			);
			return;
		}

		// ถ้าเป็นเพลงเดิมที่กำลังเล่นอยู่ ไม่ต้องเริ่มใหม่
		if (currentMusicId == sound.id &&
			musicAudioSource.clip == sound.clip &&
			musicAudioSource.isPlaying)
		{
			return;
		}

		musicAudioSource.Stop();

		currentMusicId = sound.id;
		musicAudioSource.outputAudioMixerGroup = musicGroup;
		musicAudioSource.clip = sound.clip;
		musicAudioSource.loop = sound.loop;
		musicAudioSource.volume = sound.volume;

		musicAudioSource.Play();

		Debug.Log(
			$"[AudioManager] Started music '{sound.id}', " +
			$"isPlaying = {musicAudioSource.isPlaying}"
		);
	}

	public void StopMusic()
	{
		if (musicAudioSource != null)
		{
			musicAudioSource.Stop();
			musicAudioSource.clip = null;
		}

		currentMusicId = null;
	}

	public void PlaySFX(string id)
	{
		if (sfxAudioSource == null)
		{
			Debug.LogError("[AudioManager] SFX AudioSource is NULL!");
			return;
		}

		if (!sfxAudioSource.isActiveAndEnabled)
		{
			Debug.LogError(
				"[AudioManager] SFX AudioSource is disabled or inactive!"
			);
			return;
		}

		Sound sound = FindSFX(id);

		if (sound == null || sound.clip == null)
		{
			Debug.LogWarning($"[AudioManager] SFX not found: {id}");
			return;
		}

		if (sfxGroup == null)
		{
			Debug.LogError("[AudioManager] SFX Group is NULL!");
			return;
		}

		sfxAudioSource.outputAudioMixerGroup = sfxGroup;
		sfxAudioSource.loop = false;

		sfxAudioSource.PlayOneShot(
			sound.clip,
			Mathf.Clamp01(sound.volume)
		);

		Debug.Log(
			$"[AudioManager] SFX='{id}', " +
			$"Clip='{sound.clip.name}', " +
			$"SourceID={sfxAudioSource.GetInstanceID()}, " +
			$"Mixer='{sfxAudioSource.outputAudioMixerGroup.name}', " +
			$"Playing={sfxAudioSource.isPlaying}"
		);
	}

	public void PlaySFXAtPoint(string id, Vector3 position)
	{
		Sound sound = FindSFX(id);

		if (sound == null || sound.clip == null)
		{
			Debug.LogWarning("ไม่พบ SFX ID: " + id);
			return;
		}

		if (sfxGroup == null)
			return;

		// สร้าง AudioSource ที่ส่งเสียงเข้า SFX Group โดยตรง
		GameObject obj = new GameObject("SFX_" + id);
		obj.transform.position = position;

		AudioSource source = obj.AddComponent<AudioSource>();
		source.clip = sound.clip;
		source.volume = sound.volume;
		source.spatialBlend = 1f;
		source.outputAudioMixerGroup = sfxGroup;
		source.Play();

		Destroy(obj, sound.clip.length + 0.1f);
	}

	private void EnsurePersistentAudioSources()
	{
		AudioSource[] sources = GetComponents<AudioSource>();

		if (sources.Length > 0)
		{
			musicAudioSource = sources[0];
		}
		else
		{
			musicAudioSource = gameObject.AddComponent<AudioSource>();
		}

		if (sources.Length > 1)
		{
			sfxAudioSource = sources[1];
		}
		else
		{
			sfxAudioSource = gameObject.AddComponent<AudioSource>();
		}

		musicAudioSource.playOnAwake = false;
		musicAudioSource.spatialBlend = 0f;

		sfxAudioSource.playOnAwake = false;
		sfxAudioSource.spatialBlend = 0f;
		sfxAudioSource.loop = false;

		if (musicGroup != null)
			musicAudioSource.outputAudioMixerGroup = musicGroup;

		if (sfxGroup != null)
			sfxAudioSource.outputAudioMixerGroup = sfxGroup;

		if (repairingAudioSource == null)
		{
			repairingAudioSource = gameObject.AddComponent<AudioSource>();
		}

		repairingAudioSource.playOnAwake = false;
		repairingAudioSource.spatialBlend = 0f;
		repairingAudioSource.loop = true;

		if (sfxGroup != null)
		{
			repairingAudioSource.outputAudioMixerGroup = sfxGroup;
		}
	}

	private static bool isBootstrapping;

	[RuntimeInitializeOnLoadMethod(
	RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void BootstrapAudioManager()
	{
		if (Instance != null)
			return;

		AudioManager prefab =
			Resources.Load<AudioManager>("AudioManager");

		if (prefab == null)
		{
			Debug.LogError(
				"[AudioManager] ไม่พบ Prefab ที่ " +
				"Assets/Resources/AudioManager.prefab"
			);
			return;
		}

		AudioManager manager = Instantiate(prefab);

		Debug.Log(
			"[AudioManager] Bootstrap สำเร็จ: " +
			manager.gameObject.name
		);
	}

	public IEnumerator FadeOutMusic(float duration)
	{
		if (musicAudioSource == null)
			yield break;

		float startVolume = musicAudioSource.volume;
		float timer = 0f;

		while (timer < duration)
		{
			timer += Time.unscaledDeltaTime;

			musicAudioSource.volume = Mathf.Lerp(
				startVolume,
				0f,
				Mathf.Clamp01(timer / duration)
			);

			yield return null;
		}

		musicAudioSource.volume = 0f;
		StopMusic();
		musicAudioSource.volume = startVolume;
	}

	public void PlayLoopSFX(string id)
	{
		if (repairingAudioSource == null)
		{
			Debug.LogError("[AudioManager] Repairing AudioSource is NULL!");
			return;
		}

		if (sfxGroup == null)
		{
			Debug.LogError("[AudioManager] SFX Group is NULL!");
			return;
		}

		Sound sound = FindSFX(id);

		if (sound == null || sound.clip == null)
		{
			Debug.LogWarning("[AudioManager] SFX not found: " + id);
			return;
		}

		// ถ้าเสียงซ่อมเดิมกำลังเล่นอยู่ ไม่เริ่มซ้ำ
		if (currentRepairingSFXId == sound.id &&
			repairingAudioSource.isPlaying)
		{
			return;
		}

		repairingAudioSource.Stop();

		currentRepairingSFXId = sound.id;
		repairingAudioSource.clip = sound.clip;
		repairingAudioSource.volume = sound.volume;
		repairingAudioSource.loop = true;
		repairingAudioSource.outputAudioMixerGroup = sfxGroup;
		repairingAudioSource.Play();

		Debug.Log("[AudioManager] Repair loop started: " + id);
	}

	public void StopSFX(string id)
	{
		if (string.Equals(
			currentRepairingSFXId,
			id,
			StringComparison.OrdinalIgnoreCase))
		{
			if (repairingAudioSource != null)
			{
				repairingAudioSource.Stop();
				repairingAudioSource.clip = null;
				repairingAudioSource.loop = false;
			}

			currentRepairingSFXId = null;
		}

		// หยุดเสียง SFX ทั่วไปเฉพาะเมื่อ ID ตรงกัน
		if (string.Equals(
			currentSFXId,
			id,
			StringComparison.OrdinalIgnoreCase))
		{
			if (sfxAudioSource != null)
			{
				sfxAudioSource.Stop();
				sfxAudioSource.clip = null;
				sfxAudioSource.loop = false;
			}

			currentSFXId = null;
		}
	}
}