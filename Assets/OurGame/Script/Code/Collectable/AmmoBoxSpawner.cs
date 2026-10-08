using UnityEngine;

public class AmmoBoxSpawner : MonoBehaviour
{
	[Header("Spawn")]
	public GameObject ammoBoxPrefab;
	public Transform[] spawnPoints;
	public float minSpawnTime = 30f;
	public float maxSpawnTime = 40f;
	public float boxLifetime = 30f;

	[Header("Pickup")]
	public int ammoAmount = 60;
	[Tooltip("ติ๊ก = เติมให้ทุกกล้อง / ไม่ติ๊ก = เติมกล้องที่ใช้อยู่ล่าสุด")]
	public bool giveToAllCameras = true;
	public float pickupDistance = 2f;
	public KeyCode pickupKey = KeyCode.E;
	public WeaponController weapon;

	[Header("UI")]
	public GameObject eToPickupUI;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip pickupSound;

	private GameObject activeBox;
	private float spawnTimer;
	private float lifeTimer;
	private int lastPointIndex = -1;
	private Transform player;

	private void Start()
	{
		if (weapon == null)
			weapon = FindFirstObjectByType<WeaponController>();

		GameObject p = GameObject.FindGameObjectWithTag("Player");
		if (p != null)
			player = p.transform;

		if (eToPickupUI != null)
			eToPickupUI.SetActive(false);

		ResetSpawnTimer();
	}

	private void Update()
	{
		// นับเวลาเฉพาะตอนเกมเล่นอยู่ (ไม่นับช่วงสำรวจ/พัก/จบเกม)
		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
		{
			return;
		}

		if (activeBox == null)
		{
			// ไม่มีกล่อง -> นับถอยหลังสุ่มเกิด
			SetPromptActive(false);

			spawnTimer -= Time.deltaTime;

			if (spawnTimer <= 0f)
				SpawnBox();

			return;
		}

		// มีกล่องอยู่ -> ไม่นับเวลาเกิด นับเวลาหายแทน
		lifeTimer -= Time.deltaTime;

		if (lifeTimer <= 0f)
		{
			Debug.Log("Ammo box expired");
			RemoveBox();
			return;
		}

		bool near = IsNearBox();
		SetPromptActive(near);

		if (near && Input.GetKeyDown(pickupKey))
			CollectBox();
	}

	private void SpawnBox()
	{
		if (ammoBoxPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
		{
			Debug.LogWarning("AmmoBoxSpawner: ยังไม่ได้ใส่ Prefab หรือ Spawn Points");
			ResetSpawnTimer();
			return;
		}

		// สุ่มจุดเกิด พยายามไม่ซ้ำจุดเดิม
		int index = Random.Range(0, spawnPoints.Length);

		if (spawnPoints.Length > 1 && index == lastPointIndex)
			index = (index + 1) % spawnPoints.Length;

		lastPointIndex = index;

		Transform point = spawnPoints[index];

		activeBox = Instantiate(
			ammoBoxPrefab, point.position, point.rotation);

		lifeTimer = boxLifetime;

		Debug.Log("Ammo box spawned at " + point.name);
	}

	private void CollectBox()
	{
		if (weapon != null)
		{
			if (giveToAllCameras)
				weapon.AddReserveAmmoToAll(ammoAmount);
			else
				weapon.AddReserveAmmo(ammoAmount);
		}

		if (audioSource != null && pickupSound != null)
			audioSource.PlayOneShot(pickupSound);

		Debug.Log("Ammo box collected +" + ammoAmount);

		RemoveBox();
	}

	private void RemoveBox()
	{
		if (activeBox != null)
			Destroy(activeBox);

		activeBox = null;
		SetPromptActive(false);

		// เริ่มสุ่มเวลาเกิดรอบใหม่หลังกล่องหายไป
		ResetSpawnTimer();
	}

	private void ResetSpawnTimer()
	{
		spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
	}

	private bool IsNearBox()
	{
		if (activeBox == null)
			return false;

		// ไม่ให้เก็บตอนกำลังดูกล้อง (ปุ่ม E ใช้ออกจากกล้อง)
		if (CCTVManager.Instance != null &&
			CCTVManager.Instance.isWatchingCCTV)
		{
			return false;
		}

		if (player == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p == null) return false;
			player = p.transform;
		}

		return Vector3.Distance(
			player.position,
			activeBox.transform.position) <= pickupDistance;
	}

	private void SetPromptActive(bool active)
	{
		if (eToPickupUI != null && eToPickupUI.activeSelf != active)
			eToPickupUI.SetActive(active);
	}
}