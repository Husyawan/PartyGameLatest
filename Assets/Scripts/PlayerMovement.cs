using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Realtime;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement and Speed Settings")]
    public float walkSpeed = 8f;
    public float sprintSpeed = 14f;
    public float maxVelocityChange = 10f;

    [Header("Air & Jumping Controls")]
    [Range(0, 1f)] public float airControl = 0.5f;
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    [Space] public float groundCheckDistance = 0.75f;

    [Header("Animation")]
    public Animator animator; // Reference to Animator component

    #region Private Variables
    public Vector2 input;
    private Rigidbody rb;
    private InputAction _jumpAction;  // Jump action from Input System
    private InputSystem_Actions inputSystem;
    private bool sprinting;
    private bool jumping;
    private bool grounded;
    private Vector3 lastTargetVelocity;
    #endregion
    public LayerMask groundLayer; // Assign ground layer in the inspector
    private CapsuleCollider playerCollider;
    void Awake()
    {
        inputSystem = new InputSystem_Actions();  // Initialize input system actions
    }

    public override void OnEnable()
    {
        _jumpAction = inputSystem.Player.Jump;  // Bind Jump action
        _jumpAction.performed += OnJumpPerformed;  // Subscribe to jump event
        _jumpAction.Enable();
    }

    public override void OnDisable()
    {
        _jumpAction.Disable();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumping = true;  // Set the jumping flag to true when the jump button is pressed
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        // Ensure the animator is attached and assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (InputManager.LockInput)
        {
            input = Vector2.zero;
            sprinting = false;
            jumping = false;
            animator.SetBool("isMoving", false);  // Set to idle when input is locked
            return;
        }

        // Gather input for movement
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        // Detect sprinting (controller or keyboard left shift)
        sprinting = Input.GetKey(KeyCode.LeftShift);

        // Update animator based on input magnitude
        bool isMoving = input.magnitude > 0;
        animator.SetBool("isMoving", isMoving);
    }
    private void CheckGrounded()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 6.5f, groundLayer))
        {
            // Check the angle of the surface normal
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            // Consider the player grounded if the angle is less than a certain threshold (e.g., 45 degrees)
            if (angle < 45f)
            {
                //Debug.Log("Working");
                grounded = true;
            }
            else
            {
                //Debug.Log("Niot Working");
                grounded = false;
            }
        }
        else
        {
            //Debug.Log("Niot 222 Working");
            grounded = false;
        }

        // Debug visualization (optional)
        Debug.DrawRay(rayOrigin, Vector3.down * 6.5f, grounded ? Color.yellow : Color.red);
    }

    private void OnCollisionStay(Collision other)
    {
       
      //  grounded = true;
       
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        CheckGrounded();
        if (grounded)
        {
            if (jumping)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumping = false;  // Reset the jump flag after applying force
            }
            else
            {
                ApplyMovement(sprinting ? sprintSpeed : walkSpeed, false);
            }
        }
        else
        {
            if (input.magnitude > 0.5f)
            {
                ApplyMovement(sprinting ? sprintSpeed : walkSpeed, true);
            }
        }

        grounded = false;  // Reset grounded state for the next frame
    }

    private void ApplyMovement(float _speed, bool _inAir)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity) * _speed;

        if (_inAir)
            targetVelocity += lastTargetVelocity * (1 - airControl);

        Vector3 velocityChange = targetVelocity - rb.velocity;

        if (_inAir)
        {
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange * airControl,
                maxVelocityChange * airControl);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange * airControl,
                maxVelocityChange * airControl);
        }
        else
        {
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        }

        velocityChange.y = 0;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}