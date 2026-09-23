using UnityEngine;
using System.Collections;
using TMPro;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class CameraAmmo
    {
        [Tooltip("กระสุนในแม็กของกล้องนี้")]
        public int magazineAmmo = 30;

        [Tooltip("กระสุนสำรองของกล้องนี้")]
        public int reserveAmmo = 0;
    }

    [Header("Weapon Stats")]
    public int damage = 5;
    public float range = 100f;
    public int maxAmmo = 30;

    [Header("5 Camera Ammo")]
    [Tooltip("Element 0 = Camera 1, Element 1 = Camera 2 ... Element 4 = Camera 5")]
    public CameraAmmo[] cameraAmmo = new CameraAmmo[5];

    [Header("Weapon Status")]
    public bool canShoot = false;
    public LayerMask targetLayer;

    [Header("Visual Effects")]
    public LineRenderer bulletTrail;

    [Tooltip("Element 0 = Camera 1, Element 1 = Camera 2 ... Element 4 = Camera 5")]
    public Transform[] gunBarrels;

    public float trailDuration = 0.05f;

    [Header("Reload")]
    public float reloadTime = 1f;
    private bool isReloading = false;

    [Header("UI References")]
    public TextMeshProUGUI ammoText;

    [Header("Ammo Game Over")]
    [Tooltip("ถ้ากระสุนของกล้องปัจจุบันหมดทั้ง Magazine และ Reserve ให้แพ้")]
    public bool ammoEmptyCausesGameOver = true;

    [Tooltip("แสดงสถานะว่ากระสุนของกล้องปัจจุบันหมดหรือไม่")]
    [SerializeField]
    private bool currentCameraAmmoEmpty = false;

    private int currentCameraIndex = 0;

    void Awake()
    {
        // ถ้ายังไม่มีข้อมูล 5 กล้อง ให้สร้างให้ครบ
        if (cameraAmmo == null || cameraAmmo.Length != 5)
        {
            cameraAmmo = new CameraAmmo[5];
        }

        for (int i = 0; i < 5; i++)
        {
            if (cameraAmmo[i] == null)
            {
                cameraAmmo[i] = new CameraAmmo();
            }
        }
    }

    void Start()
    {
        UpdateCurrentCameraIndex();
        UpdateAmmoUI();

        if (bulletTrail != null)
        {
            bulletTrail.enabled = false;
        }
    }

    void Update()
    {
        if (!canShoot)
            return;

        UpdateCurrentCameraIndex();

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }

        if (!isReloading && Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void EnableWeapon()
    {
        canShoot = true;
        UpdateCurrentCameraIndex();
        UpdateAmmoUI();
    }

    public void DisableWeapon()
    {
        canShoot = false;
        isReloading = false;
    }

    // =========================================
    // Shoot
    // =========================================

    void Shoot()
    {
        UpdateCurrentCameraIndex();

        CameraAmmo ammo = GetCurrentAmmoData();

        if (ammo == null)
            return;

        if (ammo.magazineAmmo <= 0)
        {
            Debug.Log("กล้อง " + (currentCameraIndex + 1) + " กระสุนหมด! กด R เพื่อ Reload");
            return;
        }

        Camera activeCamera = GetActiveCamera();

        if (activeCamera == null)
            return;

        ammo.magazineAmmo--;

        UpdateAmmoUI();

        // ตรวจว่ากระสุนหมดทั้ง Magazine + Reserve หรือไม่
        CheckAmmoGameOver();

        Debug.Log(
            "Bang! Camera " + (currentCameraIndex + 1) +
            " = " + ammo.magazineAmmo + "/" + ammo.reserveAmmo
        );

        Ray ray = activeCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        RaycastHit hit;

        Vector3 hitPosition =
            ray.origin + (ray.direction * range);

        if (Physics.Raycast(ray, out hit, range, targetLayer))
        {
            Debug.Log("Hit: " + hit.transform.name);

            hitPosition = hit.point;

            // -----------------------------
            // Anomaly
            // -----------------------------

            AnomalyTarget target =
                hit.transform.GetComponent<AnomalyTarget>();

            if (target == null)
            {
                target =
                    hit.transform.GetComponentInParent<AnomalyTarget>();
            }

            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // -----------------------------
            // Ammo Pickup
            // -----------------------------

            AmmoPickup pickup =
                hit.transform.GetComponent<AmmoPickup>();

            if (pickup == null)
            {
                pickup =
                    hit.transform.GetComponentInParent<AmmoPickup>();
            }

            if (pickup != null)
            {
                Debug.Log(
                    "Ammo Pickup ถูกยิง! +" +
                    pickup.ammoAmount + " นัด"
                );

                pickup.CollectAmmo(this);
            }
        }

        // -----------------------------
        // Bullet Trail ของกล้องปัจจุบัน
        // -----------------------------

        if (bulletTrail != null &&
            gunBarrels != null &&
            currentCameraIndex >= 0 &&
            currentCameraIndex < gunBarrels.Length &&
            gunBarrels[currentCameraIndex] != null)
        {
            StartCoroutine(
                ShowBulletTrail(
                    gunBarrels[currentCameraIndex].position,
                    hitPosition
                )
            );
        }
    }

    // =========================================
    // Add Reserve Ammo
    // Pickup จะเติมให้ "กล้องที่กำลังใช้อยู่"
    // =========================================

    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0)
            return;

        CameraAmmo ammo = GetCurrentAmmoData();

        if (ammo == null)
            return;

        ammo.reserveAmmo += amount;

        Debug.Log(
            "Camera " + (currentCameraIndex + 1) +
            " ได้กระสุนสำรอง +" + amount +
            " → " + ammo.magazineAmmo + "/" + ammo.reserveAmmo
        );

        UpdateAmmoUI();
    }

    // =========================================
    // Reload
    // =========================================

    public void StartReload()
    {
        if (isReloading)
            return;

        CameraAmmo ammo = GetCurrentAmmoData();

        if (ammo == null)
            return;

        if (ammo.magazineAmmo >= maxAmmo)
        {
            Debug.Log(
                "Camera " + (currentCameraIndex + 1) +
                " กระสุนเต็มอยู่แล้ว!"
            );
            return;
        }

        if (ammo.reserveAmmo <= 0)
        {
            Debug.Log(
                "Camera " + (currentCameraIndex + 1) +
                " ไม่มีกระสุนสำรอง!"
            );
            return;
        }

        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        int reloadCamera = currentCameraIndex;

        Debug.Log(
            "Camera " + (reloadCamera + 1) +
            " กำลัง Reload..."
        );

        yield return new WaitForSeconds(reloadTime);

        // ถ้าเปลี่ยนกล้องระหว่าง Reload
        // ให้เติมกล้องเดิมที่เริ่ม Reload
        if (reloadCamera >= 0 &&
            reloadCamera < cameraAmmo.Length &&
            cameraAmmo[reloadCamera] != null)
        {
            CameraAmmo ammo = cameraAmmo[reloadCamera];

            int neededAmmo = maxAmmo - ammo.magazineAmmo;
            int ammoToLoad = Mathf.Min(neededAmmo, ammo.reserveAmmo);

            ammo.magazineAmmo += ammoToLoad;
            ammo.reserveAmmo -= ammoToLoad;
        }

        isReloading = false;

        UpdateCurrentCameraIndex();
        UpdateAmmoUI();

        // ตรวจสถานะกระสุนหลัง Reload
        CheckAmmoGameOver();

        CameraAmmo current = GetCurrentAmmoData();

        if (current != null)
        {
            Debug.Log(
                "Reload เสร็จ! Camera " +
                (currentCameraIndex + 1) +
                " = " +
                current.magazineAmmo +
                "/" +
                current.reserveAmmo
            );
        }
    }

    // =========================================
    // Camera Index
    // =========================================

    void UpdateCurrentCameraIndex()
    {
        Camera activeCamera = GetActiveCamera();

        if (activeCamera == null)
            return;

        int newCameraIndex = GetCameraIndex(activeCamera);

        // อัปเดตทันทีเมื่อเปลี่ยนกล้อง
        if (newCameraIndex != currentCameraIndex)
        {
            currentCameraIndex = newCameraIndex;
            UpdateAmmoUI();

            Debug.Log(
                "เปลี่ยนเป็น Camera " +
                (currentCameraIndex + 1) +
                " → Ammo = " +
                cameraAmmo[currentCameraIndex].magazineAmmo +
                "/" +
                cameraAmmo[currentCameraIndex].reserveAmmo
            );
        }
    }

    int GetCameraIndex(Camera camera)
    {
        if (camera == null)
            return 0;

        string cameraName = camera.name;

        if (cameraName.Contains("1"))
            return 0;

        if (cameraName.Contains("2"))
            return 1;

        if (cameraName.Contains("3"))
            return 2;

        if (cameraName.Contains("4"))
            return 3;

        if (cameraName.Contains("5"))
            return 4;

        return 0;
    }

    CameraAmmo GetCurrentAmmoData()
    {
        if (cameraAmmo == null)
            return null;

        if (currentCameraIndex < 0 ||
            currentCameraIndex >= cameraAmmo.Length)
            return null;

        return cameraAmmo[currentCameraIndex];
    }

    // =========================================
    // Bullet Trail
    // =========================================

    IEnumerator ShowBulletTrail(
        Vector3 startPoint,
        Vector3 endPoint)
    {
        if (bulletTrail == null)
            yield break;

        bulletTrail.enabled = true;

        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, endPoint);

        yield return new WaitForSeconds(trailDuration);

        bulletTrail.enabled = false;
    }

    // =========================================
    // Active CCTV Camera
    // =========================================

    Camera GetActiveCamera()
    {
        Camera[] allCams =
            FindObjectsOfType<Camera>();

        foreach (Camera cam in allCams)
        {
            if (cam.isActiveAndEnabled &&
                cam.name.Contains("CCTV_Cam"))
            {
                return cam;
            }
        }

        return null;
    }

    // =========================================
    // Ammo Game Over Check
    // =========================================

    void CheckAmmoGameOver()
    {
        if (!ammoEmptyCausesGameOver)
            return;

        // ถ้าเกมจบไปแล้ว ไม่ต้องตรวจซ้ำ
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsPlaying())
        {
            return;
        }

        CameraAmmo ammo = GetCurrentAmmoData();

        if (ammo == null)
            return;

        // กระสุนหมดจริงเมื่อ Magazine และ Reserve เป็น 0 ทั้งคู่
        currentCameraAmmoEmpty =
            ammo.magazineAmmo <= 0 &&
            ammo.reserveAmmo <= 0;

        if (!currentCameraAmmoEmpty)
            return;

        Debug.Log(
            "GAME OVER! Camera " +
            (currentCameraIndex + 1) +
            " กระสุนหมดทั้ง Magazine และ Reserve!"
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(
                GameManager.GameOverReason.OutOfAmmo
            );
        }
    }


    // =========================================
    // Ammo UI
    // =========================================

    void UpdateAmmoUI()
    {
        if (ammoText == null)
            return;

        CameraAmmo ammo = GetCurrentAmmoData();

        if (ammo == null)
            return;

        ammoText.text =
            ammo.magazineAmmo + "/" + ammo.reserveAmmo;
    }
}
