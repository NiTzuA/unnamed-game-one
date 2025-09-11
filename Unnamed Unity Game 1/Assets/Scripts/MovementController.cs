using UnityEngine;

public class MovementController : MonoBehaviour
{

    public CharacterController controller;
    public Transform groundCheck;
    public Transform playerCam;
    public Camera playerFov;

    public float walkFov = 50f;
    public float crouchSpeed = 3f;
    public float baseSpeed = 6f;
    public float gravity = -16.0f;
    public float groundDistance = 0.2f;
    public float jumpHeight = 2f;
    public float extraJumpHeight = 1;
    public float extraJumpsCount = 1;
    public float crouchHeight = 1f;
    public float boostMultiplier = 3f;
    public float sprintSpeed = 12f;
    public float sprintZoom = 0.15f;

    private float walkSpeed = 0;
    private float currentJumps = 0;
    private float currentSpeed = 0;
    private float z;
    private float x;

    public LayerMask groundMask;

    Vector3 velocity;

    bool isCrouching;
    bool isSprinting;
    bool isGrounded;
    float standHeight;

    private void Start()
    {
        walkSpeed = baseSpeed;
        playerFov.fieldOfView = walkFov;
        standHeight = playerCam.localPosition.y;
    }

    void Update()
    {

        //sprinting while in crouch should not be allowed

        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        if (x == 0f && z == 0)
        {
            currentSpeed = 0;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * 3f);
            velocity.y = -2f;
            currentJumps = extraJumpsCount;
        } else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * 1f); 
        }

        float targetPosition = isCrouching ? standHeight - crouchHeight : standHeight;
        Vector3 newPos = playerCam.localPosition;
        newPos.y = targetPosition;
        playerCam.localPosition = newPos;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0)
        {
            isSprinting = true;
            baseSpeed = sprintSpeed;
            playerFov.fieldOfView = Mathf.Lerp(playerFov.fieldOfView, walkFov + (walkFov * sprintZoom), 15f * Time.deltaTime);
        }
        else if (isGrounded && !isCrouching)
        {
            isSprinting = false;
            baseSpeed = walkSpeed; // idk if this line is needed
            playerFov.fieldOfView = Mathf.Lerp(playerFov.fieldOfView, walkFov, 15f * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.LeftControl) && isGrounded) 
        {
            if (!isCrouching && isSprinting)
            {   
                currentSpeed = baseSpeed * boostMultiplier ;
            }

            baseSpeed = crouchSpeed;
            isCrouching = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            baseSpeed = walkSpeed; // idk if this line is needed
            isCrouching = false;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {

            velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentSpeed *= 1.25f;
        }
        else if (Input.GetButtonDown("Jump") && currentJumps > 0)
        {
            currentJumps--;
            velocity.y = Mathf.Sqrt(extraJumpHeight * -2f * gravity);
            currentSpeed = baseSpeed * (boostMultiplier * 0.75f);

        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); 

    }
}
