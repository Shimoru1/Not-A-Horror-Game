using UnityEngine;
using UnityEngine.UI;

public enum AnomalyType
{
	Combat,    // ยิงจนกลับเป็นปกติ (สลับสภาพ Object ในฉาก)
	Interact,  // เดินเข้าไปกด E ค้าง
	Disturb,   // ยิงตาย = โดนหักกระสุน/กล้องเปื้อน, หายเองตามเวลา
	Protect    // ยิงตาย = แพ้ทันที, หายเองตามเวลา
}

public enum AnomalyResolveReason
{
	Killed,
	Cured,
	Expired
}

public class AnomalyTarget : MonoBehaviour
{
	[Header("Type")]
	public AnomalyType type = AnomalyType.Combat;

	[Header("Health (Combat / Disturb / Protect)")]
	public int maxHealth = 20;

	[Header("Lifetime (Disturb / Protect)")]
	[Tooltip("หายเองหลังกี่วินาที")]
	public float lifetime = 20f;

	[Header("Interact")]
	[Tooltip("ต้องกด E ค้างกี่วินาที")]
	public float holdTime = 3f;
	public float interactDistance = 2.5f;
	[Tooltip("(ไม่บังคับ) Image แบบ Filled แสดงความคืบหน้า")]
	[SerializeField] private Image progressFill;

	[Header("Combat Swap")]
	[Tooltip("สภาพปกติ (เช่น รูปภาพปกติ)")]
	public GameObject normalState;
	[Tooltip("สภาพ Anomaly (เช่น รูปภาพที่เปลี่ยนไป) - ต้องมี Collider")]
	public GameObject anomalyState;

	private AnomalyManager manager;
	private Transform spawnPoint;
	private Transform player;

	private bool isActive = false;
	private int health;
	private float lifeTimer;
	private float holdProgress;

	public bool IsActive => isActive;
	public Transform SpawnPoint => spawnPoint;

	// ==================================================
	// INIT / ACTIVATE
	// ==================================================

	// เรียกโดย Manager สำหรับ Combat ที่วางไว้ในฉาก
	public void InitCombat()
	{
		isActive = false;
		SetCombatVisual(false);
	}

	public void Activate(AnomalyManager owner, Transform point)
	{
		manager = owner;
		spawnPoint = point;

		isActive = true;
		health = maxHealth;
		lifeTimer = lifetime;
		holdProgress = 0f;

		if (type == AnomalyType.Combat)
			SetCombatVisual(true);

		UpdateProgressUI();
	}

	private void SetCombatVisual(bool anomaly)
	{
		if (normalState != null) normalState.SetActive(!anomaly);
		if (anomalyState != null) anomalyState.SetActive(anomaly);
	}

	// ==================================================
	// UPDATE
	// ==================================================

	private void Update()
	{
		if (!isActive)
			return;

		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
			return;

		if (type == AnomalyType.Disturb || type == AnomalyType.Protect)
		{
			lifeTimer -= Time.deltaTime;

			if (lifeTimer <= 0f)
			{
				Resolve(AnomalyResolveReason.Expired);
				return;
			}
		}

		if (type == AnomalyType.Interact)
			HandleInteract();
	}

	private void HandleInteract()
	{
		// กดแก้ได้เฉพาะตอนไม่ได้ดูกล้อง
		if (CCTVManager.Instance != null &&
			CCTVManager.Instance.isWatchingCCTV)
		{
			ResetHold();
			return;
		}

		if (player == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p == null) return;
			player = p.transform;
		}

		float dist = Vector3.Distance(player.position, transform.position);

		if (dist <= interactDistance && Input.GetKey(KeyCode.E))
		{
			holdProgress += Time.deltaTime;
			UpdateProgressUI();

			if (holdProgress >= holdTime)
				Resolve(AnomalyResolveReason.Cured);
		}
		else
		{
			ResetHold();
		}
	}

	private void ResetHold()
	{
		if (holdProgress == 0f) return;
		holdProgress = 0f;
		UpdateProgressUI();
	}

	private void UpdateProgressUI()
	{
		if (progressFill == null) return;

		progressFill.gameObject.SetActive(holdProgress > 0f);
		progressFill.fillAmount = holdTime > 0f ? holdProgress / holdTime : 0f;
	}

	// ==================================================
	// DAMAGE (เรียกจาก WeaponController)
	// ==================================================

	public void TakeDamage(int damage)
	{
		if (!isActive)
			return;

		// Interact ยิงไม่เข้า
		if (type == AnomalyType.Interact)
			return;

		health -= damage;

		if (health > 0)
			return;

		switch (type)
		{
			case AnomalyType.Combat:
				Resolve(AnomalyResolveReason.Killed);
				break;

			case AnomalyType.Disturb:
				if (manager != null)
					manager.ApplyDisturbPenalty();
				Resolve(AnomalyResolveReason.Killed);
				break;

			case AnomalyType.Protect:
				Resolve(AnomalyResolveReason.Killed);

				if (GameManager.Instance != null)
				{
					GameManager.Instance.GameOver(
						GameManager.GameOverReason.ShotProtectAnomaly
					);
				}
				break;
		}
	}

	// ==================================================
	// RESOLVE
	// ==================================================

	private void Resolve(AnomalyResolveReason reason)
	{
		if (!isActive)
			return;

		isActive = false;

		if (type == AnomalyType.Combat)
			SetCombatVisual(false); // กลับเป็นปกติ

		if (manager != null)
			manager.OnAnomalyResolved(this, reason);
	}
}
