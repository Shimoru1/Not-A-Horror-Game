using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AnomalyTarget;

public class AnomalyManager : MonoBehaviour
{
	private readonly Dictionary<AnomalyTarget, GameObject> prefabOf =
		new Dictionary<AnomalyTarget, GameObject>();

	[Header("HP ของแต่ละประเภท")]
	public int combatHealth = 20;
	public int disturbHealth = 20;
	public int protectHealth = 50;

	// ==================================================
	// COMBAT (Object ในฉากที่สลับสภาพ)
	// ==================================================

	[Header("Combat Anomaly (Object ในฉาก)")]
	[Tooltip("ลาก Object ที่มี AnomalyTarget (type = Combat) มาใส่ เช่น รูปภาพต่างๆ")]
	public AnomalyTarget[] combatTargets;

	[Tooltip("โอกาสที่จะสุ่มได้ Combat (0-1) เมื่อมีทั้ง Combat และ Prefab ว่างอยู่")]
	[Range(0f, 1f)] public float combatChance = 0.5f;


	// ==================================================
	// PREFAB ANOMALY (Interact / Disturb / Protect)
	// ==================================================

	[Header("Prefab Anomaly (Interact / Disturb / Protect)")]
	public GameObject[] anomalyPrefabs;

	[Tooltip("จุดเกิด - 1 จุดมี Anomaly ได้ทีละ 1 ตัว")]
	public Transform[] spawnPoints;


	// ==================================================
	// TIMING
	// ==================================================

	[Header("Random Spawn Timing")]
	public float delayBeforeFirstSpawn = 5f;
	public float minSpawnInterval = 5f;
	public float maxSpawnInterval = 10f;


	// ==================================================
	// DISTURB PENALTY
	// ==================================================

	[Header("Disturb Penalty")]
	[Tooltip("จำนวนกระสุนที่โดนหัก (ถ้าสุ่มได้แบบหักกระสุน)")]
	public int disturbAmmoPenalty = 5;


	// ==================================================
	// COUNTER
	// ==================================================

	[Header("Anomaly Counter")]
	public AnomalyCounter counterSystem;


	// ==================================================
	// INTERNAL
	// ==================================================

	private readonly List<AnomalyTarget> activeAnomalies =
		new List<AnomalyTarget>();

	private readonly HashSet<Transform> occupiedPoints =
		new HashSet<Transform>();

	private Coroutine spawnCoroutine;


	private void Start()
	{
		if (counterSystem == null)
			counterSystem = FindFirstObjectByType<AnomalyCounter>();


		if (combatTargets != null)
		{
			foreach (AnomalyTarget t in combatTargets)
			{
				if (t == null) continue;

				if (t.type != AnomalyType.Combat && t.type != AnomalyType.Interact)
				{
					Debug.LogWarning(
						"[AnomalyManager] " + t.name +
						" อยู่ใน combatTargets แต่ type ต้องเป็น Combat หรือ Interact"
					);
				}

				t.InitCombat();
			}
		}

		spawnCoroutine = StartCoroutine(SpawnLoop());
	}

	public int GetMaxHealth(AnomalyType type)
	{
		switch (type)
		{
			case AnomalyType.Combat: return combatHealth;
			case AnomalyType.Disturb: return disturbHealth;
			case AnomalyType.Protect: return protectHealth;
			default: return 1; // Interact ไม่ใช้ HP
		}
	}


	// ==================================================
	// SPAWN LOOP
	// ==================================================

	private IEnumerator SpawnLoop()
	{
		while (GameManager.Instance != null &&
			   !GameManager.Instance.IsPlaying())
		{
			if (GameManager.Instance.IsGameOver() ||
				GameManager.Instance.IsWin())
				yield break;

			yield return null;
		}

		yield return new WaitForSeconds(delayBeforeFirstSpawn);

		while (true)
		{
			if (GameManager.Instance != null)
			{
				if (GameManager.Instance.IsGameOver() ||
					GameManager.Instance.IsWin())
					yield break;

				if (!GameManager.Instance.IsPlaying())
				{
					yield return null;
					continue;
				}
			}

			SpawnRandomAnomaly();

			yield return new WaitForSeconds(
				Random.Range(minSpawnInterval, maxSpawnInterval)
			);
		}
	}


	// ==================================================
	// SPAWN
	// ==================================================

