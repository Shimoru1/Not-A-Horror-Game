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
    public AnomalyCounter counterSystem;

    private bool isDead = false;


    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        health -= amount;

        if (health <= 0)
        {
            isDead = true;
            StartCoroutine(DieWithDelay());
        }
    }


    private IEnumerator DieWithDelay()
    {
        yield return new WaitForSeconds(1f);

        // =====================================
        // สุ่มโอกาส Drop Ammo
        // =====================================

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


        // =====================================
        // Counter
        // =====================================

        if (counterSystem != null)
        {
            counterSystem.AnomalyRemoved();
        }


        Destroy(gameObject);
    }


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