using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    // ==================================================
    // ANOMALY SETUP
    // ==================================================

    [Header("Anomaly Setup")]

    [Tooltip("ลาก Prefab ของ Anomaly มาใส่ตรงนี้")]
    public GameObject[] anomalyPrefabs;

    [Tooltip("ลากจุดเกิด Empty Object มาใส่ตรงนี้")]
    public Transform[] spawnPoints;


    // ==================================================
    // RANDOM SPAWN TIMING
    // ==================================================

    [Header("Random Spawn Timing")]

    [Tooltip("เวลาที่รอก่อนเริ่ม Spawn ตัวแรก")]
    public float delayBeforeFirstSpawn = 5f;

    [Tooltip("เวลาต่ำสุดก่อน Spawn ตัวถัดไป")]
    public float minSpawnInterval = 5f;

    [Tooltip("เวลาสูงสุดก่อน Spawn ตัวถัดไป")]
    public float maxSpawnInterval = 10f;


    // ==================================================
    // SPAWN LIMIT
    // ==================================================

    [Header("Spawn Limit")]

    [Tooltip("จำนวน Anomaly สูงสุดที่อนุญาตให้มีในฉากพร้อมกัน")]
    public int maxActiveAnomalies = 6;


    // ==================================================
    // ANOMALY COUNTER
    // ==================================================

    [Header("Anomaly Counter")]

    [Tooltip("ลาก AnomalyCounter จาก Hierarchy มาใส่ตรงนี้")]
    public AnomalyCounter counterSystem;


    // ==================================================
    // INTERNAL DATA
    // ==================================================

    private List<GameObject> activeAnomalies =
        new List<GameObject>();

    private Coroutine spawnCoroutine;


    // ==================================================
    // START
    // ==================================================

    private void Start()
    {
        // ถ้ายังไม่ได้ลาก Counter มา
        // ให้หาให้อัตโนมัติ
        if (counterSystem == null)
        {
            counterSystem =
                FindFirstObjectByType<AnomalyCounter>();
        }

        // เริ่มระบบ Spawn
        spawnCoroutine =
            StartCoroutine(SpawnLoop());
    }


	// ==================================================
	// SPAWN LOOP
	// ==================================================

	private IEnumerator SpawnLoop()
	{
		// รอจนกว่าเกมจะเข้าสู่ Playing (รวมถึงช่วง Starting)
		while (GameManager.Instance != null &&
			   !GameManager.Instance.IsPlaying())
		{
			// ถ้าเกมจบไปแล้วก่อนเริ่ม ก็ไม่ต้อง spawn
			if (GameManager.Instance.IsGameOver() ||
				GameManager.Instance.IsWin())
				yield break;

			yield return null;
		}

		// เริ่มนับ delay หลังเกมเริ่มจริง
		yield return new WaitForSeconds(delayBeforeFirstSpawn);

		while (true)
		{
			if (GameManager.Instance != null)
			{
				// จบเกมแล้ว หยุดถาวร
				if (GameManager.Instance.IsGameOver() ||
					GameManager.Instance.IsWin())
					yield break;

				// Pause ชั่วคราว: รอจนกลับมา Playing
				if (!GameManager.Instance.IsPlaying())
				{
					yield return null;
					continue;
				}
			}

			CleanupDestroyedAnomalies();

			if (activeAnomalies.Count < maxActiveAnomalies)
				SpawnRandomAnomaly();

			float randomWaitTime =
				Random.Range(minSpawnInterval, maxSpawnInterval);

			yield return new WaitForSeconds(randomWaitTime);
		}
	}


	// ==================================================
	// SPAWN RANDOM ANOMALY
	// ==================================================

	private void SpawnRandomAnomaly()
    {
        // ==========================================
        // ตรวจสอบเกม
        // ==========================================

        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying())
        {
            return;
        }


        // ==========================================
        // ตรวจสอบ Prefab
        // ==========================================

        if (anomalyPrefabs == null ||
            anomalyPrefabs.Length == 0)
        {
            Debug.LogWarning(
                "AnomalyManager ไม่มี Anomaly Prefab!"
            );

            return;
        }


        // ==========================================
        // ตรวจสอบ Spawn Point
        // ==========================================

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "AnomalyManager ไม่มี Spawn Point!"
            );

            return;
        }


        // ==========================================
        // CLEANUP
        // ==========================================

        CleanupDestroyedAnomalies();


        // ==========================================
        // CHECK LIMIT
        // ==========================================

        if (activeAnomalies.Count >= maxActiveAnomalies)
        {
            return;
        }


        // ==========================================
        // RANDOM PREFAB
        // ==========================================

        int randomPrefabIndex =
            Random.Range(
                0,
                anomalyPrefabs.Length
            );


        // ==========================================
        // RANDOM SPAWN POINT
        // ==========================================

        int randomSpawnIndex =
            Random.Range(
                0,
                spawnPoints.Length
            );


        // ==========================================
        // SPAWN
        // ==========================================

        GameObject newAnomaly =
            Instantiate(
                anomalyPrefabs[randomPrefabIndex],
                spawnPoints[randomSpawnIndex].position,
                spawnPoints[randomSpawnIndex].rotation
            );


        // ==========================================
        // ADD TO ACTIVE LIST
        // ==========================================

        activeAnomalies.Add(newAnomaly);


        // ==========================================
        // ANOMALY COUNTER
        // ==========================================

        if (counterSystem != null)
        {
            counterSystem.AnomalySpawned();
        }


        // ==========================================
        // DEBUG
        // ==========================================

        Debug.Log(
            "ANOMALY SPAWNED → " +
            anomalyPrefabs[randomPrefabIndex].name +
            " | Spawn Point: " +
            spawnPoints[randomSpawnIndex].name +
            " | Active: " +
            activeAnomalies.Count +
            "/" +
            maxActiveAnomalies
        );
    }


    // ==================================================
    // CLEANUP DESTROYED ANOMALIES
    // ==================================================

    private void CleanupDestroyedAnomalies()
    {
        activeAnomalies.RemoveAll(
            anomaly => anomaly == null
        );
    }


    // ==================================================
    // STOP COROUTINE
    // ==================================================

    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}