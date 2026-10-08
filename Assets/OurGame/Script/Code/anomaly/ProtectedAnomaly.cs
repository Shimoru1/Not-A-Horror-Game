using UnityEngine;

public class ProtectedAnomaly : MonoBehaviour
{
    // ==================================================
    // PROTECTED ANOMALY
    // ==================================================

    [Header("Protected Anomaly")]

    [Tooltip("ถ้าเปิดอยู่ การยิงตัวนี้จะถือว่าเป็นความผิดพลาด")]
    [SerializeField] private bool isProtected = true;


    // ==================================================
    // LIFETIME
    // ==================================================

    [Header("Lifetime")]

    [Tooltip("เปิดเพื่อให้ Protected Anomaly หายไปเองตามเวลาที่กำหนด")]
    [SerializeField] private bool autoDestroy = true;

    [Tooltip("จำนวนวินาทีที่ Protected Anomaly จะอยู่ในด่าน")]
    [SerializeField] private float lifetime = 10f;
	private float lifeTimer = 0f;


	// ==================================================
	// TEST SETTINGS
	// ==================================================

	[Header("Test Settings")]

    [Tooltip("เปิดเพื่อให้เห็นข้อความใน Console เมื่อถูกยิง")]
    [SerializeField] private bool debugLog = true;


    // ==================================================
    // PROPERTY
    // ==================================================

    public bool IsProtected => isProtected;


    // ==================================================
    // START
    // ==================================================

    private void Start()
    {
       
    }


	private void Update()
	{
		if (GameManager.Instance != null &&
			!GameManager.Instance.IsPlaying())
			return;

		if (!autoDestroy)
			return;

		lifeTimer += Time.deltaTime;

		if (lifeTimer >= lifetime)
			RemoveAnomaly();
	}
	public void OnProtectedHit()
    {
        if (!isProtected)
            return;


        if (debugLog)
        {
            Debug.Log(
                $"[ProtectedAnomaly] {gameObject.name} ถูกยิง! " +
                "ต้องส่งต่อไปยังระบบ Game Over"
            );
        }


        // ยังไม่เรียก Game Over
        // รอเชื่อมกับระบบของเพื่อนภายหลัง
    }


    // ==================================================
    // AUTO REMOVE
    // ==================================================

    private void RemoveAnomaly()
    {
        if (debugLog)
        {
            Debug.Log(
                $"[ProtectedAnomaly] {gameObject.name} " +
                $"อยู่ครบ {lifetime:F1} วินาทีแล้ว → หายไป"
            );
        }

        Destroy(gameObject);
    }


    // ==================================================
    // DESTROY
    // ==================================================

    private void OnDestroy()
    {
        CancelInvoke();
    }
}