using UnityEngine;
using UnityEngine.UIElements;

public class MovementController : MonoBehaviour
{

    [Header("Movement")]
    
    public float groundDrag;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float jumpForce;
    public float slideForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float airBoostForce;
    public float airJumpCount;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundMask;

    [Header("Camera Controls")]
    public Camera playerCam;
    public float playerFov;
    

    public Transform orientation;

    private float horizontalInput; // formerly x-axis
    private float verticalInput;   // formerly z-axis
    private bool isGrounded;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool isSprinting = false;
    private float currentAirJumpCount;
    private float moveSpeed;
    private float elapsedTimeSinceAirJump; // i need sleep
    private float currentFov;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentAirJumpCount = airJumpCount;
        currentFov = playerFov;
        ChangeFov(0, 0, false);
    }

    void Update()
    {
        MyInput();
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask); // define the magic numbers 0.5f and 0.2f later
        if (isGrounded)
        {
            if (!isSprinting)
            {
                currentFov = 0f;
                ChangeFov(currentFov, 0, false);
            }

            if (Time.time - elapsedTimeSinceAirJump > jumpCooldown)
            {
                RefreshJumps();
            }


            if (isSliding)
            {
                rb.linearDamping = 0f;
            } else
            {
                rb.linearDamping = groundDrag;
            }
        } 
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Crouching
        if (Input.GetKey(KeyCode.LeftControl) && isGrounded)
        {
            if (rb.linearVelocity.magnitude > walkSpeed + 1f)
            {
                Slide();
            } 
            else
            {
                moveSpeed = crouchSpeed;
            }
   
        }

        else
        {
            isSliding = false;
        }

        // Sprinting
        if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
            currentFov = 0.25f;
            ChangeFov(currentFov, 0, false);
            if (isSliding)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, Time.deltaTime * 3);
            }
            else
            {
                moveSpeed = sprintSpeed;
            }
            
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift)) 
        {
            isSprinting = false;
        }

        // Walking
        if (isGrounded && (horizontalInput != 0 || verticalInput != 0) && !isSprinting)
        {
            currentFov = 0f;
            ChangeFov(currentFov, 0, false);
            if (isSliding)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, Time.deltaTime * 3);
            }
            else
            {
                moveSpeed = walkSpeed;
            }
            
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Air Boost
        else if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && currentAirJumpCount > 0)
        {
            moveSpeed = rb.linearVelocity.magnitude * airBoostForce;
            currentAirJumpCount--;
            AirJump();
        }

        SpeedControl();
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }


    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void AirJump()
    {
        elapsedTimeSinceAirJump = Time.time;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce / 2 + orientation.forward * rb.linearVelocity.magnitude * airBoostForce, ForceMode.Impulse);
        ChangeFov(0.5f, 0f, false);
        // fix issue with this. Lerping does NOT work if you don't hold the key (aka doesn't work with GetKeyDown vs GetKey!
    }

    private void Slide()
    {
        isSliding = true;
        // add slide duration
        // add small boost
        // add slope detection
        // increase max speed at slope

        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        //rb.AddForce(transform.up * -jumpForce + orientation.forward * slideForce, ForceMode.Impulse);

    }

    private void RefreshJumps()
    {
        if (isGrounded)
        currentAirJumpCount = airJumpCount;
    }

    private void ChangeFov(float fovDifference, float delay, bool pulse)
    {
        if (!pulse)
        {
            playerCam.fieldOfView = Mathf.Lerp(
                playerCam.fieldOfView, playerFov + (playerFov * fovDifference), 15f * Time.deltaTime);
        }
        else
        {

        }
        // we are gonna have to rework this function lmao
    }
}
