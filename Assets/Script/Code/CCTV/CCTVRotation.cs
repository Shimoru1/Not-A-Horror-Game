using UnityEngine;

public class CCTVRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float sensitivity = 2f;
    public float maxYAngle = 80f; // ล็อกมุมก้มเงย ไม่ให้กล้องหมุนตีลังกา

    // ตั้งค่าลิมิตการหันซ้ายขวา (ปรับตามความกว้างของแต่ละห้อง)
    public float minXAngle = -45f;
    public float maxXAngle = 45f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    // เก็บค่า Rotation เริ่มต้นของกล้องแต่ละตัวไว้เป็นจุดศูนย์กลาง
    private Vector3 startRotation;

    void Start()
    {
        startRotation = transform.localEulerAngles;
    }

    void Update()
    {
        // ให้กล้องหมุนได้เฉพาะตอนที่ถูกเปิดใช้งาน (isActiveAndEnabled)
        if (isActiveAndEnabled)
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            // คำนวณแกน Y (ซ้ายขวา)
            rotationY += mouseX;
            rotationY = Mathf.Clamp(rotationY, minXAngle, maxXAngle);

            // คำนวณแกน X (ก้มเงย)
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

            // นำค่าเริ่มต้นมาบวกกับค่าที่เมาส์ขยับ
            transform.localRotation = Quaternion.Euler(startRotation.x + rotationX, startRotation.y + rotationY, 0f);
        }
    }

    void OnEnable()
    {
        // รีเซ็ตมุมกล้องกลับมาตรงกลางทุกครั้งที่สลับกล้อง
        rotationX = 0f;
        rotationY = 0f;
    }
}