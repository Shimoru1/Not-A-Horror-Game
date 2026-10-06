using UnityEngine;
using UnityEngine.UI;

public class CCTVDirtOverlay : MonoBehaviour
{
    [Header("Dirt Overlay")]
    public RawImage dirtImage;

    [Header("Current Camera")]
    public CCTVCleaningTarget currentCamera;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.85f;


    private void Start()
    {
        HideDirt();
    }


    private void Update()
    {
        if (currentCamera != null)
        {
            UpdateDirt();
        }
        else
        {
            HideDirt();
        }
    }


    public void SetCamera(CCTVCleaningTarget target)
    {
        currentCamera = target;

        if (currentCamera != null)
        {
            UpdateDirt();
        }
        else
        {
            HideDirt();
        }
    }


    private void UpdateDirt()
    {
        if (dirtImage == null || currentCamera == null)
            return;

        float dirtPercent = currentCamera.GetDirtPercent();

        // ยังสกปรกไม่ถึง 50% → ซ่อนภาพคราบ
        if (dirtPercent < 0.5f)
        {
            dirtImage.gameObject.SetActive(false);
            return;
        }

        // ถึง 50% แล้ว → แสดงภาพคราบ
        dirtImage.gameObject.SetActive(true);

        // เริ่มแสดงจาก 0% เมื่อ dirt ถึง 50%
        float visiblePercent = (dirtPercent - 0.5f) / 0.5f;

        float alpha = visiblePercent * maxAlpha;

        Color color = dirtImage.color;
        color.a = alpha;
        dirtImage.color = color;
    }


    private void HideDirt()
    {
        if (dirtImage == null)
            return;

        Color color = dirtImage.color;
        color.a = 0f;
        dirtImage.color = color;

        dirtImage.gameObject.SetActive(false);
    }
}