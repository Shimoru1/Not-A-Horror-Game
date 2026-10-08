using UnityEngine;

public class CCTVCleaningInteraction : MonoBehaviour
{
    [Header("References")]
    public CCTVManager cctvManager;
    public CCTVCleaningManager cleaningManager;


    private void Update()
    {
        // ต้องอยู่ใน CCTV ก่อน
        if (cctvManager == null)
            return;

        if (!cctvManager.isWatchingCCTV)
            return;


        // F = เข้า / ออกจาก Cleaning
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleCleaning();
        }
    }


    private void ToggleCleaning()
    {
        if (cleaningManager == null)
            return;


        // ถ้ากำลัง Cleaning อยู่ → ออก
        if (CCTVCleaningManager.IsCleaning)
        {
            cleaningManager.ExitCleaning();
            return;
        }


        // หา CCTV ปัจจุบัน
        GameObject currentCameraObject =
            cctvManager.GetCurrentCameraObject();


        if (currentCameraObject == null)
        {
            Debug.LogWarning(
                "⚠️ ไม่พบ CCTV Camera ปัจจุบัน"
            );

            return;
        }


        // หา CCTVCleaningTarget
        CCTVCleaningTarget target =
            currentCameraObject.GetComponent<CCTVCleaningTarget>();


        if (target == null)
        {
            target =
                currentCameraObject
                .GetComponentInChildren<CCTVCleaningTarget>();
        }


        if (target == null)
        {
            Debug.LogWarning(
                "⚠️ CCTV Camera นี้ไม่มี CCTVCleaningTarget"
            );

            return;
        }


        if (!target.IsDirty())
        {
            Debug.Log(
                "📺 กล้องยังสะอาดอยู่"
            );

            return;
        }


        cleaningManager.StartCleaning(target);
    }
}