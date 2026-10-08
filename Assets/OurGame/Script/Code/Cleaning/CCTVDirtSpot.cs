using UnityEngine;
using UnityEngine.EventSystems;

public class CCTVDirtSpot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isCleaned = false;
    private bool isHovered = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        TryClean();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void Update()
    {
        // รองรับกรณีเมาส์อยู่บนคราบอยู่แล้ว แล้วค่อยกด/กดค้างเมาส์
        if (isHovered)
            TryClean();
    }

    private void TryClean()
    {
        if (isCleaned)
            return;

        if (!CCTVCleaningManager.IsCleaning)
            return;

        if (!Input.GetMouseButton(0))
            return;

        CleanSpot();
    }

    private void CleanSpot()
    {
        if (isCleaned)
            return;

        isCleaned = true;

        CCTVCleaningManager manager =
            FindFirstObjectByType<CCTVCleaningManager>();

        if (manager != null)
            manager.CleanOneSpot(this);

        gameObject.SetActive(false);
    }

    public void ResetSpot()
    {
        isCleaned = false;
        isHovered = false;
        gameObject.SetActive(true);
    }
}
