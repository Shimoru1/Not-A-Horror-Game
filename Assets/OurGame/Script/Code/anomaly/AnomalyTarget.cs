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


	[Header("Lifetime (Disturb / Protect)")]
	[Tooltip("หายเองหลังกี่วินาที")]
	public float lifetime = 20f;

	[Header("Interact")]
	[Tooltip("ต้องกด E ค้างกี่วินาที")]
	public float holdTime = 3f;
	public float interactDistance = 2.5f;
	[Tooltip("(ไม่บังคับ) Image แบบ Filled แสดงความคืบหน้า")]
	[SerializeField] private Image progressFill;

	[Tooltip("Text 'E' ที่แสดงตอนอยู่ในระยะ (เช่น World Space Canvas เป็นลูกของ Object นี้ ปิดไว้ก่อน)")]
	public GameObject promptUI;

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
	private int maxHp = 1;
	private float lifeTimer;
	private float holdProgress;
	private Collider[] cachedColliders;

	public bool IsActive => isActive;
	public Transform SpawnPoint => spawnPoint;
	private bool UsesSwap =>
	type == AnomalyType.Combat || type == AnomalyType.Interact;

	private void Awake()
	{
		// เก็บ Collider ทั้งหมดของ Object นี้และลูก (รวมที่ปิดอยู่ เช่น anomalyState)
		// ตัด Collider ของ promptUI ออก เผื่อมี
		var all = GetComponentsInChildren<Collider>(true);
		var list = new System.Collections.Generic.List<Collider>();

		foreach (var c in all)
		{
			if (promptUI != null && c.transform.IsChildOf(promptUI.transform))
				continue;

			list.Add(c);
		}

		cachedColliders = list.ToArray();
	}

	// ==================================================
	// INIT / ACTIVATE
	// ==================================================

	// เรียกโดย Manager สำหรับ Combat ที่วางไว้ในฉาก
	public void InitCombat()
	{
		isActive = false;
		SetCombatVisual(false);
		SetPrompt(false);
	}

	public void Activate(AnomalyManager owner, Transform point)
	{
		if (UsesSwap)
			SetCombatVisual(true);
		manager = owner;
		spawnPoint = point;

		isActive = true;
		maxHp = owner != null ? owner.GetMaxHealth(type) : 1;
		health = maxHp;
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
		// ดูกล้องอยู่: ซ่อน Text และกดไม่ได้
		if (CCTVManager.Instance != null &&
			CCTVManager.Instance.isWatchingCCTV)
		{
			SetPrompt(false);
			ResetHold();
			return;
		}

		if (player == null)
		{
			GameObject p = GameObject.FindGameObjectWithTag("Player");
			if (p == null) return;
			player = p.transform;
		}

		float dist = GetDistanceToTarget();
		bool inRange = dist <= interactDistance;

		SetPrompt(inRange);

		if (inRange && Input.GetKey(KeyCode.E))
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
		Debug.Log($"[AnomalyTarget] {name} | type={type} | active={isActive} | hp={health}/{maxHp} | dmg={damage}");

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

		if (UsesSwap)
			SetCombatVisual(false);
		SetPrompt(false);

		if (type == AnomalyType.Combat)
			SetCombatVisual(false); // กลับเป็นปกติ
		SetPrompt(false);

		if (manager != null)
			manager.OnAnomalyResolved(this, reason);
	}

	private void SetPrompt(bool show)
	{
		if (promptUI != null && promptUI.activeSelf != show)
			promptUI.SetActive(show);
	}
	private float GetDistanceToTarget()
	{
		Vector3 p = player.position;
		float best = float.MaxValue;
		bool found = false;

		foreach (var col in cachedColliders)
		{
			// ใช้เฉพาะ Collider ที่ใช้งานอยู่ตอนนี้ (normalState / anomalyState ที่เปิดอยู่)
			if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
				continue;

			Vector3 closest;

			// MeshCollider ที่ไม่ Convex ใช้ ClosestPoint ไม่ได้ ให้ใช้ Bounds แทน
			if (col is MeshCollider mc && !mc.convex)
				closest = col.bounds.ClosestPoint(p);
			else
				closest = col.ClosestPoint(p);

			float d = Vector3.Distance(p, closest);

			if (d < best)
				best = d;

			found = true;
		}

		// ถ้าไม่มี Collider เลย ใช้วิธีเดิม
		if (!found)
			return Vector3.Distance(p, transform.position);

		return best;
	}
}
