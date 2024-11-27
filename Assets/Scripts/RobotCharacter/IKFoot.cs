using UnityEngine;

public class FootIKController : MonoBehaviour
{
    public Animator animator;
    public LayerMask groundLayer;
    public float footOffset = 0.1f;  // Offset to avoid foot clipping

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // Adjust left foot
        AdjustFootPosition(AvatarIKGoal.LeftFoot);

        // Adjust right foot
        AdjustFootPosition(AvatarIKGoal.RightFoot);
    }

    private void AdjustFootPosition(AvatarIKGoal foot)
    {
        Vector3 footPosition = animator.GetIKPosition(foot);
        RaycastHit hit;

        if (Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit, 1f, groundLayer))
        {
            Vector3 newFootPosition = hit.point;
            newFootPosition.y += footOffset;

            // Set IK position and rotation for the foot
            animator.SetIKPosition(foot, newFootPosition);
            animator.SetIKPositionWeight(foot, 1);
            animator.SetIKRotation(foot, Quaternion.LookRotation(transform.forward, hit.normal));
            animator.SetIKRotationWeight(foot, 1);
        }
        else
        {
            // Reset IK if no ground detected
            animator.SetIKPositionWeight(foot, 0);
            animator.SetIKRotationWeight(foot, 0);
        }
    }
}
