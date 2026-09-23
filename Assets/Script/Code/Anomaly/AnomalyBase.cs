using UnityEngine;

/// <summary>
/// Base class สำหรับ Anomaly ทุกประเภทในอนาคต
/// ตอนนี้สร้างไว้เป็นโครงสร้างกลางก่อน และไม่บังคับให้ Anomaly เดิมต้องเปลี่ยนทันที
/// </summary>
public abstract class AnomalyBase : MonoBehaviour
{
    [Header("Anomaly Base")]
    [Tooltip("ชื่อของ Anomaly สำหรับ Debug / Inspector")]
    public string anomalyName = "New Anomaly";

    [Tooltip("เปิดเพื่อแสดง Log ตอน Anomaly ถูกสร้าง/เริ่มทำงาน")]
    public bool showDebugLog = false;

    protected bool isActive = true;
    protected bool hasBeenRemoved = false;

    protected virtual void Awake()
    {
        isActive = true;
        hasBeenRemoved = false;
    }

    protected virtual void Start()
    {
        if (showDebugLog)
        {
            Debug.Log("Anomaly Started: " + anomalyName, gameObject);
        }
    }

    /// <summary>
    /// เรียกเมื่อ Anomaly ถูกจัดการ/กำจัด
    /// ลูกแต่ละประเภทสามารถ Override เพื่อเพิ่มพฤติกรรมเฉพาะได้
    /// </summary>
    public virtual void OnAnomalyRemoved()
    {
        if (hasBeenRemoved)
            return;

        hasBeenRemoved = true;
        isActive = false;

        if (showDebugLog)
        {
            Debug.Log("Anomaly Removed: " + anomalyName, gameObject);
        }
    }

    /// <summary>
    /// ใช้เช็กว่า Anomaly ตัวนี้ยังทำงานอยู่หรือไม่
    /// </summary>
    public bool IsAnomalyActive()
    {
        return isActive && !hasBeenRemoved;
    }

    /// <summary>
    /// จุดสำหรับให้ Anomaly ลูก Override เพื่อเริ่มพฤติกรรมเฉพาะตัว
    /// </summary>
    public virtual void ActivateAnomaly()
    {
        isActive = true;
        hasBeenRemoved = false;
    }

    /// <summary>
    /// จุดสำหรับให้ Anomaly ลูก Override เมื่อเกมจบ
    /// </summary>
    public virtual void StopAnomaly()
    {
        isActive = false;
    }
}
