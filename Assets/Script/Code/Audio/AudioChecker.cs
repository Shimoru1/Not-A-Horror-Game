using UnityEngine;

public class AudioChecker : MonoBehaviour
{
	private void Start()
	{
		AudioListener[] listeners = FindObjectsByType<AudioListener>(
			FindObjectsSortMode.None
		);

		Debug.Log("AudioListeners found: " + listeners.Length);

		foreach (AudioListener listener in listeners)
		{
			Debug.Log("AudioListener: " + listener.gameObject.name);
		}
	}
}
