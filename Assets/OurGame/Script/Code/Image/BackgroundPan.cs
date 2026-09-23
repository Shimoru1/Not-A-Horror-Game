using UnityEngine;

public class BackgroundPan : MonoBehaviour
{
	public float panAngle = 15f;
	public float panSpeed = 0.5f;

	private Quaternion startRotation;

	void Start()
	{
		startRotation = transform.rotation;
	}

	void Update()
	{
		float angle = Mathf.Sin(Time.time * panSpeed) * panAngle;

		transform.rotation = startRotation * Quaternion.Euler(0, angle, 0);
	}
}
