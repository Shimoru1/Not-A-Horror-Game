using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
	public static TransitionManager Instance;

	// true ระหว่างที่กำลังเล่นอนิเมชันปิด/เปิด (ใช้กันกดปุ่มหรือ ESC ซ้ำ)
	public static bool IsBusy => Instance != null && Instance.isBusy;

	[Header("Transition")]
	[SerializeField] private Image circleTransition;

	[SerializeField] private float duration = 0.7f;

	private bool shouldOpen = false;
	private bool isBusy = false;
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

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (shouldOpen)
		{
			shouldOpen = false;

			StartCoroutine(OpenCircle());
		}
	}

	// =========================================================
	// ใช้ฟังก์ชันนี้แทน SceneManager.LoadScene ทุกที่
	// ถ้าซีนนั้นไม่มี TransitionManager จะโหลดตรงๆ ให้
	// =========================================================

	public static void Load(string sceneName)
	{
		if (Instance != null)
			Instance.StartLoadScene(sceneName);
		else
			SceneManager.LoadScene(sceneName);
	}

	public void StartLoadScene(string sceneName)
	{
		if (isBusy)
			return;

		if (!Application.CanStreamedLevelBeLoaded(sceneName))
		{
			Debug.LogError("[TransitionManager] Scene not in Build Settings: " + sceneName);
			return;
		}

		StartCoroutine(LoadSceneProcess(sceneName));
	}

	private IEnumerator LoadSceneProcess(string sceneName)
	{
		isBusy = true;

		circleTransition.gameObject.SetActive(true);

		// บล็อกการคลิก UI ระหว่างเล่นอนิเมชัน
		circleTransition.raycastTarget = true;

		circleTransition.fillAmount = 0f;

		float timer = 0f;

		while (timer < duration)
		{
			// unscaled: ต้องเล่นได้แม้เกมถูก Pause / ชนะ / แพ้ (timeScale = 0)
			timer += Time.unscaledDeltaTime;

			float t = Mathf.Clamp01(timer / duration);

			t = Mathf.SmoothStep(0f, 1f, t);

			circleTransition.fillAmount = t;

			yield return null;
		}

		circleTransition.fillAmount = 1f;

		shouldOpen = true;

		// ให้ซีนใหม่เริ่มแบบปกติ
		Time.timeScale = 1f;
		AudioListener.pause = false;

		SceneManager.LoadScene(sceneName);
	}

	private IEnumerator OpenCircle()
	{
		circleTransition.gameObject.SetActive(true);
		circleTransition.raycastTarget = true;

		circleTransition.fillAmount = 1f;

		float timer = 0f;

		while (timer < duration)
		{
			timer += Time.unscaledDeltaTime;

			float t = Mathf.Clamp01(timer / duration);

			t = Mathf.SmoothStep(0f, 1f, t);

			float value = Mathf.Lerp(1f, 0f, t);

			circleTransition.fillAmount = value;

			yield return null;
		}

		circleTransition.fillAmount = 0f;
		circleTransition.raycastTarget = false;

		circleTransition.gameObject.SetActive(false);

		isBusy = false;
	}
}