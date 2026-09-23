using UnityEngine;
using System.Collections;

public class AnomalyTarget : MonoBehaviour
{
    [Header("Health")]

    public int health = 10;


    [Header("Ammo Drop")]

    [Range(0f, 1f)]
    public float ammoDropChance = 0.4f;

    public GameObject ammoPickupPrefab;

    [Min(1)]
    public int minAmmoDrop = 5;

    [Min(1)]
    public int maxAmmoDrop = 10;


    [Header("Counter")]

    [Tooltip("ไม่จำเป็นต้องลาก ถ้าว่างระบบจะหาให้อัตโนมัติ")]
    public AnomalyCounter counterSystem;


    private bool isDead = false;


    // ==================================================
    // AWAKE
    // ==================================================

    private void Awake()
    {
        // ==============================================
        // ถ้าไม่ได้ลาก Counter มา
        // ให้หา AnomalyCounter ใน Scene อัตโนมัติ
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
    }


    // ==================================================
    // TAKE DAMAGE
    // ==================================================

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;


        health -= amount;


        Debug.Log(
            gameObject.name +
            " โดนยิง! HP เหลือ: " +
            health
        );


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
            // เผื่อกรณีที่ Counter ถูกสร้าง/เปลี่ยน
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