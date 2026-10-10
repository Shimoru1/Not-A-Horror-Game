using UnityEngine;
using TMPro;

public class PowerManager : MonoBehaviour
{
	public static PowerManager Instance;

	[Header("Power Settings")]
	[Tooltip("เปอร์เซ็นต์ไฟเริ่มต้น")]
	[Range(0f, 100f)]
	public float powerPercent = 100f;

	[Tooltip("ไฟลดกี่ % ต่อวินาที")]
	public float powerDrainPerSecond = 1f;

	[Tooltip("ไฟจะดับเมื่อเหลือเท่านี้")]
	public float powerOffPercent = 0f;

	[Header("Repair Settings")]
	[Tooltip("ต้องซ่อมถึงกี่ % ถึงจะเปิดไฟ")]
	public float repairRequiredPercent = 100f;

	[Tooltip("ความเร็วในการซ่อมต่อวินาที")]
	public float repairSpeed = 25f;

	[Tooltip("ระยะที่สามารถซ่อมได้")]
	public float repairDistance = 2.5f;

	[Header("Power Off Vignette")]
	[SerializeField] private GameObject powerOffVignette;

	[Header("Player")]
	public Transform player;

	[Header("Player Camera")]
	[SerializeField] private Camera playerCamera;

	[Header("Power UI")]
	[Tooltip("UI ที่แสดงเปอร์เซ็นต์เฉพาะตอนอยู่ใกล้ Repair Station")]
	[SerializeField] private GameObject powerUI;
	[SerializeField] private TextMeshProUGUI powerText;

	[Header("Power Text Fade Colors")]
	[SerializeField] private Color fullPowerColor = Color.green;
	[SerializeField] private Color emptyPowerColor = Color.red;

	[Header("Repair Target")]
	public GameObject repairStation;

	[Header("Power % Position")]
	[SerializeField] private Vector2 powerTextOffset = Vector2.zero;

	[Header("Repair Indicator")]
	public GameObject repairIcon;

	[Tooltip("UI แสดงปุ่ม E สำหรับซ่อม")]
	public GameObject repairUI;

	[Header("Power Objects")]
	[Tooltip("Object ไฟทั้งหมดในเกม เช่น Light")]
	public Light[] lights;

	[Tooltip("GameObject ที่ต้องปิดตอนเกิดไฟดับ")]
	public GameObject[] powerObjects;

	public bool IsPowerOff { get; private set; }

	private bool isRepairing = false;

	private const string PowerOffSFX = "PowerOffSound";
	private const string PowerOnSFX = "PowerOnSound";
	private const string RepairingSFX = "RepairingSound";

