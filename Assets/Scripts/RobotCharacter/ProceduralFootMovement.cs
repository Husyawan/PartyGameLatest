using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralFootMovement : MonoBehaviour
{
    public Transform leftFoot;
    public Transform rightFoot;
    public float stepDistance = 0.5f;      // Distance each foot travels forward
    public float stepHeight = 0.2f;        // Height each foot is lifted off the ground
    public float stepSpeed = 2f;           // Speed at which each step is completed

    private Vector3 leftFootStartPos;
    private Vector3 rightFootStartPos;
    private bool leftStep = true;          // Determines which foot is moving

    private Rigidbody rb; // Reference to the character's Rigidbody

    void Start()
    {
        // Save initial foot positions
        leftFootStartPos = leftFoot.position;
        rightFootStartPos = rightFoot.position;

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Only trigger foot movement if the character is moving (based on velocity)
        if (rb.velocity.magnitude > 0.1f) // If the character is moving
        {
            MoveFeet();
        }
    }

    private void MoveFeet()
    {
        if (leftStep)
        {
            // Move the left foot in a step motion
            MoveFoot(leftFoot, leftFootStartPos, rightFoot.position + transform.forward * stepDistance);
        }
        else
        {
            // Move the right foot in a step motion
            MoveFoot(rightFoot, rightFootStartPos, leftFoot.position + transform.forward * stepDistance);
        }
    }

    private void MoveFoot(Transform foot, Vector3 startPos, Vector3 targetPos)
    {
        // Calculate step position using Lerp for smooth movement
        float footStepProgress = Mathf.PingPong(Time.time * stepSpeed, 1);
        Vector3 stepPosition = Vector3.Lerp(startPos, targetPos, footStepProgress);

        // Add a vertical lift to create the stepping motion
        stepPosition.y += Mathf.Sin(footStepProgress * Mathf.PI) * stepHeight;

        // Apply calculated position to the foot
        foot.position = stepPosition;

        // Check if the step is completed
        if (footStepProgress >= 0.95f) // Adjust threshold for completion
        {
            // Toggle step for the next foot
            leftStep = !leftStep;
        }
    }
}
