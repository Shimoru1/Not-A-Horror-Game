using UnityEngine;

public class CCTVRotation : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    public float mouseSensitivity = 2.5f;

    [Header("Rotation Limits")]
    public float horizontalLimit = 80f;
    public float verticalUpLimit = 45f;
    public float verticalDownLimit = 45f;

    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;

    private Vector3 startRotation;

    private void Awake()
    {
        startRotation = transform.localEulerAngles;

        horizontalRotation = 0f;
        verticalRotation = 0f;
    }

    private void OnEnable()
    {
        // รีเซ็ตมุมเมื่อเปิด CCTV
        horizontalRotation = 0f;
        verticalRotation = 0f;

        LockMouse();
    }

    private void OnDisable()
    {
        UnlockMouse();
    }

    private void Update()
    {
        // ทำงานเฉพาะตอนกล้องนี้กำลังเปิดอยู่
        if (!gameObject.activeInHierarchy)
            return;

        RotateCamera();
    }

    private void RotateCamera()
    {
        float mouseX =
            Input.GetAxis("MouseX") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("MouseY") * mouseSensitivity;

        // ซ้าย / ขวา
        horizontalRotation += mouseX;

        horizontalRotation =
            Mathf.Clamp(
                horizontalRotation,
                -horizontalLimit,
                horizontalLimit
            );

        // ขึ้น / ลง
        verticalRotation -= mouseY;

        verticalRotation =
            Mathf.Clamp(
                verticalRotation,
                -verticalUpLimit,
                verticalDownLimit
            );

        transform.localRotation =
            Quaternion.Euler(
                startRotation.x + verticalRotation,
                startRotation.y + horizontalRotation,
                startRotation.z
            );
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}