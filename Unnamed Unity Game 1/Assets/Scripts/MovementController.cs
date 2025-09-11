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
    



    public Transform orientation;

    private float horizontalInput; // formerly x-axis
    private float verticalInput;   // formerly z-axis
    private bool isGrounded;
    private bool isCrouching = false;
    private bool isSliding = false;
    private float currentAirJumpCount;
    private float moveSpeed;
    private float elapsedTimeSinceAirJump; // i need sleep

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentAirJumpCount = airJumpCount;
    }

    void Update()
    {
        MyInput();
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask); // define the magic numbers 0.5f and 0.2f later
        if (isGrounded)
        {
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
            if (isSliding)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, Time.deltaTime * 3);
            }
            else
            {
                moveSpeed = sprintSpeed;
            }
            
        }

        // Walking
        else if (isGrounded && (horizontalInput != 0 || verticalInput != 0))
        {
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
}
