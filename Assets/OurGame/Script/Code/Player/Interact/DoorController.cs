using UnityEngine;

public class DoorController : MonoBehaviour
{
	public float openAngle = 90f;
	public float openSpeed = 3f;

	private bool isOpen = false;
	private bool playerNearby = false;

	private Quaternion closedRotation;
	private Quaternion openRotation;

	void Start()
	{
		closedRotation = transform.localRotation;
		openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
	}

	void Update()
	{
		if (playerNearby && Input.GetKeyDown(KeyCode.F))
		{
			isOpen = !isOpen;
		}

		Quaternion targetRotation = isOpen
			? openRotation
			: closedRotation;

		transform.localRotation = Quaternion.Slerp(
			transform.localRotation,
			targetRotation,
			Time.deltaTime * openSpeed
		);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerNearby = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerNearby = false;
		}
	}
}
