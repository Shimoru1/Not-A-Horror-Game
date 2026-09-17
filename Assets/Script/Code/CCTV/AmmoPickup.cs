using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Ammo")]
    public int ammoAmount = 10;

    [Header("Floating Animation")]
    public float floatHeight = 0.25f;
    public float floatSpeed = 2f;

    [Header("Rotation Animation")]
    public float rotateSpeed = 90f;

    private Vector3 startPosition;
    private float randomOffset;


    private void Start()
    {
        startPosition = transform.position;

        // ทำให้แต่ละกล่องลอยไม่พร้อมกัน
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }


    private void Update()
    {
        // =========================
        // ลอยขึ้น / ลง
        // =========================

        float newY =
            Mathf.Sin(
                Time.time * floatSpeed + randomOffset
            ) * floatHeight;

        transform.position =
            startPosition + Vector3.up * newY;


        // =========================
        // หมุน
        // =========================

        transform.Rotate(
            Vector3.up * rotateSpeed * Time.deltaTime,
            Space.World
        );
    }


    // ==========================================
    // ถูกยิง = เก็บกระสุน
    // ==========================================

    public void CollectAmmo(WeaponController weapon)
    {
        if (weapon == null)
            return;

        weapon.AddReserveAmmo(ammoAmount);

        Destroy(gameObject);
    }
}