	private void SpawnRandomAnomaly()
	{
		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
			return;

		// Combat ที่ยังว่าง (ยังเป็นปกติอยู่)
		List<AnomalyTarget> freeCombat = new List<AnomalyTarget>();

		if (combatTargets != null)
		{
			foreach (AnomalyTarget t in combatTargets)
			{
				if (t != null && !t.IsActive)
					freeCombat.Add(t);
			}
		}

		// จุดเกิดที่ยังว่าง
		List<Transform> freePoints = new List<Transform>();

		if (spawnPoints != null)
		{
			foreach (Transform p in spawnPoints)
			{
				if (p != null && !occupiedPoints.Contains(p))
					freePoints.Add(p);
			}
		}

		List<GameObject> freePrefabs = new List<GameObject>();

		if (anomalyPrefabs != null)
		{
			foreach (GameObject pf in anomalyPrefabs)
			{
				if (pf != null &&
					!freePrefabs.Contains(pf) &&
					!prefabOf.ContainsValue(pf))
				{
					freePrefabs.Add(pf);
				}
			}
		}

		bool canCombat = freeCombat.Count > 0;

		bool canPrefab = freePoints.Count > 0 && freePrefabs.Count > 0;

		if (!canCombat && !canPrefab)
			return; // ไม่มีที่ว่างแล้ว

		bool spawnCombat =
			canCombat &&
			(!canPrefab || Random.value < combatChance);

		AnomalyTarget target;

		if (spawnCombat)
		{
			target = freeCombat[Random.Range(0, freeCombat.Count)];
			target.Activate(this, null);

			Debug.Log("ANOMALY SPAWNED → Combat | " + target.name);
		}
		else
		{
			Transform point = freePoints[Random.Range(0, freePoints.Count)];
			GameObject prefab = freePrefabs[Random.Range(0, freePrefabs.Count)];

			if (prefab == null)
				return;

			GameObject go =
				Instantiate(prefab, point.position, point.rotation);

			target = go.GetComponent<AnomalyTarget>();

			if (target == null)
				target = go.GetComponentInChildren<AnomalyTarget>();

			if (target == null || target.type == AnomalyType.Combat || target.type == AnomalyType.Interact)
			{
				Debug.LogWarning(
					"[AnomalyManager] Prefab " + prefab.name +
					" ต้องมี AnomalyTarget และ type เป็น Disturb หรือ Protect"
				);

				Destroy(go);
				return;
			}

			prefabOf[target] = prefab;
			occupiedPoints.Add(point);
			target.Activate(this, point);

			Debug.Log(
				"ANOMALY SPAWNED → " + target.type + " | " +
				prefab.name + " @ " + point.name
			);
		}

		activeAnomalies.Add(target);

		if (counterSystem != null)
			counterSystem.AnomalySpawned();
	}


	// ==================================================
	// RESOLVED (เรียกจาก AnomalyTarget)
	// ==================================================

	public void OnAnomalyResolved(AnomalyTarget target,AnomalyResolveReason reason)
	{
		prefabOf.Remove(target);
		activeAnomalies.Remove(target);

		if (target.SpawnPoint != null)
			occupiedPoints.Remove(target.SpawnPoint);

		if (counterSystem != null)
		{
			bool defeated =
				reason == AnomalyResolveReason.Killed ||
				reason == AnomalyResolveReason.Cured;

			counterSystem.AnomalyRemoved(defeated);
		}

		// Combat เป็น Object ในฉาก แค่สลับกลับเป็นปกติ ไม่ลบ
		if (target.type != AnomalyType.Combat &&target.type != AnomalyType.Interact)
			Destroy(target.gameObject);

		Debug.Log(
			"ANOMALY RESOLVED → " + target.type + " (" + reason + ")"
		);
	}


	// ==================================================
	// DISTURB PENALTY: สุ่มอย่างใดอย่างหนึ่ง
	// ==================================================

	public void ApplyDisturbPenalty()
	{
		CCTVManager cctv = CCTVManager.Instance;

		if (cctv == null)
			return;

		// 50% กล้องเปื้อน (ถ้าเปื้อนครบทุกตัวแล้ว จะเปลี่ยนไปหักกระสุนแทน)
		if (Random.value < 0.5f && cctv.MakeRandomCameraDirty())
		{
			Debug.Log("DISTURB PENALTY → กล้องเปื้อน");
			return;
		}

		if (cctv.cameraWeapon != null)
		{
			cctv.cameraWeapon.ReduceAmmo(
				cctv.GetCurrentCameraIndex(),
				disturbAmmoPenalty
			);

			Debug.Log("DISTURB PENALTY → หักกระสุน " + disturbAmmoPenalty);
		}
	}


	private void OnDestroy()
	{
		if (spawnCoroutine != null)
			StopCoroutine(spawnCoroutine);
	}
}