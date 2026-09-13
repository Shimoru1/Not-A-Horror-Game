using UnityEngine;

public class BackgroundPan : MonoBehaviour
{
	public float panDistance = 5f;
	public float panSpeed = 0.5f;

	private Vector3 startPosition;

	void Start()
	{
		startPosition = transform.position;
	}

	void Update()
	{
		float x = Mathf.Sin(Time.time * panSpeed) * panDistance;

		transform.position = startPosition + new Vector3(x, 0, 0);
	}
}
