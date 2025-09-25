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
    public float speedLimit;
    public float slideDuration;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundMask;

    [Header("Camera Controls")]
    public Camera playerCam;
    public Transform playerObject;
    public float playerFov;


    public Transform orientation;

    private float horizontalInput; // formerly x-axis
    private float verticalInput;   // formerly z-axis
    private bool crouchPos = false;
    private bool isGrounded;
    private bool isAirborne = false;
    private bool isCrouching = false;
    private bool isSliding = false;
    private bool isSprinting = false;
    private bool isAirJumped = true;
    private bool canMoveVertical = true;
    private bool canMoveHorizontal = true;
    private bool hasSlid = false;
    private bool justLanded = false;
    private float currentAirJumpCount;
    private float moveSpeed;
    private float elapsedTimeSinceAirJump; // i need sleep
    private float elapsedTimeSinceSlide;
    private float elapsedTimeSinceLanding;
    private float currentFov;
    private float horizontalSpeed;
    private float lastSpeed;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        lastSpeed = 0;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentAirJumpCount = airJumpCount;
        currentFov = playerFov;
        ChangeFov(0, 0, false);
    }

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        if (currentSpeed < lastSpeed - 0.01f)
        {
            moveSpeed = currentSpeed;
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
        if (isGrounded)
        {
            if (isAirborne)
            {
                justLanded = true;
                elapsedTimeSinceLanding = Time.time;
            }

            if (Time.time - elapsedTimeSinceLanding > 0.1f)
            {
                justLanded = false;
            }

            if (isAirJumped)
            {
                elapsedTimeSinceAirJump = Time.time;
                isAirJumped = false;
            }

            isAirborne = false;

            if (!isSprinting && !isSliding)
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
            }
            else
            {
                rb.linearDamping = Mathf.Lerp(rb.linearDamping, groundDrag, Time.deltaTime * 2f);
            }

            if (rb.linearVelocity.magnitude - 1 < crouchSpeed && hasSlid)
            {
                canMoveVertical = true;
                canMoveHorizontal = true;
                isCrouching = true;
            }

            if (rb.linearVelocity.magnitude - 1 < sprintSpeed && !isCrouching)
            {
                hasSlid = false;
            }
        }
        else
        {
            isAirborne = true;
            rb.linearDamping = 0;
        }
        MyInput();

        lastSpeed = currentSpeed;
    }

    private void FixedUpdate()
    {
        if (moveSpeed > speedLimit)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, speedLimit, Time.deltaTime * 2f);
        }
        AdjustCamera();
        MovePlayer();
    }

    private void MyInput()
    {
        if (canMoveVertical)
        {
            verticalInput = Input.GetAxisRaw("Vertical");
        }
        else
        {
            verticalInput = 0;
        }

        if (canMoveHorizontal)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            horizontalInput = 0;
        }


        // Crouching
        if (Input.GetKey(KeyCode.LeftControl) && isGrounded)
        {
            isCrouching = true;
            if (rb.linearVelocity.magnitude > walkSpeed + 1f)
            {
                Slide();
            }
            else
            {
                moveSpeed = crouchSpeed;
            }

            // As soon as the player slides, reduce linearDamping to 0 first, then do a short impulse (or a force.move, test which feels better), then start a timer OR check if the player is on a slope using an angled raycast that is checking if the ground is greater than the set sliding angle. If the player timer is out, or the angle is not steep enough, return the damping to the ground drag again. idk if this works

        }
        else
        {
            canMoveVertical = true;
            canMoveHorizontal = true;
            isCrouching = false;
            isSliding = false;
        }



        // Sprinting
        if (isGrounded && Input.GetKey(KeyCode.LeftShift) && !isSliding)
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
        isAirJumped = true;
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce / 2 + orientation.forward * rb.linearVelocity.magnitude * airBoostForce, ForceMode.Impulse);
    }

    private void Slide()
    {
        if (!isSliding)
        {
            elapsedTimeSinceSlide = Time.time;
            if (!isAirborne && !hasSlid)
            {
                moveSpeed = rb.linearVelocity.magnitude * slideForce;
            }
            canMoveVertical = false;
            canMoveHorizontal = false;
            hasSlid = true;
        }

        isSliding = true;

        if (Time.time - elapsedTimeSinceSlide < slideDuration && !justLanded)
        {
            rb.AddForce(orientation.forward * slideForce * 20, ForceMode.Force);
        }
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
    }

    private void AdjustCamera()
    {
        if (isCrouching)
        {
            if (!crouchPos)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                crouchPos = true;
            }

            float currentPlayerHeight = Mathf.Lerp(playerObject.transform.localScale.y,
            (playerHeight / 4), Time.deltaTime * 10f);
            Vector3 currentScale = playerObject.transform.localScale;
            currentScale.y = currentPlayerHeight;
            playerObject.transform.localScale = currentScale;
        }
        else
        {
            crouchPos = false;
            float currentPlayerHeight = Mathf.Lerp(playerObject.transform.localScale.y,
            (playerHeight / 2), Time.deltaTime * 10f);
            Vector3 currentScale = playerObject.transform.localScale;
            currentScale.y = currentPlayerHeight;
            playerObject.transform.localScale = currentScale;
        }

        if (isAirJumped)
        {
            ChangeFov(0.6f, 0, false);
        }
    }
}
