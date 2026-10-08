using UnityEngine;

public class OrangeDisturbance : MonoBehaviour
{
    [Header("Orange Disturbance")]

    [Tooltip("เปิดใช้งานระบบลงโทษของตัวส้ม")]
    [SerializeField] private bool isActive = true;

    [Tooltip("จำนวนกระสุนที่จะหัก")]
    [Min(1)]
    [SerializeField] private int ammoPenalty = 1;


    [Header("Debug")]

    [SerializeField] private bool debugLog = true;


    private bool hasBeenKilled = false;


    public void OnKilledByCamera(int cameraIndex)
    {
        if (!isActive)
            return;


        if (hasBeenKilled)
            return;


        hasBeenKilled = true;


        WeaponController weapon =
            FindFirstObjectByType<WeaponController>();


        if (weapon == null)
        {
            Debug.LogWarning(
                "[OrangeDisturbance] " +
                "หา WeaponController ไม่เจอ!"
            );

            return;
        }


        weapon.ReduceAmmo(
            cameraIndex,
            ammoPenalty
        );


        if (debugLog)
        {
            Debug.Log(
                "🟠 Orange Disturbance → " +
                "Camera " +
                (cameraIndex + 1) +
                " ถูกหักกระสุน " +
                ammoPenalty +
                " นัด"
            );
        }
    }
}