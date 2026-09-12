using UnityEngine;
using UnityEngine.UI; // สำหรับ UI
using System.Collections; // สำหรับ Coroutine

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int damage = 5;
    public float range = 100f;
    public int maxAmmo = 30; // กระสุนทั้งหมด
    private int currentAmmo;

    [Header("Weapon Status")]
    public bool canShoot = false;
    public LayerMask targetLayer;

    [Header("Visual Effects")]
    public LineRenderer bulletTrail;
    public Transform gunBarrel; // ตำแหน่งปลายกระบอกปืน (ต้องสร้าง Empty Object ไปวางไว้ที่ปลายปืน 3D ของเรา)
    public float trailDuration = 0.05f; // ระยะเวลาที่เส้นกระสุนแสดง

    [Header("UI References")]
    public Text ammoText;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        if (bulletTrail != null)
        {
            bulletTrail.enabled = false; // ปิดเส้นกระสุนไว้ก่อน
        }
    }

    void Update()
    {
        if (canShoot && Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void EnableWeapon()
    {
        canShoot = true;
    }

    public void DisableWeapon()
    {
        canShoot = false;
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("กระสุนหมด!");
            return; // ถ้ายิงไม่ได้ก็ออกจากฟังก์ชัน
        }

        Camera activeCamera = GetActiveCamera();

        if (activeCamera == null) return;

        currentAmmo--;
        UpdateAmmoUI();

        Debug.Log("Bang! ยิงจากกล้อง: " + activeCamera.name);

        Ray ray = activeCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        Vector3 hitPosition = ray.origin + (ray.direction * range); // จุดปลายทางของกระสุน (กรณีไม่โดนอะไร)

        if (Physics.Raycast(ray, out hit, range, targetLayer))
        {
            Debug.Log("Hit: " + hit.transform.name);
            hitPosition = hit.point; // ถ้าโดนอะไร ก็ให้จุดปลายทางเป็นจุดที่โดน
        }

        // แสดงเอฟเฟกต์เส้นกระสุน
        if (bulletTrail != null && gunBarrel != null)
        {
            StartCoroutine(ShowBulletTrail(gunBarrel.position, hitPosition));
        }
    }

    IEnumerator ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)
    {
        bulletTrail.enabled = true;
        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, endPoint);

        yield return new WaitForSeconds(trailDuration); // รอแป๊บเดียว

        bulletTrail.enabled = false; // ปิดเส้นกระสุน
    }

    Camera GetActiveCamera()
    {
        Camera[] allCams = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCams)
        {
            if (cam.isActiveAndEnabled && cam.name.Contains("CCTV_Cam"))
            {
                return cam;
            }
        }
        return null;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }
}