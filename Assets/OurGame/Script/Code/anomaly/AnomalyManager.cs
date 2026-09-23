using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    [Header("Anomaly Setup")]

    [Tooltip("ลาก Prefab ของ Anomaly มาใส่ตรงนี้ เพิ่มได้เรื่อยๆ")]
    public GameObject[] anomalyPrefabs;

    [Tooltip("ลากจุดเกิด Empty Object มาใส่ตรงนี้")]
    public Transform[] spawnPoints;


    [Header("Spawn Timing")]

    public float delayBeforeFirstSpawn = 30f;

    public float spawnInterval = 10f;


    [Header("Spawn Limit")]

    [Tooltip("จำนวน Anomaly สูงสุดที่อนุญาตให้มี ก่อน Game Over")]
    public int maxActiveAnomalies = 6;


    [Header("Anomaly Counter")]

    [Tooltip("ลาก AnomalyCounter จาก Hierarchy มาใส่ตรงนี้")]
    public AnomalyCounter counterSystem;


    // เก็บ Anomaly ที่ถูก Spawn อยู่ในฉาก
    private List<GameObject> activeAnomalies =
        new List<GameObject>();


    private bool gameStarted = false;


    // ==========================================
    // START
    // ==========================================

    void Start()
    {
        // ถ้ายังไม่ได้ลาก Counter มา
        // ให้หาอัตโนมัติ
        if (counterSystem == null)
        {
            counterSystem =
                FindFirstObjectByType<AnomalyCounter>();
        }


        Invoke(
            "StartSpawning",
            delayBeforeFirstSpawn
        );
    }


    // ==========================================
    // START SPAWNING
    // ==========================================

    void StartSpawning()
    {
        gameStarted = true;


        InvokeRepeating(
            "SpawnRandomAnomaly",
            0f,
            spawnInterval
        );
    }


    // ==========================================
    // SPAWN ANOMALY
    // ==========================================

    void SpawnRandomAnomaly()
    {
        // ถ้าเกมจบแล้ว หยุด Spawn
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying())
        {
            return;
        }


        // ตรวจสอบ Prefab
        if (anomalyPrefabs == null ||
            anomalyPrefabs.Length == 0)
        {
            return;
        }


        // ตรวจสอบ Spawn Point
        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return;
        }


        // ลบตัวที่ถูก Destroy แล้วออกจาก List
        CleanupDestroyedAnomalies();


        // ==========================================
        // ถ้ามีครบ 6 ตัวแล้ว
        // ยังไม่ Spawn เพิ่ม
        // ==========================================

        if (activeAnomalies.Count >= maxActiveAnomalies)
        {
            return;
        }


        // ==========================================
        // สุ่ม Anomaly
        // ==========================================

        int randomPrefabIndex =
            Random.Range(
                0,
                anomalyPrefabs.Length
            );


        // ==========================================
        // สุ่มจุดเกิด
        // ==========================================

        int randomSpawnIndex =
            Random.Range(
                0,
                spawnPoints.Length
            );


        // ==========================================
        // Spawn
        // ==========================================

        GameObject newAnomaly =
            Instantiate(
                anomalyPrefabs[randomPrefabIndex],
                spawnPoints[randomSpawnIndex].position,
                spawnPoints[randomSpawnIndex].rotation
            );


        // ==========================================
        // เพิ่มเข้า List
        // ==========================================

        activeAnomalies.Add(newAnomaly);


        // ==========================================
        // แจ้ง AnomalyCounter
        // ==========================================

        if (counterSystem != null)
        {
            counterSystem.AnomalySpawned();
        }


        // ==========================================
        // Debug
        // ==========================================

        Debug.Log(
            "โผล่มาแล้ว: " +
            anomalyPrefabs[randomPrefabIndex].name +
            " | Active: " +
            activeAnomalies.Count +
            "/" +
            maxActiveAnomalies
        );
    }


    // ==========================================
    // CLEANUP
    // ==========================================

    void CleanupDestroyedAnomalies()
    {
        activeAnomalies.RemoveAll(
            anomaly => anomaly == null
        );
    }


    // ==========================================
    // DESTROY
    // ==========================================

    private void OnDestroy()
    {
        CancelInvoke();
    }
}