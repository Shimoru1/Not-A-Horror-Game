using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float groundDrag = 5f;

    [Header("GroundCheck")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;

    public Transform orientation;

    private bool grounded;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
	public bool canMove = true;

	private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
    }


    private void Update()
    {
        grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            whatIsGround
        );

        PlayerInput();
        SpeedControl();

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }


    private void FixedUpdate()
    {
        MovePlayer();
    }


    private void PlayerInput()
    {
		if (!canMove)
		{
			horizontalInput = 0f;
			verticalInput = 0f;
			return;
		}

		horizontalInput = Input.GetAxisRaw("MoveX");
		verticalInput = Input.GetAxisRaw("MoveY");
	}


    private void MovePlayer()
    {
		if (orientation == null || !canMove)
			return;

		moveDirection =
			orientation.forward * verticalInput +
			orientation.right * horizontalInput;

		rb.AddForce(
			moveDirection.normalized * moveSpeed * 10f,
			ForceMode.Force
		);
	}
	public void StopMovement()
	{
		if (rb == null)
			rb = GetComponent<Rigidbody>();

		rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
	}


	private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel =
                flatVel.normalized * moveSpeed;

            rb.linearVelocity = new Vector3(
                limitedVel.x,
                rb.linearVelocity.y,
                limitedVel.z
            );
        }
    }
}