	// =========================================================
	// AWAKE
	// =========================================================

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Debug.LogWarning("[PowerManager] Duplicate instance destroyed.");
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			StopRepairingSound();
			Instance = null;
		}
	}

	// =========================================================
	// START
	// =========================================================

	private void Start()
	{
		IsPowerOff = false;

		HideAllRepairUI();

		if (powerOffVignette != null)
		{
			powerOffVignette.SetActive(false);
		}

		UpdatePowerUI();
	}

	// =========================================================
	// UPDATE
	// =========================================================

	private void Update()
	{
		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
		{
			StopRepairingSound();
			isRepairing = false;
			return;
		}

		HandleRepair();

		if (!isRepairing && !IsPowerOff)
		{
			DrainPower();
		}

		UpdatePowerTextPosition();
		UpdatePowerUI();
	}

	// =========================================================
	// POWER DRAIN
	// =========================================================

	private void DrainPower()
	{
		powerPercent -= powerDrainPerSecond * Time.deltaTime;

		powerPercent = Mathf.Clamp(powerPercent, 0f, 100f);

		if (powerPercent <= powerOffPercent)
		{
			PowerOff();
		}
	}

	// =========================================================
	// POWER OFF
	// =========================================================

	private void PowerOff()
	{
		if (IsPowerOff)
			return;

		IsPowerOff = true;
		powerPercent = 0f;

		isRepairing = false;
		StopRepairingSound();

		Debug.Log("POWER OFF!");

		DisablePowerObjects();

		PlaySFX(PowerOffSFX);

		if (repairStation != null)
		{
			repairStation.SetActive(true);
		}

		if (powerOffVignette != null)
		{
			powerOffVignette.SetActive(true);
		}

		if (CCTVManager.Instance != null)
		{
			CCTVManager.Instance.OnPowerOff();
		}

		UpdatePowerUI();
	}

	// =========================================================
	// TURN OFF LIGHTS
	// =========================================================

	private void DisablePowerObjects()
	{
		if (lights != null)
		{
			foreach (Light lightObject in lights)
			{
				if (lightObject != null)
				{
					lightObject.enabled = false;
				}
			}
		}

		if (powerObjects != null)
		{
			foreach (GameObject obj in powerObjects)
			{
				if (obj != null)
				{
					obj.SetActive(false);
				}
			}
		}
	}

	private void EnablePowerObjects()
	{
		// เปิดไฟทั้งหมดกลับมา
		if (lights != null)
		{
			foreach (Light lightObject in lights)
			{
				if (lightObject != null)
				{
					lightObject.enabled = true;
				}
			}
		}

		// เปิด GameObject ที่ถูกปิดตอนเกิดไฟดับ
		if (powerObjects != null)
		{
			foreach (GameObject obj in powerObjects)
			{
				if (obj != null)
				{
					obj.SetActive(true);
				}
			}
		}
	}

	// =========================================================
	// REPAIR
	// =========================================================

	private void HandleRepair()
	{
		if (player == null || repairStation == null)
		{
			isRepairing = false;
			StopRepairingSound();
			return;
		}

		float distance = Vector3.Distance(
			player.position,
			repairStation.transform.position
		);

		bool nearRepairStation = distance <= repairDistance;

		bool canRepair =
			nearRepairStation &&
			Input.GetKey(KeyCode.E) &&
			powerPercent < repairRequiredPercent;

		if (!canRepair)
		{
			isRepairing = false;
			StopRepairingSound();
			return;
		}

		isRepairing = true;

		powerPercent = Mathf.Min(
			powerPercent + repairSpeed * Time.deltaTime,
			repairRequiredPercent
		);

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlayLoopSFX(RepairingSFX);
		}

		if (powerPercent >= repairRequiredPercent)
		{
			powerPercent = repairRequiredPercent;
			isRepairing = false;

			StopRepairingSound();

			// เรียกเมื่อไฟดับอยู่เท่านั้น
			if (IsPowerOff)
			{
				PowerRestored();
			}
		}
	}

	// =========================================================
	// POWER RESTORED
	// =========================================================

	private void PowerRestored()
	{
		if (!IsPowerOff)
			return;

		Debug.Log("[PowerManager] POWER RESTORED!");

		powerPercent = 100f;
		IsPowerOff = false;
		isRepairing = false;

		// หยุดเฉพาะเสียงซ่อม
		StopRepairingSound();

		// เปิดไฟและอุปกรณ์กลับมา
		EnablePowerObjects();

		// เล่นเสียงเปิดไฟ
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX("PowerOnSound");
		}
		else
		{
			Debug.LogError(
				"[PowerManager] AudioManager.Instance is NULL!"
			);
		}

		if (powerOffVignette != null)
			powerOffVignette.SetActive(false);

		if (CCTVManager.Instance != null)
			CCTVManager.Instance.OnPowerRestored();

		UpdatePowerUI();
	}

	// =========================================================
	// AUDIO MANAGER HELPERS
	// =========================================================

	private void PlaySFX(string id)
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(id);
		}
		else
		{
			Debug.LogWarning(
				"[PowerManager] AudioManager.Instance is null. SFX: " + id
			);
		}
	}

	private void StopRepairingSound()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.StopSFX(RepairingSFX);
		}
	}

	// =========================================================
	// POWER UI
	// =========================================================

	private void UpdatePowerUI()
	{
		if (player == null || repairStation == null)
		{
			HideAllRepairUI();
			return;
		}

		if (CCTVManager.Instance != null &&
			CCTVManager.Instance.isWatchingCCTV)
		{
			HideAllRepairUI();
			return;
		}

		float distance = Vector3.Distance(
			player.position,
			repairStation.transform.position
		);

		bool nearRepairStation = distance <= repairDistance;

		bool showPowerUI = nearRepairStation;

		if (powerUI != null)
		{
			powerUI.SetActive(showPowerUI);
		}

		if (powerText != null)
		{
			powerText.gameObject.SetActive(showPowerUI);

			if (showPowerUI)
			{
				powerText.text =
					Mathf.CeilToInt(powerPercent) + "%";

				UpdatePowerTextColor();
			}
		}

		bool showRepairUI =
			nearRepairStation &&
			powerPercent < repairRequiredPercent;

		if (repairUI != null)
		{
			repairUI.SetActive(showRepairUI);
		}

		bool showRepairIcon =
			IsPowerOff &&
			!nearRepairStation;

		if (repairIcon != null)
		{
			repairIcon.SetActive(showRepairIcon);
		}
	}

	private void UpdatePowerTextColor()
	{
		if (powerText == null)
			return;

		float normalizedPower = Mathf.Clamp01(powerPercent / 100f);

		powerText.color = Color.Lerp(
			emptyPowerColor,
			fullPowerColor,
			normalizedPower
		);
	}

	private void UpdatePowerTextPosition()
	{
		if (powerText == null ||
			repairStation == null ||
			playerCamera == null)
			return;

		Vector3 screenPosition =
			playerCamera.WorldToScreenPoint(
				repairStation.transform.position
			);

		powerText.transform.position =
			screenPosition + (Vector3)powerTextOffset;
	}

	private void HideAllRepairUI()
	{
		if (powerUI != null)
		{
			powerUI.SetActive(false);
		}

		if (powerText != null)
		{
			powerText.gameObject.SetActive(false);
		}

		if (repairUI != null)
		{
			repairUI.SetActive(false);
		}

		if (repairIcon != null)
		{
			repairIcon.SetActive(false);
		}
	}
}