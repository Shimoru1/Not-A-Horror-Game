using UnityEngine;

public class ProtectedAnomaly : MonoBehaviour
{
    [Header("Protected Anomaly")]
    [Tooltip("ถ้าเปิดอยู่ การยิงตัวนี้จะถือว่าเป็นความผิดพลาด")]
    [SerializeField] private bool isProtected = true;

    [Header("Test Settings")]
    [Tooltip("เปิดเพื่อให้เห็นข้อความใน Console เมื่อถูกยิง")]
    [SerializeField] private bool debugLog = true;

    public bool IsProtected => isProtected;

    /// <summary>
    /// เรียกใช้เมื่อกระสุนตรวจพบว่าโดน Protected Anomaly
    /// </summary>
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

        // ยังไม่เรียก Game Over ตรงนี้
        // เพราะเราจะเชื่อมกับ GameOverController
        // ตัวจริงของโปรเจกต์ฟ้าในขั้นต่อไป
    }
}