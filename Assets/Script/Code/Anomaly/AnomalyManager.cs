using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    [Header("Anomaly Setup")]
    [Tooltip("ลาก Prefab ของ Anomaly (แคปซูล/โมเดล/รูปภาพ) มาใส่ตรงนี้ เพิ่มได้เรื่อยๆ")]
    public GameObject[] anomalyPrefabs;

    [Tooltip("ลากจุดเกิด (Empty Object ที่วางตามจุดต่างๆ) มาใส่ตรงนี้")]
    public Transform[] spawnPoints;

    [Header("Spawn Timing")]
    public float delayBeforeFirstSpawn = 30f; // รอ 30 วิแรก
    public float spawnInterval = 10f; // สุ่มเกิดทุกๆ 10 วิ

    private bool gameStarted = false;

    void Start()
    {
        // สั่งให้เริ่มทำงานหลังจากรอ 30 วินาที
        Invoke("StartSpawning", delayBeforeFirstSpawn);
    }

    void StartSpawning()
    {
        gameStarted = true;
        // เริ่มลูปสุ่มเกิด
        InvokeRepeating("SpawnRandomAnomaly", 0f, spawnInterval);
    }

    void SpawnRandomAnomaly()
    {
        if (anomalyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // สุ่มตัว Anomaly และ สุ่มจุดเกิด
        int randomPrefabIndex = Random.Range(0, anomalyPrefabs.Length);
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);

        Instantiate(anomalyPrefabs[randomPrefabIndex], spawnPoints[randomSpawnIndex].position, spawnPoints[randomSpawnIndex].rotation);
        Debug.Log("โผล่มาแล้ว: " + anomalyPrefabs[randomPrefabIndex].name);
    }
}