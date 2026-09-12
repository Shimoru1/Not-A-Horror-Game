using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientaion;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("MouseX") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("MouseY") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);//กันคอบิด

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientaion.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
