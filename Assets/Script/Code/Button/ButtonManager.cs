using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
	public Image logo;
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

	private void Start()
	{
		playHover = play.GetComponent<ButtonHover>();
		settingHover = setting.GetComponent<ButtonHover>();
		quitHover = quit.GetComponent<ButtonHover>();

		play.gameObject.SetActive(false);
		setting.gameObject.SetActive(false);
		quit.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
	}

	public void ShowButtons()
	{
		buttonsShown = true;

		StartCoroutine(ShowButtonsSequence());
	}

	IEnumerator ShowButtonsSequence()
	{
		play.gameObject.SetActive(true);
		yield return StartCoroutine(ZoomButton(play));

		setting.gameObject.SetActive(true);
		yield return StartCoroutine(ZoomButton(setting));

		quit.gameObject.SetActive(true);
		yield return StartCoroutine(ZoomButton(quit));
	}

	IEnumerator ZoomButton(Button button)
	{
		RectTransform rect = button.GetComponent<RectTransform>();

		Vector3 normalScale = rect.localScale;

		rect.localScale = Vector3.zero;

		float timer = 0f;
		float duration = 0.25f;

		while (timer < duration)
		{
			timer += Time.deltaTime;

			float t = timer / duration;

			t = Mathf.SmoothStep(0f, 1f, t);

			rect.localScale =Vector3.Lerp(Vector3.zero,normalScale,t);

			yield return null;
		}

		rect.localScale = normalScale;
	}

	public void Play()
	{
		logo.gameObject.SetActive(false);
		play.gameObject.SetActive(false);
		setting.gameObject.SetActive(false);
		quit.gameObject.SetActive(false);
		back.gameObject.SetActive(true);
		start.gameObject.SetActive(true);
	}

	public void Back()
	{
		logo.gameObject.SetActive(true);
		play.gameObject.SetActive(true);
		setting.gameObject.SetActive(true);
		quit.gameObject.SetActive(true);
		back.gameObject.SetActive(false);
		start.gameObject.SetActive(false);

		playHover.ResetButton();
		settingHover.ResetButton();
		quitHover.ResetButton();
	}
	public void StartGame() 
	{
		SceneManager.LoadScene("MainMap");
	}

	public void Quit()
	{
		Application.Quit();
	}
}
