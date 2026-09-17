using UnityEngine;

public class GameMusic : MonoBehaviour
{
	[SerializeField] private AudioSource backgroundMusic;

	private void Start()
	{
		AudioManager.Instance.PlayMusic(backgroundMusic);
	}

	public void StopMusic()
	{
		AudioManager.Instance.StopMusic(backgroundMusic);
	}
}
