using UnityEngine;

namespace SnoozyRat
{
    public class SR_DemoCharacterRotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [Tooltip("Rotation speed in degrees per second")]
        [SerializeField] private float rotationSpeed = 45f;

        // Update is called once per frame
        void Update()
        {
            int v = 0;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                v -= 1;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                v += 1;
            }

            transform.Rotate(rotationAxis, Time.deltaTime * v * rotationSpeed);
        }
    }
}
