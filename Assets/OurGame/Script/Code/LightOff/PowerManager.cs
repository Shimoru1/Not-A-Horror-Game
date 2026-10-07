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

	[Header("Power Off Sound")]
	public AudioSource powerOffAudio;

	[Header("Repair Sound")]
	public AudioSource repairAudio;

	[Header("Power On Sound")]
	public AudioSource powerOnAudio;

	public bool IsPowerOff { get; private set; }

	private bool isRepairing = false;

	// =========================================================
	// AWAKE
	// =========================================================

	private void Awake()
	{
		Instance = this;
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
		powerPercent -=
			powerDrainPerSecond * Time.deltaTime;

		powerPercent =
			Mathf.Clamp(powerPercent, 0f, 100f);

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

		Debug.Log("POWER OFF!");

		DisablePowerObjects();

		if (powerOffAudio != null)
		{
			powerOffAudio.Play();
		}

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
		// ปิด Light
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

		// ปิด Object อื่น
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

	// =========================================================
	// REPAIR
	// =========================================================

	private void HandleRepair()
	{
		if (player == null ||
			repairStation == null)
		{
			isRepairing = false;
			return;
		}


		// =====================================================
		// ตรวจระยะ
		// =====================================================

		float distance = Vector3.Distance(
			player.position,
			repairStation.transform.position
		);

		bool nearRepairStation =
			distance <= repairDistance;


		// =====================================================
		// ต้องอยู่ใกล้ + กด E + Power ยังไม่เต็ม
		// =====================================================

		if (nearRepairStation &&
			Input.GetKey(KeyCode.E) &&
			powerPercent < repairRequiredPercent)
		{
			isRepairing = true;


			// =================================================
			// เพิ่ม Power
			// =================================================

			powerPercent +=
				repairSpeed * Time.deltaTime;

			powerPercent = Mathf.Clamp(
				powerPercent,
				0f,
				repairRequiredPercent
			);


			// =================================================
			// เสียงซ่อม
			// =================================================

			if (repairAudio != null &&
				!repairAudio.isPlaying)
			{
				repairAudio.Play();
			}


			// =================================================
			// ซ่อมเต็ม
			// =================================================

			if (powerPercent >= repairRequiredPercent)
			{
				powerPercent =
					repairRequiredPercent;

				// ถ้าไฟดับ → เปิดไฟกลับ
				if (IsPowerOff)
				{
					PowerRestored();
				}
				else
				{
					// ไฟยังไม่ดับ แค่ซ่อมเต็ม
					isRepairing = false;

					if (repairAudio != null)
					{
						repairAudio.Stop();
					}
				}
			}
		}
		else
		{
			isRepairing = false;


			// =================================================
			// หยุดเสียงซ่อม
			// =================================================

			if (repairAudio != null &&
				repairAudio.isPlaying)
			{
				repairAudio.Stop();
			}
		}
	}


	// =========================================================
	// POWER RESTORED
	// =========================================================

	private void PowerRestored()
	{
		Debug.Log("POWER RESTORED!");

		powerPercent = 100f;

		IsPowerOff = false;

		isRepairing = false;

		EnablePowerObjects();

		if (repairAudio != null)
		{
			repairAudio.Stop();
		}

		if (powerOnAudio != null)
		{
			powerOnAudio.Play();
		}

		if (powerOffVignette != null)
		{
			powerOffVignette.SetActive(false);
		}

		if (CCTVManager.Instance != null)
		{
			CCTVManager.Instance.OnPowerRestored();
		}

		UpdatePowerUI();
	}

	// =========================================================
	// TURN ON LIGHTS
	// =========================================================

	private void EnablePowerObjects()
	{
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
	// POWER UI
	// =========================================================

	private void UpdatePowerUI()
	{
		// =====================================================
		// ตรวจ Reference
		// =====================================================

		if (player == null || repairStation == null)
		{
			HideAllRepairUI();
			return;
		}

		// =====================================================
		// CCTV
		// =====================================================

		if (CCTVManager.Instance != null &&
			CCTVManager.Instance.isWatchingCCTV)
		{
			HideAllRepairUI();
			return;
		}

		// =====================================================
		// ตรวจระยะ
		// =====================================================

		float distance = Vector3.Distance(
			player.position,
			repairStation.transform.position
		);

		bool nearRepairStation =
			distance <= repairDistance;

		// =====================================================
		// POWER %
		// =====================================================

		bool showPowerUI =
			nearRepairStation;

		if (powerUI != null)
		{
			powerUI.SetActive(showPowerUI);
		}

		// สำคัญมาก!
		// สั่ง powerText โดยตรงด้วย
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

		// =====================================================
		// REPAIR E UI
		// =====================================================

		bool showRepairUI =
			nearRepairStation &&
			powerPercent < repairRequiredPercent;


		if (repairUI != null)
		{
			repairUI.SetActive(showRepairUI);
		}

		// =====================================================
		// REPAIR ICON
		// =====================================================

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

		// แปลง 0-100 ให้เป็น 0-1
		float normalizedPower =
			Mathf.Clamp01(powerPercent / 100f);

		// 0% = แดง
		// 100% = เขียว
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
