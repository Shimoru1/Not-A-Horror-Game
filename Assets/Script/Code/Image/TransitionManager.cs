using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
	public static TransitionManager Instance;

	[Header("Transition")]
	[SerializeField] private Image circleTransition;

	[SerializeField] private float duration = 0.7f;

	private bool shouldOpen = false;
	private Canvas transitionCanvas;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		DontDestroyOnLoad(gameObject);

		transitionCanvas = circleTransition.GetComponentInParent<Canvas>();

		if (transitionCanvas != null)
		{
			transitionCanvas.overrideSorting = true;
			transitionCanvas.sortingOrder = 9999;
		}
	
		circleTransition.transform.SetAsLastSibling();

		SceneManager.sceneLoaded += OnSceneLoaded;	
	}

	private void Start()
	{
		SetupCircle();

		circleTransition.gameObject.SetActive(false);
	}

	private void SetupCircle()
	{
		if (circleTransition == null)
		{
			Debug.LogError("CircleTransition not Assign!");
			return;
		}

		circleTransition.type = Image.Type.Filled;
		circleTransition.fillMethod = Image.FillMethod.Radial360;
		circleTransition.fillOrigin = 0;
		circleTransition.fillClockwise = true;

		circleTransition.fillAmount = 0f;
		circleTransition.raycastTarget = false;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene,LoadSceneMode mode)
	{
		if (shouldOpen)
		{
			shouldOpen = false;

			StartCoroutine(OpenCircle());
		}
	}

	public void StartLoadScene(string sceneName)
	{
		StartCoroutine(LoadSceneProcess(sceneName));
	}

	private IEnumerator LoadSceneProcess(string sceneName)
	{
		circleTransition.gameObject.SetActive(true);

		circleTransition.fillAmount = 0f;

		float timer = 0f;

		while (timer < duration)
		{
			timer += Time.deltaTime;

			float t = Mathf.Clamp01(timer / duration);

			t = Mathf.SmoothStep(0f, 1f, t);

			circleTransition.fillAmount = t;

			yield return null;
		}

		circleTransition.fillAmount = 1f;

		shouldOpen = true;

		SceneManager.LoadScene(sceneName);
	}

	private IEnumerator OpenCircle()
	{
		circleTransition.gameObject.SetActive(true);

		circleTransition.fillAmount = 1f;

		float timer = 0f;

		while (timer < duration)
		{
			timer += Time.deltaTime;

			float t = Mathf.Clamp01(timer / duration);

			t = Mathf.SmoothStep(0f, 1f, t);

			float value = Mathf.Lerp(1f, 0f, t);

			circleTransition.fillAmount = value;

			yield return null;
		}

		circleTransition.fillAmount = 0f;

		circleTransition.gameObject.SetActive(false);
	}
}
