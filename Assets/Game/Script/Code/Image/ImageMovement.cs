using UnityEngine;

public class ImageMovement : MonoBehaviour
{
	[Header("Floating Effect")]
	public float height = 20f;
	public float speed = 3f;

	[Header("Zoom Effect")]
	public float startScale = 5f;
	public float zoomDuration = 0.5f;

	[Header("Jelly Effect")]
	public float jellyAmount = 0.5f;
	public float jellySpeed = 10f;

	private RectTransform logo;
	private Vector2 normalPosition;
	private Vector3 normalScale;

	private float timer;

	void Start()
	{
		logo = GetComponent<RectTransform>();

		normalPosition = logo.anchoredPosition;
		normalScale = logo.localScale;

		logo.anchoredPosition = Vector2.zero;
		logo.localScale = normalScale * startScale;
	}

	void Update()
	{
		timer += Time.deltaTime;

		if (timer < zoomDuration)
		{
			float t = timer / zoomDuration;
	
			float ease = Mathf.SmoothStep(0f, 1f, t);
		
			logo.anchoredPosition = Vector2.Lerp(Vector2.zero,normalPosition,ease);
		
			float scale = Mathf.Lerp(startScale,1f,ease);

			logo.localScale = normalScale * scale;
		}
		else
		{			
			float jellyTime = timer - zoomDuration;
		
			float jellyFade = Mathf.Exp(-jellyTime * 4f);

			float wave = Mathf.Sin(jellyTime * jellySpeed);

			float squash = wave * jellyAmount * jellyFade;
		
			float scaleX = 1f - squash;
			float scaleY = 1f + squash;

			logo.localScale = new Vector3(normalScale.x * scaleX,normalScale.y * scaleY,normalScale.z);
		
			float y = Mathf.Sin(jellyTime * speed) * height;

			logo.anchoredPosition = normalPosition + new Vector2(0, y);
		}
	}
}
