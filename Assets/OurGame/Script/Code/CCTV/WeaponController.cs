using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{

    [Header("Weapon Stats")]
    public int damage = 5;
    public float range = 100f;
    public int maxAmmo = 30;

	[Header("Ammo")]
	public int magazineAmmo = 30;
	public int reserveAmmo = 0;

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

	[Header("Weapon Audio")]
	[SerializeField] private AudioSource weaponAudioSource;

	[SerializeField] private AudioClip shootSound;
	[SerializeField] private AudioClip reloadSound;

    [Header("Ammo Game Over")]
    [Tooltip("ถ้ากระสุนของกล้องปัจจุบันหมดทั้ง Magazine และ Reserve ให้แพ้")]
    public bool ammoEmptyCausesGameOver = true;

    [Tooltip("แสดงสถานะว่ากระสุนของกล้องปัจจุบันหมดหรือไม่")]
    [SerializeField]
    private bool currentCameraAmmoEmpty = false;

    private int currentCameraIndex = 0;

	[Header("Low Ammo Warning")]
	[SerializeField] private AudioClip lowAmmoSound;
	[Tooltip("เตือนเมื่อกระสุนในแม็ก <= ค่านี้ และกระสุนสำรองเป็น 0")]
	public int lowAmmoThreshold = 10;
	[Tooltip("เล่นเสียงเตือนซ้ำทุกกี่วินาที (ควรไม่น้อยกว่าความยาวคลิป)")]
	public float lowAmmoSoundInterval = 1.5f;

	[Header("Low Ammo Effect")]
	[SerializeField] private Image lowAmmoImage;
	[SerializeField] private TextMeshProUGUI lowAmmoText;

	[Tooltip("ความเร็วการ fade เข้า-ออก")]
	public float lowAmmoPulseSpeed = 4f;
	[Range(0f, 1f)] public float lowAmmoMaxAlpha = 0.6f;

	private bool isLowAmmo = false;
	private float lowAmmoSoundTimer = 0f;

	private bool isPaused = false;

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
		// ==============================
		// PAUSE
		// ==============================

		if (PauseMenu.Instance != null &&
			PauseMenu.Instance.IsPaused)
		{
			return;
		}

		// ==============================
		// GAME OVER / WIN
		// ==============================

		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
		{
			StopLowAmmoEffect();
			return;
		}

		UpdateLowAmmoEffect();

		// ==============================
		// WEAPON DISABLED
		// ==============================

		if (!canShoot)
			return;

		UpdateCurrentCameraIndex();

		// ==============================
		// RELOAD
		// ==============================

		if (Input.GetKeyDown(KeyCode.R))
		{
			StartReload();
		}

		// ==============================
		// SHOOT
		// ==============================

		if (!isReloading &&
			Input.GetMouseButtonDown(0))
		{
			Shoot();
		}
	}

	public void EnableWeapon()
    {
        UpdateCurrentCameraIndex();
        canShoot = true;
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

		if (magazineAmmo <= 0)
		{
			Debug.Log("กระสุนหมด! กด R เพื่อ Reload");
			return;
		}

		Camera activeCamera = GetActiveCamera();

		if (activeCamera == null)
			return;

		magazineAmmo--;

		UpdateAmmoUI();
		PlaySound(shootSound);
		CheckAmmoGameOver();

		Debug.Log("Bang! Camera " + (currentCameraIndex + 1) +
				  " = " + magazineAmmo + "/" + reserveAmmo);

		Ray ray = activeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

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

		reserveAmmo += amount;

		Debug.Log("ได้กระสุนสำรอง +" + amount +
				  " → " + magazineAmmo + "/" + reserveAmmo);

		UpdateAmmoUI();
	}

	public void AddReserveAmmoToAll(int amount)
	{
		AddReserveAmmo(amount);
	}
	// =========================================
	// Reload
	// =========================================

	public void StartReload()
    {
		if (isReloading)
			return;

		if (magazineAmmo >= maxAmmo)
		{
			Debug.Log("กระสุนเต็มอยู่แล้ว!");
			return;
		}

		if (reserveAmmo <= 0)
		{
			Debug.Log("ไม่มีกระสุนสำรอง!");
			return;
		}

		PlaySound(reloadSound);
		StartCoroutine(Reload());
	}

	IEnumerator Reload()
	{
		isReloading = true;

		Debug.Log("กำลัง Reload...");

		yield return new WaitForSeconds(reloadTime);

		int neededAmmo = maxAmmo - magazineAmmo;
		int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

		magazineAmmo += ammoToLoad;
		reserveAmmo -= ammoToLoad;

		isReloading = false;

		UpdateAmmoUI();
		CheckAmmoGameOver();

		Debug.Log("Reload เสร็จ! " + magazineAmmo + "/" + reserveAmmo);
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

		if (newCameraIndex != currentCameraIndex)
		{
			currentCameraIndex = newCameraIndex;
			Debug.Log("เปลี่ยนเป็น Camera " + (currentCameraIndex + 1));
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

		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
		{
			return;
		}

		currentCameraAmmoEmpty =
			magazineAmmo <= 0 && reserveAmmo <= 0;

		if (!currentCameraAmmoEmpty)
			return;

		Debug.Log("GAME OVER! กระสุนหมดทั้ง Magazine และ Reserve!");

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
		CheckLowAmmoWarning();

		if (ammoText == null)
			return;

		ammoText.text = magazineAmmo + "/" + reserveAmmo;
	}
	
	public void ReduceAmmo(int cameraIndex, int amount)
	{
		if (amount <= 0)
			return;

		int remaining = amount;

		int fromReserve = Mathf.Min(remaining, reserveAmmo);
		reserveAmmo -= fromReserve;
		remaining -= fromReserve;

		if (remaining > 0)
			magazineAmmo = Mathf.Max(0, magazineAmmo - remaining);

		Debug.Log("ถูกหักกระสุน " + amount +
				  " → " + magazineAmmo + "/" + reserveAmmo);

		UpdateAmmoUI();
		CheckAmmoGameOver();
	}

	void CheckLowAmmoWarning()
	{
		isLowAmmo =
			reserveAmmo <= 0 &&
			magazineAmmo > 0 &&
			magazineAmmo <= lowAmmoThreshold;

		if (!isLowAmmo)
			StopLowAmmoEffect();
	}
	void UpdateLowAmmoEffect()
	{
		// เตือนเฉพาะตอนดูกล้อง
		if (!isLowAmmo || !canShoot)
		{
			StopLowAmmoEffect();
			return;
		}

		// เสียงเตือนวนซ้ำ
		lowAmmoSoundTimer -= Time.deltaTime;

		if (lowAmmoSoundTimer <= 0f)
		{
			PlaySound(lowAmmoSound);
			lowAmmoSoundTimer = lowAmmoSoundInterval;
		}

		// ภาพและข้อความ fade in / fade out พร้อมกัน
		float t = (Mathf.Sin(Time.time * lowAmmoPulseSpeed) + 1f) * 0.5f;

		SetLowAmmoImageAlpha(t * lowAmmoMaxAlpha);
		SetLowAmmoTextAlpha(t);

		Debug.Log("isLow=" + isLowAmmo + " canShoot=" + canShoot +
		  " img=" + (lowAmmoImage != null) + " txt=" + (lowAmmoText != null));
	}

	void StopLowAmmoEffect()
	{
		lowAmmoSoundTimer = 0f;
		SetLowAmmoImageAlpha(0f);
		SetLowAmmoTextAlpha(0f);
	}

	void SetLowAmmoImageAlpha(float alpha)
	{
		if (lowAmmoImage == null)
			return;

		bool show = alpha > 0f;
		if (lowAmmoImage.gameObject.activeSelf != show)
			lowAmmoImage.gameObject.SetActive(show);

		Color c = lowAmmoImage.color;
		c.a = alpha;
		lowAmmoImage.color = c;
	}
	void SetLowAmmoTextAlpha(float alpha)
	{
		if (lowAmmoText == null)
			return;

		bool show = alpha > 0f;
		if (lowAmmoText.gameObject.activeSelf != show)
			lowAmmoText.gameObject.SetActive(show);

		Color c = lowAmmoText.color;
		c.a = alpha;
		lowAmmoText.color = c;
	}

	public void SetPaused(bool paused)
	{
		isPaused = paused;

		if (paused)
		{
			isReloading = false;
		}
	}
	private void PlaySound(AudioClip clip)
	{
		if (clip == null)
			return;

		if (weaponAudioSource == null)
			return;

		weaponAudioSource.PlayOneShot(clip);
	}
}
