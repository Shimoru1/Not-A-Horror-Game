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

	[Header("Reload UI")]
	[SerializeField] private Image reloadImage;

	[Header("UI References")]
    public TextMeshProUGUI ammoText;

    [Header("Ammo Game Over")]
    [Tooltip("ถ้ากระสุนของกล้องปัจจุบันหมดทั้ง Magazine และ Reserve ให้แพ้")]
    public bool ammoEmptyCausesGameOver = true;

    [Tooltip("แสดงสถานะว่ากระสุนของกล้องปัจจุบันหมดหรือไม่")]
    [SerializeField]
    private bool currentCameraAmmoEmpty = false;

    private int currentCameraIndex = 0;
	
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
		HideReloadUI();

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
		TryAutoReload();
	}

	public void DisableWeapon()
	{
		canShoot = false;
		CancelReload();
	}

	// =========================================
	// Shoot
	// =========================================

	void Shoot()
    {
		UpdateCurrentCameraIndex();

		if (magazineAmmo <= 0)
		{
			Debug.Log("กระสุนหมด!");
			TryAutoReload();
			return;
		}

		Camera activeCamera = GetActiveCamera();

		if (activeCamera == null)
			return;

		magazineAmmo--;

		UpdateAmmoUI();
		PlaySound("ShotSound");
		CheckAmmoGameOver();
		TryAutoReload();

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
		TryAutoReload();
	}

	public void AddReserveAmmoToAll(int amount)
	{
		AddReserveAmmo(amount);
	}
	// =========================================
	// Reload
	// =========================================

	Coroutine reloadRoutine;

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

		PlaySound("ReloadSound");
		reloadRoutine = StartCoroutine(Reload());
	}

	void TryAutoReload()
	{
		if (!canShoot || isReloading)
			return;

		// Reload เฉพาะตอน Magazine หมด และยังมีกระสุนสำรอง
		if (magazineAmmo > 0 || reserveAmmo <= 0)
			return;

		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
			return;

		StartReload();
	}

	IEnumerator Reload()
	{
		isReloading = true;

		Debug.Log("กำลัง Reload...");

		// แสดง UI
		if (reloadImage != null)
		{
			reloadImage.gameObject.SetActive(true);
			reloadImage.fillAmount = 1f;
		}

		float timer = 0f;

		while (timer < reloadTime)
		{
			// หยุดนับเวลาตอน Pause
			if (PauseMenu.Instance != null && PauseMenu.Instance.IsPaused)
			{
				yield return null;
				continue;
			}

			// ถูกยกเลิก (เช่น ปิดอาวุธ / Pause ผ่าน SetPaused)
			if (!isReloading)
			{
				HideReloadUI();
				yield break;
			}

			timer += Time.deltaTime;

			if (reloadImage != null)
				reloadImage.fillAmount = 1f - (timer / reloadTime);

			yield return null;
		}

		int neededAmmo = maxAmmo - magazineAmmo;
		int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

		magazineAmmo += ammoToLoad;
		reserveAmmo -= ammoToLoad;

		isReloading = false;
		HideReloadUI();

		UpdateAmmoUI();
		CheckAmmoGameOver();

		Debug.Log("Reload เสร็จ! " + magazineAmmo + "/" + reserveAmmo);
	}

	void HideReloadUI()
	{
		if (reloadImage != null)
		{
			reloadImage.fillAmount = 0f;
			reloadImage.gameObject.SetActive(false);
		}
	}

	void CancelReload()
	{
		if (reloadRoutine != null)
		{
			StopCoroutine(reloadRoutine);
			reloadRoutine = null;
		}

		isReloading = false;
		HideReloadUI();
	}

	// =========================================
	// Camera Index
	// =========================================

	void UpdateCurrentCameraIndex()
	{
		if (CCTVManager.Instance == null)
			return;

		int newCameraIndex = CCTVManager.Instance.GetCurrentCameraIndex();

		if (newCameraIndex != currentCameraIndex)
		{
			currentCameraIndex = newCameraIndex;
			Debug.Log("เปลี่ยนเป็น Camera " + (currentCameraIndex + 1));
		}
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
		if (CCTVManager.Instance == null)
			return null;

		GameObject obj = CCTVManager.Instance.GetCurrentCameraObject();

		if (obj == null)
			return null;

		Camera cam = obj.GetComponent<Camera>();

		return (cam != null && cam.isActiveAndEnabled) ? cam : null;
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
			PlaySound("LowAmmoSound");
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
	private void PlaySound(string sfxId)
	{
		if (AudioManager.Instance == null)
		{
			Debug.LogWarning(
				"[WeaponController] AudioManager.Instance is NULL!"
			);
			return;
		}

		AudioManager.Instance.PlaySFX(sfxId);
	}
}
