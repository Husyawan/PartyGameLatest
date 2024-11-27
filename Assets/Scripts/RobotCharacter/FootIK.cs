
using UnityEngine;

public class footIK : MonoBehaviour
{
    // Debug flags
    [Header("Debug")]
    [SerializeField] private bool groundDebug; // Enable ground debug visualization
    [SerializeField] private bool ikDebug; // Enable IK debug visualization

    // References
    [Header("References")]
    [SerializeField] private PlayerMovement movement; // Reference to the player movement script

    // Ground check parameters
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer; // Layer mask for ground objects
    [SerializeField] private Transform groundRaycastOrigin; // Origin of ground raycasts
    [SerializeField] private float groundRaycastDistance = 0.1f; // Distance of ground raycasts
    [SerializeField] private float groundRaycastRadius = 0.1f; // Radius of ground raycasts

    // Body movement parameters
    [Header("Body")]
    [SerializeField] private Transform body; // Reference to the body transform
    [SerializeField] private float bodyHeight; // Height of the body above the ground
    [SerializeField] private float bodyMoveSpeed; // Speed of body movement

    // Foot IK Visuals parameters
    [Header("Foot IK Visuals")]
    [SerializeField] private float footSpeed; // Speed of foot movement
    [SerializeField] private float stepHeight; // Height of footstep

    // Foot IK parameters
    [Header("Foot IK")]
    [SerializeField] private Transform leftFootTarget; // Target transform for left foot
    [SerializeField] private Transform rightFootTarget; // Target transform for right foot
    [SerializeField] private Transform ikRaycastOrigin; // Origin of IK raycasts
    [SerializeField] private Transform leftFootRaycastOrigin; // Origin of raycast for left foot
    [SerializeField] private Transform rightFootRaycastOrigin; // Origin of raycast for right foot
    [SerializeField] private float stepDistance; // Distance threshold for footstep
    [SerializeField] private float stepLength; // Length of each step
    [SerializeField] private float maxFootReach; // Maximum reach of the foot
    [SerializeField] private float footYOffset; // Vertical offset of the foot
    [SerializeField] private LayerMask ikLayer; // Layer mask for IK objects


    [SerializeField] private float moveForce = 50f;
    [SerializeField] private float maxSpeed = 5f;

    // Private variables
    private bool isGrounded; // Whether the player is grounded
    private RaycastHit groundHit; // Information about the ground hit

    private Vector3 oldPosRight, oldPosLeft; // Previous positions of the feet
    private Vector3 newPosRight, newPosLeft; // New positions of the feet
    private Vector3 currentPosLeft, currentPosRight; // Current positions of the feet
    private Vector3 footTargetPosRight, footTargetPosLeft; // Target positions for the feet
    private float lerpLeft = 0f, lerpRight = .5f; // Lerp values for smooth foot movement
    private bool rightFootStep = true, leftFootStep = false; // Flags for alternating footstep

    // Update is called once per frame
    void Update()
    {
        // Perform foot IK for left and right feet
        FootIK(leftFootRaycastOrigin, leftFootTarget, ref newPosRight, ref oldPosRight, ref footTargetPosRight, leftFootStep, ref rightFootStep, ref lerpLeft, ref currentPosRight);
        FootIK(rightFootRaycastOrigin, rightFootTarget, ref newPosLeft, ref oldPosLeft, ref footTargetPosLeft, rightFootStep, ref leftFootStep, ref lerpRight, ref currentPosLeft);

        // Perform ground check
        isGrounded = Physics.SphereCast(groundRaycastOrigin.position, groundRaycastRadius, Vector3.down, out groundHit, groundRaycastDistance, groundLayer);

        // Calculate and update the position of the body based on foot positions
        Vector3 bodyTarget = ((footTargetPosRight + footTargetPosLeft) / 2) + (Vector3.up * bodyHeight) - (body.forward * stepLength);

        // Move the body towards the calculated target position
        if (isGrounded)
            body.position = Vector3.Lerp(body.position, bodyTarget, bodyMoveSpeed * Time.deltaTime);
    }
    void FixedUpdate()
    {
        Move();
        Balance();
    }
    private void Move()
    {
        if (movement.input.magnitude > 0.1f)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            Vector3 movementForce = movement.input.normalized * moveForce * Time.fixedDeltaTime;

            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(movementForce, ForceMode.VelocityChange);
            }
        }
    }

    private void Balance()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 uprightForce = Vector3.up * 10f;
        Vector3 torqueForce = Vector3.Cross(transform.up, Vector3.up);

        rb.AddForce(uprightForce, ForceMode.Acceleration);
        rb.AddTorque(torqueForce * 0.5f, ForceMode.Acceleration);
    }
    // Start is called before the first frame update
    void Start()
    {
        // Initialize foot positions
        FootIK(leftFootRaycastOrigin, leftFootTarget, ref newPosRight, ref oldPosRight, ref footTargetPosRight, leftFootStep, ref rightFootStep, ref lerpLeft, ref currentPosRight);
        FootIK(rightFootRaycastOrigin, rightFootTarget, ref newPosLeft, ref oldPosLeft, ref footTargetPosLeft, rightFootStep, ref leftFootStep, ref lerpRight, ref currentPosLeft);

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.mass = 1f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Set initial foot positions
        newPosLeft = newPosRight = oldPosLeft = oldPosRight = footTargetPosLeft;
    }

    // Function to perform Foot IK
    void FootIK(Transform rayOrigin, Transform foot, ref Vector3 newPos, ref Vector3 oldPos, ref Vector3 targetPos, bool altFootStep, ref bool footStep, ref float lerp, ref Vector3 currentPos)
    {
        RaycastHit hitForward;
        bool rayForward = Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hitForward, stepLength, ikLayer);
        Vector3 hipPos = rayForward ? hitForward.point : rayOrigin.position + stepLength * ikRaycastOrigin.forward;

        RaycastHit hitDown;
        bool rayDown = Physics.Raycast(hipPos, Vector3.down, out hitDown, maxFootReach, ikLayer);

        targetPos = rayDown ? Vector3.up * footYOffset + hitDown.point : Vector3.up * (footYOffset - maxFootReach) + hipPos;

        float footDistance = Vector3.Distance(oldPos, targetPos);
        float randomizedStepDistance = stepDistance * Random.Range(0.8f, 1.2f);
        if (footDistance > randomizedStepDistance)
        {
            newPos = targetPos;
            lerp = 0;
        }

        if (lerp < 1 && !altFootStep)
        {
            footStep = true;
            currentPos = Vector3.Lerp(foot.position, newPos, lerp);
            lerp += Time.deltaTime * footSpeed;
        }
        else
        {
            footStep = false;
            oldPos = newPos;
            lerp = 0;
        }

        float yStep = movement.input.magnitude >= 1 ? (0.5f * (1 + Mathf.Sin(2 * Mathf.PI * lerp))) * stepHeight : 0;
        foot.position = currentPos + Vector3.up * yStep;

        if (ikDebug)
        {
            Debug.DrawLine(ikRaycastOrigin.position, hipPos, Color.red);
            Debug.DrawLine(rayOrigin.position, hitDown.point, Color.red);
        }
    }
}