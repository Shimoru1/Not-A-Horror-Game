using System.Collections;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
	[Header("Main Menu")]
	public Image logo;
	public Image Bg;
	public Button play;
	public Button setting;
	public Button quit;
	public Button back;
	public Button start;
	private ButtonHover playHover;
	private ButtonHover settingHover;
	private ButtonHover quitHover;

	[HideInInspector]

	public bool buttonsShown = false;
	[Header("Music")]
	[SerializeField] private AudioSource menuMusic;

	[Header("Stage Select")]
	public GameObject stageSelect;
	public Button nextStage;
	public Button previousStage;
	public Image[] stageImages;
	public TMP_Text stageName;
	public string[] sceneNames;
	private int currentStage = 0;
	private bool isChangingStage = false;

	[Header("Stage Motion")]
	public float stageMoveDistance = 500f;
	public float stageAnimationDuration = 0.35f;

	[Header("Stage Fade")]
	public float fadeDuration = 0.3f;


	private Coroutine buttonsSequence;
	private bool cancelButtonSequence = false;
	private Vector3 playNormalScale;
	private Vector3 settingNormalScale;
	private Vector3 quitNormalScale;

	private void Start()
	{
		playNormalScale = play.GetComponent<RectTransform>().localScale;
		settingNormalScale = setting.GetComponent<RectTransform>().localScale;
		quitNormalScale = quit.GetComponent<RectTransform>().localScale;

	    playHover = play.GetComponent<ButtonHover>();
		settingHover = setting.GetComponent<ButtonHover>();
		quitHover = quit.GetComponent<ButtonHover>();

		play.gameObject.SetActive(false);
		setting.gameObject.SetActive(false);
		quit.gameObject.SetActive(false);

		back.gameObject.SetActive(false);
		start.gameObject.SetActive(false);

		nextStage.gameObject.SetActive(false);
		previousStage.gameObject.SetActive(false);

		stageSelect.SetActive(false);
		stageName.gameObject.SetActive(false);

		currentStage = 0;

		UpdateStageDisplay();

	}

	public void ShowButtons()
	{
		buttonsShown = true;
		cancelButtonSequence = false;

		if (buttonsSequence != null)
		{
			StopCoroutine(buttonsSequence);
		}

		buttonsSequence = StartCoroutine(ShowButtonsSequence());
	}

	IEnumerator ShowButtonsSequence()
	{
		play.gameObject.SetActive(true);
		yield return StartCoroutine(ZoomButton(play));

		if (cancelButtonSequence)
			yield break;

		setting.gameObject.SetActive(true);
		yield return StartCoroutine(ZoomButton(setting));

		if (cancelButtonSequence)
			yield break;

		quit.gameObject.SetActive(true);
		yield return StartCoroutine(ZoomButton(quit));

		buttonsSequence = null;
	}
	public void ShowButtonsImmediately()
	{
		cancelButtonSequence = false;

		if (buttonsSequence != null)
		{
			StopCoroutine(buttonsSequence);
			buttonsSequence = null;
		}
		playHover.ResetButton();
		settingHover.ResetButton();
		quitHover.ResetButton();

		play.GetComponent<RectTransform>().localScale = playNormalScale;
		setting.GetComponent<RectTransform>().localScale = settingNormalScale;
		quit.GetComponent<RectTransform>().localScale = quitNormalScale;

		play.gameObject.SetActive(true);
		setting.gameObject.SetActive(true);
		quit.gameObject.SetActive(true);

		buttonsShown = true;
	}

	IEnumerator ZoomButton(Button button)
	{
		RectTransform rect = button.GetComponent<RectTransform>();

		Vector3 normalScale;

		if (button == play)
			normalScale = playNormalScale;
		else if (button == setting)
			normalScale = settingNormalScale;
		else
			normalScale = quitNormalScale;

		rect.localScale = Vector3.zero;

		float timer = 0f;
		float duration = 0.25f;

		while (timer < duration)
		{
			if (cancelButtonSequence)
			{
				button.gameObject.SetActive(false);
				yield break;
			}

			timer += Time.deltaTime;

			float t = timer / duration;
			t = Mathf.SmoothStep(0f, 1f, t);

			rect.localScale = Vector3.Lerp(Vector3.zero, normalScale, t);

			yield return null;
		}

		rect.localScale = normalScale;
	}
	public void HideRemainingButtons()
	{
		cancelButtonSequence = true;
		if (buttonsSequence != null)
		{
			StopCoroutine(buttonsSequence);
			buttonsSequence = null;
		}

		play.gameObject.SetActive(false);
		setting.gameObject.SetActive(false);
		quit.gameObject.SetActive(false);
		ResetButton(play);
		ResetButton(setting);
		ResetButton(quit);
	}
	void ResetButton(Button button)
	{
		RectTransform rect = button.GetComponent<RectTransform>();

		rect.localScale = Vector3.zero;
		button.gameObject.SetActive(false);
	}
	public void OnPlayClick()
	{
		HideRemainingButtons();
	}

	public void Play()
	{
		OnPlayClick();
		logo.gameObject.SetActive(false);
		Bg.gameObject.SetActive(true);

		play.gameObject.SetActive(false);
		setting.gameObject.SetActive(false);
		quit.gameObject.SetActive(false);

		back.gameObject.SetActive(true);
		start.gameObject.SetActive(true);

		nextStage.gameObject.SetActive(true);
		previousStage.gameObject.SetActive(true);

		stageSelect.SetActive(true);
		stageName.gameObject.SetActive(true);

		UpdateStageDisplay();
	}

	public void Back()
	{
		logo.gameObject.SetActive(true);
		Bg.gameObject.SetActive(false);

		play.GetComponent<RectTransform>().localScale = playNormalScale;
		setting.GetComponent<RectTransform>().localScale = settingNormalScale;
		quit.GetComponent<RectTransform>().localScale = quitNormalScale;

		play.gameObject.SetActive(true);
		setting.gameObject.SetActive(true);
		quit.gameObject.SetActive(true);

		back.gameObject.SetActive(false);
		start.gameObject.SetActive(false);

		nextStage.gameObject.SetActive(false);
		previousStage.gameObject.SetActive(false);

		stageSelect.SetActive(false);
		stageName.gameObject.SetActive(false);

		playHover.ResetButton();
		settingHover.ResetButton();
		quitHover.ResetButton();
	}

	public void NextStage()
	{
		if (isChangingStage)
			return;

		int next = currentStage + 1;

		if (next >= stageImages.Length)
		{
			next = 0;
		}

		StartCoroutine(ChangeStage(next, 1));
	}
	public void PreviousStage()
	{
		if (isChangingStage)
			return;

		int previous = currentStage - 1;

		if (previous < 0)
		{
			previous = stageImages.Length - 1;
		}

		StartCoroutine(ChangeStage(previous, -1));
	}

	IEnumerator ChangeStage(int newStage, int direction)
	{
		isChangingStage = true;

		Image currentImage = stageImages[currentStage];
		Image nextImage = stageImages[newStage];

		RectTransform currentRect = currentImage.GetComponent<RectTransform>();

		RectTransform nextRect = nextImage.GetComponent<RectTransform>();

		CanvasGroup currentCanvas = currentImage.GetComponent<CanvasGroup>();

		CanvasGroup nextCanvas = nextImage.GetComponent<CanvasGroup>();

		if (currentCanvas == null)
			currentCanvas = currentImage.gameObject.AddComponent<CanvasGroup>();

		if (nextCanvas == null)
			nextCanvas = nextImage.gameObject.AddComponent<CanvasGroup>();

		Vector2 currentStartPosition = currentRect.anchoredPosition;

		Vector2 nextTargetPosition = nextRect.anchoredPosition;

		nextImage.gameObject.SetActive(true);

		Vector2 nextStartPosition = nextTargetPosition +Vector2.right *stageMoveDistance *direction;

		nextRect.anchoredPosition = nextStartPosition;

		nextCanvas.alpha = 0f;

		currentCanvas.alpha = 1f;

		nextImage.transform.SetAsLastSibling();

		float timer = 0f;

		while (timer < stageAnimationDuration)
		{
			timer += Time.deltaTime;

			float t = Mathf.Clamp01(timer / stageAnimationDuration);

			t = Mathf.SmoothStep(0f, 1f, t);

			Vector2 currentEndPosition = currentStartPosition -Vector2.right *stageMoveDistance *direction;

			currentRect.anchoredPosition = Vector2.Lerp(currentStartPosition,currentEndPosition,t);

			currentCanvas.alpha = Mathf.Lerp(1f,0f,t);

			nextRect.anchoredPosition = Vector2.Lerp(nextStartPosition,nextTargetPosition,t);

			nextCanvas.alpha = Mathf.Lerp(0f,1f,t);

			yield return null;
		}

		currentCanvas.alpha = 0f;
		nextCanvas.alpha = 1f;

		currentRect.anchoredPosition = currentStartPosition;

		nextRect.anchoredPosition = nextTargetPosition;

		currentImage.gameObject.SetActive(false);

		currentStage = newStage;

		UpdateStageName();

		isChangingStage = false;
	}

	void UpdateStageDisplay()
	{
		if (stageImages == null || stageImages.Length == 0)
			return;

		for (int i = 0; i < stageImages.Length; i++)
		{
			stageImages[i].gameObject.SetActive(false);
		}

		stageImages[currentStage].gameObject.SetActive(true);

		UpdateStageName();
	}

	void UpdateStageName()
	{
		if (stageName == null)
			return;

		stageName.text = "Stage " + (currentStage + 1);
	}

	public void StartGame()
	{
		if (isChangingStage)
			return;

		if (currentStage >= sceneNames.Length)
			return;

		string sceneToLoad = sceneNames[currentStage];

		start.interactable = false;
		nextStage.interactable = false;
		previousStage.interactable = false;

		if (menuMusic != null)
		{
			menuMusic.Stop();
		}

		if (TransitionManager.Instance == null)
		{
			Debug.LogError("No TransitionManager!");
			return;
		}

		TransitionManager.Instance.StartLoadScene(sceneToLoad);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
