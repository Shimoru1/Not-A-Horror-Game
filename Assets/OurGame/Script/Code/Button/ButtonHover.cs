using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("Normal")]
	public float normalRotation = 0f;

	[Header("Hover")]
	public float hoverRotation = -8f;
	public float rotationSpeed = 10f;

	[Header("Shake")]
	public bool shake = false;
	public float shakeAmount = 4f;
	public float shakeSpeed = 25f;

	private RectTransform rect;
	private bool isHover;

	void Start()
	{
		rect = GetComponent<RectTransform>();
		isHover = false;
		rect.localRotation = Quaternion.Euler(0f, 0f, normalRotation);
	}

	void Update()
	{
		if (shake && isHover)
		{		
			float rotation =Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            rect.localRotation =Quaternion.Euler(0f, 0f, rotation);
		}
		else
		{
			float targetRotation =isHover ? hoverRotation : normalRotation;

			float currentRotation =rect.localEulerAngles.z;

			if (currentRotation > 180f)
				currentRotation -= 360f;

			float newRotation = Mathf.Lerp(currentRotation,targetRotation,Time.deltaTime * rotationSpeed);

			rect.localRotation =Quaternion.Euler(0f, 0f, newRotation);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHover = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHover = false;
		rect.localRotation = Quaternion.Euler(0f, 0f, normalRotation);
	}
	public void ResetButton()
	{
		isHover = false;
		rect.localRotation = Quaternion.Euler(0f, 0f, normalRotation);
	}
}
