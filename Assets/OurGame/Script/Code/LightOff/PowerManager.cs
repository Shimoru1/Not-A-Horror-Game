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

		if (powerUI != null)
		{
			powerUI.SetActive(false);
		}

		if (repairIcon != null)
		{
			repairIcon.SetActive(false);
		}

		if (repairUI != null)
		{
			repairUI.SetActive(false);
		}

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

		if (!IsPowerOff)
		{
			DrainPower();
		}

		// สามารถซ่อมได้ทั้งตอนเปิดและตอนดับ
		HandleRepair();

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

		// เสียงไฟดับ
		if (powerOffAudio != null)
		{
			powerOffAudio.Play();
		}

		// RepairStation ต้องอยู่ในฉาก
		if (repairStation != null)
		{
			repairStation.SetActive(true);
		}

		if (powerOffVignette != null)
		{
			powerOffVignette.SetActive(true);
		}

		// PowerUI จะถูกเปิดเฉพาะเมื่ออยู่ใกล้ Station
		UpdatePowerUI();

		// แจ้ง CCTV
		if (CCTVManager.Instance != null)
		{
			CCTVManager.Instance.OnPowerOff();
		}
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
			return;
		}

		float distance = Vector3.Distance(
			player.position,
			repairStation.transform.position
		);

		bool nearRepairStation =
			distance <= repairDistance;


		// =====================================================
		// อยู่ใกล้ + กด E ค้าง + Power ยังไม่เต็ม
		// =====================================================

		if (nearRepairStation &&
			Input.GetKey(KeyCode.E) &&
			powerPercent < repairRequiredPercent)
		{
			isRepairing = true;

			// เพิ่ม %
			powerPercent +=
				repairSpeed * Time.deltaTime;

			powerPercent = Mathf.Clamp(
				powerPercent,
				0f,
				repairRequiredPercent
			);


			// เสียงซ่อม
			if (repairAudio != null &&
				!repairAudio.isPlaying)
			{
				repairAudio.Play();
			}


			// ถ้าซ่อมจนเต็ม
			if (powerPercent >= repairRequiredPercent)
			{
				powerPercent = repairRequiredPercent;

				// ถ้าไฟดับอยู่ ให้เปิดไฟกลับ
				if (IsPowerOff)
				{
					PowerRestored();
				}
				else
				{
					// ไฟยังไม่ดับ แค่ซ่อมจนเต็ม
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

			// หยุดเสียงซ่อม
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

		// เปิดไฟกลับ
		EnablePowerObjects();

		// หยุดเสียงซ่อม
		if (repairAudio != null)
		{
			repairAudio.Stop();
		}

		// เสียงไฟกลับมา
		if (powerOnAudio != null)
		{
			powerOnAudio.Play();
		}

		// ซ่อน Power %
		if (powerUI != null)
		{
			powerUI.SetActive(false);
		}

		// ซ่อนไอคอน
		if (repairIcon != null)
		{
			repairIcon.SetActive(false);
		}

		if (powerOffVignette != null)
		{
			powerOffVignette.SetActive(false);
		}

		// แจ้ง CCTV
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
		if (powerUI == null)
			return;

		if (player == null || repairStation == null)
		{
			powerUI.SetActive(false);

			if (repairIcon != null)
				repairIcon.SetActive(false);

			if (repairUI != null)
				repairUI.SetActive(false);

			return;
		}


		// =====================================================
		// อยู่ใน CCTV
		// =====================================================

		if (CCTVManager.Instance != null &&
			CCTVManager.Instance.isWatchingCCTV)
		{
			powerUI.SetActive(false);

			if (repairIcon != null)
				repairIcon.SetActive(false);

			if (repairUI != null)
				repairUI.SetActive(false);

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

		if (nearRepairStation)
		{
			powerUI.SetActive(true);

			if (powerText != null)
			{
				powerText.gameObject.SetActive(true);

				powerText.text =
					Mathf.CeilToInt(powerPercent) + "%";

				UpdatePowerTextColor();
			}
		}
		else
		{
			powerUI.SetActive(false);
		}


		// =====================================================
		// REPAIR E UI
		// =====================================================

		if (repairUI != null)
		{
			// แสดง E เมื่ออยู่ใกล้
			// และ Power ยังไม่เต็ม
			if (nearRepairStation &&
				powerPercent < repairRequiredPercent)
			{
				repairUI.SetActive(true);
			}
			else
			{
				repairUI.SetActive(false);
			}
		}


		// =====================================================
		// REPAIR ICON
		// =====================================================

		if (repairIcon != null)
		{
			// ถ้าไฟดับ + อยู่นอกระยะ
			if (IsPowerOff && !nearRepairStation)
			{
				repairIcon.SetActive(true);
			}
			else
			{
				repairIcon.SetActive(false);
			}
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
		if (powerText == null || repairStation == null)
			return;

		if (playerCamera == null)
			return;

		// ใช้ Pivot ของ RepairStation
		Vector3 screenPosition =
			playerCamera.WorldToScreenPoint(
				repairStation.transform.position
			);

		powerText.transform.position = screenPosition;
	}

}
