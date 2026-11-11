using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
	[System.Serializable]
	public class UITourStep
	{
		[Tooltip("UI ที่จะทำกรอบ (ว่าง = ข้อความกลางจอ)")]
		public RectTransform target;
		[TextArea(2, 4)] public string text;
		[Tooltip("-1 = ใช้ค่า Padding กลาง")]
		public float padding = -1f;
	}

	[System.Serializable]
	public class SpawnEntry
	{
		[Tooltip("แบบ Combat: Object ในฉากที่อยู่ใน AnomalyManager.combatTargets")]
		public AnomalyTarget combatTarget;
		[Tooltip("แบบ Prefab (Disturb): ใส่ prefab + จุดเกิด")]
		public GameObject prefab;
		public Transform point;
	}

	[Header("Player Control")]
	public PlayerMovement playerMovement; 

	private bool prevCanMove = true;

	[Header("References")]
	public AnomalyManager anomalyManager;
	public AmmoBoxSpawner ammoBoxSpawner;
	public WeaponController weapon;
	public Transform player;

	[Header("Waypoints")]
	public Transform computerPoint;
	public Transform ammoBoxPoint;

	[Header("Step Settings")]
	[Tooltip("เวลาให้เดินสำรวจก่อนเริ่มแนะนำเข้ากล้อง (วินาที)")]
	public float exploreDuration = 60f;
	public int secondWaveKillsNeeded = 3;
	[Tooltip("จำนวนอโนมาลี่ที่ทำให้ขึ้นเตือน (ต้องน้อยกว่าจำนวนที่ทำให้แพ้)")]
	public int warningCount = 8;
	public float clickDelay = 0.6f;

	[Header("Wave Setup")]
	public SpawnEntry[] firstWave;
	public SpawnEntry[] secondWave;
	public SpawnEntry[] limitWave;

	[Header("UI Tour (ลำดับการแนะนำ UI ในกล้อง)")]
	public UITourStep[] uiTour;
	public RectTransform ammoUI;
	public RectTransform wipeHintUI;

	[Header("Anomaly Types Panel")]
	public GameObject anomalyTypesPanel;

	[Header("Hint (ข้อความล่างจอ ไม่บล็อกการเล่น)")]
	public GameObject hintRoot;
	public TextMeshProUGUI hintText;

	[Header("Hint Sound")]
	public string hintSoundId = "TaskSound";

	[Header("Spotlight")]
	public GameObject spotlightRoot;
	public RectTransform dimTop, dimBottom, dimLeft, dimRight;
	public GameObject dimFull;
	public RectTransform frame;
	public RectTransform descPanel;
	public TextMeshProUGUI descText;
	public float descPanelWidth = 500f;
	public float padding = 12f;
	public float panelGap = 20f;

	[Header("Waypoint Icon")]
	public Camera playerCamera;
	public bool clampToScreen = true;
	public RectTransform waypointIcon;
	public TextMeshProUGUI waypointDistanceText;
	public float edgeMargin = 70f;
	public float waypointHeight = 0f;
	public Vector2 waypointScreenOffset = Vector2.zero;

	[Header("Search Assist (เรืองแสงเมื่อหาไม่เจอ)")]
	public float glowAfterSeconds = 60f;
	public Color glowColor = new Color(1f, 0.85f, 0.2f);
	public float glowIntensity = 2.5f;
	public float glowSpeed = 4f;
	public bool glowAddLight = true;

	private bool searchAssist = false;
	private bool glowing = false;
	private float searchTimer = 0f;
	private int lastDefeated = 0;

	private Transform waypointTarget;
	private Canvas rootCanvas;
	private RectTransform rootRT;
	private Camera rootCam;

	private bool monitorLowAmmo = false;
	private bool wasLowAmmo = false;

	// =========================================================
	// SETUP
	// =========================================================

	private static Camera CanvasCamera(Canvas c)
	{
		if (c == null || c.renderMode == RenderMode.ScreenSpaceOverlay)
			return null;

		return c.worldCamera;
	}

	private void Awake()
	{
		if (spotlightRoot != null)
		{
			rootRT = spotlightRoot.GetComponent<RectTransform>();

			Canvas c = spotlightRoot.GetComponentInParent<Canvas>();
			if (c != null)
			{
				rootCanvas = c.rootCanvas;
				rootCam = CanvasCamera(rootCanvas);
			}
		}

		if (anomalyTypesPanel != null) anomalyTypesPanel.SetActive(false);

		HideHint();
		if (spotlightRoot != null) spotlightRoot.SetActive(false);
		if (waypointIcon != null) waypointIcon.gameObject.SetActive(false);
	}

	private IEnumerator Start()
	{
		if (player == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p != null) player = p.transform;
		}
		if (playerMovement == null && CCTVManager.Instance != null)
			playerMovement = CCTVManager.Instance.playerMovement;

		if (playerMovement == null)
			playerMovement = FindFirstObjectByType<PlayerMovement>();

		// ยังเข้ากล้องไม่ได้ จนกว่าจะถึงขั้นนำทางไปคอมพิวเตอร์
		if (CCTVManager.Instance != null)
			CCTVManager.Instance.canEnterCCTV = false;

		while (GameManager.Instance == null || !GameManager.Instance.IsPlaying())
			yield return null;

		yield return new WaitForSeconds(0.5f);

		yield return Step_Welcome();
		yield return Step_Explore();
		yield return Step_GoToComputer();
		yield return Step_UITour();
		yield return Step_AnomalyTypes();
		yield return Step_FirstWave();
		yield return Step_AmmoBox();
		yield return Step_Dirty();
		yield return Step_SecondWave();
		yield return Step_PowerCut();
		yield return Step_LimitWave();
		yield return Step_Win();
	}

	private void Update()
	{
		UpdateSearchAssist();
		MonitorLowAmmo();
		UpdateWaypoint();
	}

	// ทุกครั้งที่ขึ้นเตือนกระสุนใกล้หมด ให้เกิดกล่องกระสุนทันที
	private void MonitorLowAmmo()
	{
		if (!monitorLowAmmo || weapon == null || ammoBoxSpawner == null)
			return;

		bool low = weapon.IsLowAmmo;

		if (low && !wasLowAmmo && ammoBoxSpawner.ActiveBox == null)
			ammoBoxSpawner.SpawnBoxAt(ammoBoxPoint);

		wasLowAmmo = low;
	}

	// =========================================================
	// STEPS
	// =========================================================

	private IEnumerator Step_Welcome()
	{
		yield return Dialog("Welcome to The Normally Gallery.");
	}

	private IEnumerator Step_Explore()
	{
		SetHint("Try to explore by pressing W A S D.");

		float timer = 0f;

		while (timer < exploreDuration)
		{
			timer += Time.deltaTime;
			yield return null;
		}

		HideHint();
	}

	private IEnumerator Step_GoToComputer()
	{
		if (CCTVManager.Instance != null)
			CCTVManager.Instance.canEnterCCTV = true;

		SetWaypoint(computerPoint);
		SetHint("Walk to Security room and E to use camera.");

		yield return new WaitUntil(() => CCTVManager.Instance.isWatchingCCTV);

		ClearWaypoint();
		HideHint();
	}

	private IEnumerator Step_UITour()
	{
		yield return new WaitForSeconds(0.5f);
		yield return Tour(new List<UITourStep>(uiTour));
	}

	private IEnumerator Step_AnomalyTypes()
	{
		if (anomalyTypesPanel == null)
			yield break;

		BeginFreeze();

		anomalyTypesPanel.SetActive(true);

		yield return WaitForClick();

		anomalyTypesPanel.SetActive(false);

		yield return EndFreeze();
	}

	private IEnumerator Step_FirstWave()
	{
		int spawned = SpawnWave(firstWave);

		monitorLowAmmo = true;
		wasLowAmmo = false;

		SetHint("A/D to switch cam, Find and shot it.");

		while (true)
		{
			if (weapon.IsLowAmmo || weapon.magazineAmmo <= 0)
				break;

			// กำจัดครบแต่กระสุนยังไม่ใกล้หมด -> หักกระสุนให้ถึงจุดเตือน
			if (spawned > 0 && anomalyManager.ActiveCount == 0)
			{
				int excess = weapon.magazineAmmo - weapon.lowAmmoThreshold;
				if (excess > 0) weapon.ReduceAmmo(0, excess);
			}

			yield return null;
		}

		HideHint();

		RectTransform ammoRect = ammoUI != null
			? ammoUI
			: (weapon.ammoText != null ? weapon.ammoText.rectTransform : null);

		yield return Dialog("Running out of bullets.", ammoRect);
	}

	private IEnumerator Step_AmmoBox()
	{
		// ปกติ MonitorLowAmmo เกิดกล่องไปแล้ว (ถ้ามีกล่องอยู่จะไม่เกิดซ้ำ)
		ammoBoxSpawner.SpawnBoxAt(ammoBoxPoint);

		SetWaypoint(ammoBoxPoint);
		SetHint("Leave the cam by pressing E and find the ammo box.");

		yield return new WaitUntil(() => ammoBoxSpawner.ActiveBox == null);

		SetWaypoint(computerPoint);
		SetHint("You collected it, back to Security room.");

		yield return new WaitUntil(() => CCTVManager.Instance.isWatchingCCTV);

		ClearWaypoint();
		HideHint();
	}

	private IEnumerator Step_Dirty()
	{
		yield return new WaitForSeconds(0.5f);

		int dirtyIndex = CCTVManager.Instance.MakeCurrentCameraDirty();
		if (dirtyIndex < 0) yield break;

		yield return new WaitForSeconds(0.3f);

		yield return Dialog("Cam is dirted!.", wipeHintUI);

		SetHint("Repeat pressing F to clean.");

		yield return new WaitUntil(() => !CCTVManager.Instance.IsCameraDirty(dirtyIndex));

		HideHint();
	}

	private IEnumerator Step_SecondWave()
	{
		yield return new WaitForSeconds(0.8f);

		// ตัวจาก First wave ที่ยังค้างอยู่ในแมพ
		int leftover = anomalyManager.ActiveCount;
		int baseline = anomalyManager.DefeatedCount;

		// ตัวที่ active อยู่แล้วจะถูกข้ามอัตโนมัติ (ไม่ซ้อนทับกัน)
		SpawnWave(secondWave);
		BeginSearchAssist();

		bool clearAll = leftover > 0;
		int lastShown = -1;

		while (true)
		{
			int active = anomalyManager.ActiveCount;
			int done = anomalyManager.DefeatedCount - baseline;

			if (clearAll)
			{
				if (active == 0) break;
			}
			else
			{
				if (done >= secondWaveKillsNeeded) break;
			}

			int shown = clearAll ? active : secondWaveKillsNeeded - done;

			if (shown != lastShown)
			{
				bool first = lastShown == -1;
				lastShown = shown;

				if (clearAll)
					SetHint("Destroy anomalies (" + shown + " left)", first);
				else
					SetHint("Destroy " + shown + " more anomalies.", first);
			}

			yield return null;
		}

		EndSearchAssist();
		HideHint();
	}

	private IEnumerator Step_PowerCut()
	{
		PowerManager.Instance.ForcePowerOff();

		ClearWaypoint(); 

		yield return new WaitForSeconds(1f);

		yield return Dialog("Power outaged! unable to use cam.");

		SetHint("Leave the cam and go fix at power station.");

		yield return new WaitUntil(() => !PowerManager.Instance.IsPowerOff);

		ClearWaypoint();
		HideHint();
	}

	private IEnumerator Step_LimitWave()
	{
		yield return new WaitForSeconds(1f);

		foreach (SpawnEntry e in limitWave)
		{
			if (anomalyManager.ActiveCount >= warningCount) break;
			SpawnOne(e);
		}

		BeginSearchAssist();

		SetHint("Many anomalies here! go destroy them quick!");
		yield return new WaitForSeconds(3.5f);

		if (!CCTVManager.Instance.isWatchingCCTV)
			SetWaypoint(computerPoint);

		SetHint("Find and Destroy them.");

		yield return new WaitUntil(() => anomalyManager.ActiveCount == 0);

		EndSearchAssist();
		HideHint();
		ClearWaypoint();
	}

	private IEnumerator Step_Win()
	{
		yield return new WaitForSeconds(1f);
		GameManager.Instance.WinGame();
	}

	// =========================================================
	// SPAWN
	// =========================================================

	private int SpawnWave(SpawnEntry[] wave)
	{
		int n = 0;
		if (wave == null) return 0;

		foreach (SpawnEntry e in wave)
			if (SpawnOne(e)) n++;

		return n;
	}

	private bool SpawnOne(SpawnEntry e)
	{
		if (e == null) return false;

		if (e.combatTarget != null)
			return anomalyManager.SpawnCombatTarget(e.combatTarget);

		return anomalyManager.SpawnPrefabAt(e.prefab, e.point);
	}

	// =========================================================
	// FREEZE (หยุดเกมตอนแสดงกรอบ แต่ยังกด Pause Menu ได้)
	// =========================================================

	private void BeginFreeze()
	{
		HideHint();

		// ล็อกผู้เล่น (จำค่าเดิมไว้ เพราะตอนอยู่ในกล้อง canMove เป็น false อยู่แล้ว)
		if (playerMovement != null)
		{
			prevCanMove = playerMovement.canMove;
			playerMovement.canMove = false;
			playerMovement.StopMovement();
		}

		if (PauseMenu.Instance != null)
			PauseMenu.Instance.tutorialFreeze = true;

		// หยุดเกมแต่ให้เพลงเล่นต่อ
		GameManager.Instance.PauseGameKeepMusic();
	}

	private IEnumerator EndFreeze()
	{
		// รอ 1 เฟรม ไม่งั้นคลิกสุดท้ายจะกลายเป็นการยิง
		yield return null;

		if (PauseMenu.Instance != null)
			PauseMenu.Instance.tutorialFreeze = false;

		GameManager.Instance.ResumeGame();

		if (playerMovement != null)
			playerMovement.canMove = prevCanMove;
	}

	private bool IsPauseMenuOpen()
	{
		return PauseMenu.Instance != null && PauseMenu.Instance.IsPaused;
	}

	// รอคลิก โดยไม่นับคลิกที่ทำในเมนู Pause
	private IEnumerator WaitForClick()
	{
		yield return new WaitForSecondsRealtime(clickDelay);

		while (!Input.GetMouseButtonDown(0) || IsPauseMenuOpen())
			yield return null;
	}

	// =========================================================
	// DIALOG / SPOTLIGHT
	// =========================================================

	private IEnumerator Dialog(string text, RectTransform target = null)
	{
		var list = new List<UITourStep>
		{
			new UITourStep { target = target, text = text }
		};

		yield return Tour(list);
	}

	private IEnumerator Tour(IList<UITourStep> steps)
	{
		BeginFreeze();

		foreach (UITourStep s in steps)
		{
			ShowSpotlight(s);
			yield return WaitForClick();
		}

		spotlightRoot.SetActive(false);

		yield return EndFreeze();
	}

	private void ShowSpotlight(UITourStep step)
	{
		RectTransform target = step.target;
		float pad = step.padding >= 0f ? step.padding : padding;

		spotlightRoot.SetActive(true);
		descText.text = step.text;

		Canvas.ForceUpdateCanvases();

		float W = rootRT.rect.width;
		float H = rootRT.rect.height;

		bool hasTarget = target != null && target.gameObject.activeInHierarchy;

		dimFull.SetActive(!hasTarget);
		dimTop.gameObject.SetActive(hasTarget);
		dimBottom.gameObject.SetActive(hasTarget);
		dimLeft.gameObject.SetActive(hasTarget);
		dimRight.gameObject.SetActive(hasTarget);
		frame.gameObject.SetActive(hasTarget);

		Rect r = default;

		if (hasTarget)
		{
			Canvas tc = target.GetComponentInParent<Canvas>();
			if (tc != null) tc = tc.rootCanvas;
			Camera targetCam = CanvasCamera(tc);

			Vector3[] corners = new Vector3[4];
			target.GetWorldCorners(corners);

			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(float.MinValue, float.MinValue);

			for (int i = 0; i < 4; i++)
			{
				Vector2 screen = RectTransformUtility.WorldToScreenPoint(targetCam, corners[i]);

				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					rootRT, screen, rootCam, out Vector2 local);

				// เทียบกับมุมซ้ายล่างของ SpotlightRoot
				local -= rootRT.rect.min;

				min = Vector2.Min(min, local);
				max = Vector2.Max(max, local);
			}

			r = Rect.MinMaxRect(min.x - pad, min.y - pad, max.x + pad, max.y + pad);

			SetRect(dimBottom, 0, 0, W, r.yMin);
			SetRect(dimTop, 0, r.yMax, W, H - r.yMax);
			SetRect(dimLeft, 0, r.yMin, r.xMin, r.height);
			SetRect(dimRight, r.xMax, r.yMin, W - r.xMax, r.height);
			SetRect(frame, r.xMin, r.yMin, r.width, r.height);
		}

		// กล่องคำอธิบาย
		descPanel.anchorMin = descPanel.anchorMax = Vector2.zero;
		descPanel.pivot = Vector2.zero;
		descPanel.sizeDelta = new Vector2(descPanelWidth, descPanel.sizeDelta.y);
		LayoutRebuilder.ForceRebuildLayoutImmediate(descPanel);

		float pw = descPanel.rect.width;
		float ph = descPanel.rect.height;
		float x, y;

		if (hasTarget)
		{
			x = Mathf.Clamp(r.center.x - pw * 0.5f, 10f, W - pw - 10f);

			bool placeBelow = r.center.y > H * 0.5f;
			y = placeBelow ? r.yMin - panelGap - ph : r.yMax + panelGap;
			y = Mathf.Clamp(y, 10f, H - ph - 10f);
		}
		else
		{
			x = (W - pw) * 0.5f;
			y = (H - ph) * 0.5f;
		}

		descPanel.anchoredPosition = new Vector2(x, y);
	}

	private static void SetRect(RectTransform rt, float x, float y, float w, float h)
	{
		rt.anchorMin = rt.anchorMax = Vector2.zero;
		rt.pivot = Vector2.zero;
		rt.anchoredPosition = new Vector2(x, y);
		rt.sizeDelta = new Vector2(Mathf.Max(0f, w), Mathf.Max(0f, h));
	}

	// =========================================================
	// HINT
	// =========================================================

	private void SetHint(string text, bool playSound = true)
	{
		bool isNew =
			hintRoot == null || !hintRoot.activeSelf ||
			hintText == null || hintText.text != text;

		if (hintRoot != null) hintRoot.SetActive(true);
		if (hintText != null) hintText.text = text;

		if (playSound && isNew &&
			AudioManager.Instance != null &&
			!string.IsNullOrEmpty(hintSoundId))
		{
			AudioManager.Instance.PlaySFX(hintSoundId);
		}
	}

	private void HideHint()
	{
		if (hintRoot != null) hintRoot.SetActive(false);
	}

	// =========================================================
	// WAYPOINT
	// =========================================================

	private void SetWaypoint(Transform t) { waypointTarget = t; }

	private void ClearWaypoint()
	{
		waypointTarget = null;
		if (waypointIcon != null) waypointIcon.gameObject.SetActive(false);
	}

	private void UpdateWaypoint()
	{
		if (waypointIcon == null) return;

		Camera cam = playerCamera != null ? playerCamera : Camera.main;

		// ถ้าไม่มีจุดหมายจาก step แต่มีกล่องกระสุนอยู่ ให้ชี้ไปที่กล่อง
		Transform target = waypointTarget;

		if (target == null &&
			ammoBoxSpawner != null &&
			ammoBoxSpawner.ActiveBox != null)
		{
			target = ammoBoxSpawner.ActiveBox.transform;
		}

		bool hide = target == null || cam == null || !cam.isActiveAndEnabled || (PowerManager.Instance != null && PowerManager.Instance.IsPowerOff) || (CCTVManager.Instance != null && CCTVManager.Instance.isWatchingCCTV) || (GameManager.Instance != null && !GameManager.Instance.IsPlaying());

		if (hide)
		{
			waypointIcon.gameObject.SetActive(false);
			return;
		}

		Vector3 sp = cam.WorldToScreenPoint(
			target.position + Vector3.up * waypointHeight);

		bool behind = sp.z < 0f;

		if (clampToScreen)
		{
			if (behind)
			{
				sp.x = Screen.width - sp.x;
				sp.y = Screen.height - sp.y;
			}

			sp.x += waypointScreenOffset.x;
			sp.y += waypointScreenOffset.y;

			sp.x = Mathf.Clamp(sp.x, edgeMargin, Screen.width - edgeMargin);
			sp.y = Mathf.Clamp(sp.y, edgeMargin, Screen.height - edgeMargin);
		}
		else
		{
			// แบบ PowerManager: ตรงที่วัตถุเท่านั้น ถ้าอยู่นอกจอให้ซ่อน
			sp.x += waypointScreenOffset.x;
			sp.y += waypointScreenOffset.y;

			bool offScreen =
				behind ||
				sp.x < 0f || sp.x > Screen.width ||
				sp.y < 0f || sp.y > Screen.height;

			if (offScreen)
			{
				waypointIcon.gameObject.SetActive(false);
				return;
			}
		}

		sp.z = 0f;

		waypointIcon.gameObject.SetActive(true);
		waypointIcon.position = sp;

		if (waypointDistanceText != null && player != null)
		{
			float d = Vector3.Distance(player.position, target.position);
			waypointDistanceText.text = Mathf.RoundToInt(d) + " m";
		}
	}

	// =========================================================
	// SEARCH ASSIST
	// =========================================================

	private void BeginSearchAssist()
	{
		searchAssist = true;
		glowing = false;
		searchTimer = 0f;
		lastDefeated = anomalyManager.DefeatedCount;
	}

	private void EndSearchAssist()
	{
		searchAssist = false;
		glowing = false;
		searchTimer = 0f;
		ClearGlow();
	}

	private void UpdateSearchAssist()
	{
		if (!searchAssist || glowing) return;

		if (GameManager.Instance == null || !GameManager.Instance.IsPlaying())
			return;

		// กำจัดได้ตัวใหม่ = ผู้เล่นหาเจอแล้ว เริ่มนับใหม่
		int defeated = anomalyManager.DefeatedCount;
		if (defeated != lastDefeated)
		{
			lastDefeated = defeated;
			searchTimer = 0f;
		}

		if (anomalyManager.ActiveCount == 0)
		{
			searchTimer = 0f;
			return;
		}

		searchTimer += Time.deltaTime;

		if (searchTimer >= glowAfterSeconds)
			GlowAllActive();
	}

	private void GlowAllActive()
	{
		glowing = true;

		foreach (AnomalyTarget t in anomalyManager.ActiveAnomalies)
		{
			if (t == null) continue;

			AnomalyGlow g = t.GetComponentInChildren<AnomalyGlow>();
			if (g == null) g = t.gameObject.AddComponent<AnomalyGlow>();

			g.StartGlow(glowColor, glowIntensity, glowSpeed, glowAddLight);
		}
	}

	private void ClearGlow()
	{
		foreach (AnomalyTarget t in anomalyManager.ActiveAnomalies)
		{
			if (t == null) continue;

			AnomalyGlow g = t.GetComponentInChildren<AnomalyGlow>();
			if (g != null) g.StopGlow();
		}
	}
}