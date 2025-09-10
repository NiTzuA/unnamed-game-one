using UnityEngine;

public class MovementController : MonoBehaviour
{

    public CharacterController controller;
    public Transform groundCheck;
    public Transform playerCam;

    public float speed = 6f;
    public float gravity = -16.0f;
    public float groundDistance = 0.2f;
    public float jumpHeight = 2f;
    public float extraJumpHeight = 1;
    public float extraJumpsCount = 1;
    public float crouchHeight = 1f;
    public float currentJumps = 0;

    public LayerMask groundMask;

    Vector3 velocity;

    bool isCrouching;
    bool isGrounded;
    float standHeight;

    private void Start()
    {
        
    }
    void Update()
    {
        standHeight = playerCam.transform.position.y;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            currentJumps = extraJumpsCount;
        }
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        /*
        if (Input.GetKeyDown(KeyCode.LeftControl)) isCrouching = true;
        if (Input.GetKeyUp(KeyCode.LeftControl)) isCrouching = false;

        float targetPosition = isCrouching ? standHeight - crouchHeight : standHeight;
        Vector3 newPos = playerCam.transform.position;
        newPos.y = targetPosition;
        playerCam.position = newPos;

        I'M TRYING TO MAKE A CROUCH SYSTEM THAT DOESNT FUCK WITH THE JUMP CAM I'LL DO THIS TOMORROW
        */ 

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (Input.GetButtonDown("Jump") && currentJumps > 0)
        {
            currentJumps--;
            velocity.y = Mathf.Sqrt(extraJumpHeight * -2f * gravity);
            // SET SPEED TO A MULTIPLIER THEN LERP IT BACK TO NORMALCY WHEN GROUNDED
        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); 

    }
}
