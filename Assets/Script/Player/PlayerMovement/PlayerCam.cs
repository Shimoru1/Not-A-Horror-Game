using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

	public float fov = 90f;

	public Transform orientaion;

    float xRotation;
    float yRotation;

	private Camera cam;

	private void Start()
    {
		cam = GetComponent<Camera>();
		cam.fieldOfView = fov;

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
