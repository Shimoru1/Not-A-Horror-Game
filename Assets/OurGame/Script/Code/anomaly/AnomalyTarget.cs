using UnityEngine;
using System.Collections;

public class AnomalyTarget : MonoBehaviour
{
    // ==================================================
    // ANOMALY TYPE
    // ==================================================

    public enum AnomalyType
    {
        Threat,
        Disturbance,
        Harmless
    }


    [Header("Anomaly Type")]

    [Tooltip("กำหนดประเภทของ Anomaly")]
    public AnomalyType anomalyType = AnomalyType.Threat;

    [Tooltip("Anomaly ตัวนี้สามารถถูกยิงได้หรือไม่")]
    public bool canBeShot = true;

    [Tooltip("Anomaly ตัวนี้ต้องใช้ Interaction จัดการหรือไม่")]
    public bool requiresInteraction = false;

    [Tooltip("Anomaly ตัวนี้ไม่มีอันตราย")]
    public bool isHarmless = false;


    // ==================================================
    // HEALTH
    // ==================================================

    [Header("Health")]

    public int health = 10;


    // ==================================================
    // AMMO DROP
    // ==================================================

    [Header("Ammo Drop")]

    [Range(0f, 1f)]
    public float ammoDropChance = 0.4f;

    public GameObject ammoPickupPrefab;

    [Min(1)]
    public int minAmmoDrop = 5;

    [Min(1)]
    public int maxAmmoDrop = 10;


    // ==================================================
    // COUNTER
    // ==================================================

    [Header("Counter")]

    [Tooltip("ไม่จำเป็นต้องลาก ถ้าว่างระบบจะหาให้อัตโนมัติ")]
    public AnomalyCounter counterSystem;


    // ==================================================
    // INTERNAL
    // ==================================================

    private bool isDead = false;


    // ==================================================
    // AWAKE
    // ==================================================

    private void Awake()
    {
        // ==============================================
        // หา Counter อัตโนมัติ
        // ==============================================

        if (counterSystem == null)
        {
            counterSystem =
                FindFirstObjectByType<AnomalyCounter>();
        }


        if (counterSystem == null)
        {
            Debug.LogError(
                gameObject.name +
                " → หา AnomalyCounter ไม่เจอ!"
            );
        }
        else
        {
            Debug.Log(
                gameObject.name +
                " → เชื่อมกับ AnomalyCounter แล้ว"
            );
        }


        // ==============================================
        // ตั้งค่าตาม Anomaly Type
        // ==============================================

        ApplyAnomalyTypeSettings();
    }


    // ==================================================
    // APPLY TYPE SETTINGS
    // ==================================================

    private void ApplyAnomalyTypeSettings()
    {
        switch (anomalyType)
        {
            case AnomalyType.Threat:

                // Threat = สามารถยิงได้
                canBeShot = true;
                requiresInteraction = false;
                isHarmless = false;

                break;


            case AnomalyType.Disturbance:

                // Disturbance = ตอนนี้ยังไม่ทำระบบ Interaction
                // จึงยังไม่ทำลายระบบเดิม
                canBeShot = false;
                requiresInteraction = true;
                isHarmless = false;

                break;


            case AnomalyType.Harmless:

                // Harmless = ไม่ต้องยิง
                canBeShot = false;
                requiresInteraction = false;
                isHarmless = true;

                break;
        }
    }


    // ==================================================
    // TAKE DAMAGE
    // ==================================================

    public void TakeDamage(int amount)
    {
        // ==============================================
        // ถ้าตายแล้ว ไม่รับ Damage ซ้ำ
        // ==============================================

        if (isDead)
            return;


        // ==============================================
        // ถ้ายิงไม่ได้
        // ==============================================

        if (!canBeShot)
        {
            Debug.Log(
                gameObject.name +
                " → Anomaly ตัวนี้ไม่สามารถยิงได้!"
            );

            return;
        }


        // ==============================================
        // ลด HP
        // ==============================================

        health -= amount;


        Debug.Log(
            gameObject.name +
            " โดนยิง! HP เหลือ: " +
            health
        );


        // ==============================================
        // ตรวจสอบการตาย
        // ==============================================

        if (health <= 0)
        {
            isDead = true;

            StartCoroutine(DieWithDelay());
        }
    }


    // ==================================================
    // DIE
    // ==================================================

    private IEnumerator DieWithDelay()
    {
        Debug.Log(
            gameObject.name +
            " ถูกกำจัดแล้ว!"
        );


        // รอ 1 วินาที
        yield return new WaitForSeconds(1f);


        // ==================================================
        // AMMO DROP
        // ==================================================

        float roll = Random.value;


        Debug.Log(
            gameObject.name +
            " Ammo Drop Roll = " +
            roll.ToString("F2") +
            " / Chance = " +
            ammoDropChance.ToString("F2")
        );


        if (roll < ammoDropChance)
        {
            SpawnAmmoPickup();
        }
        else
        {
            Debug.Log(
                gameObject.name +
                " ไม่ดรอปกระสุน"
            );
        }


        // ==================================================
        // ANOMALY COUNTER
        // ==================================================

        if (counterSystem == null)
        {
            // เผื่อกรณี Counter ถูกสร้าง/เปลี่ยน
            // หลังจาก Awake

            counterSystem =
                FindFirstObjectByType<AnomalyCounter>();
        }


        if (counterSystem != null)
        {
            // ลดจำนวน Anomaly ที่อยู่ในฉาก
            counterSystem.AnomalyRemoved();


            // แสดงจำนวนที่กำจัดไปทั้งหมด
            Debug.Log(
                "Total Anomalies Eliminated Tonight: " +
                counterSystem.GetTotalAnomaliesDefeated()
            );
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                " → ไม่สามารถลด Anomaly Counter ได้!"
            );
        }


        // ==================================================
        // DESTROY
        // ==================================================

        Destroy(gameObject);
    }


    // ==================================================
    // SPAWN AMMO PICKUP
    // ==================================================

    private void SpawnAmmoPickup()
    {
        if (ammoPickupPrefab == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " ไม่มี Ammo Pickup Prefab!"
            );

            return;
        }


        Vector3 spawnPosition =
            transform.position +
            Vector3.up * 0.5f;


        GameObject pickup =
            Instantiate(
                ammoPickupPrefab,
                spawnPosition,
                Quaternion.identity
            );


        AmmoPickup ammo =
            pickup.GetComponent<AmmoPickup>();


        if (ammo != null)
        {
            ammo.ammoAmount =
                Random.Range(
                    minAmmoDrop,
                    maxAmmoDrop + 1
                );


            Debug.Log(
                "Ammo Pickup เกิด! +" +
                ammo.ammoAmount +
                " นัด"
            );
        }
        else
        {
            Debug.LogWarning(
                "AmmoPickup Prefab ไม่มี AmmoPickup Script!"
            );
        }
    }
}