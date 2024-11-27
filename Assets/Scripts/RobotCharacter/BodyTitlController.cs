using UnityEngine;

public class BodyTiltController : MonoBehaviour
{
    public Transform body;
    public float tiltAmount = 10f;

    private void Update()
    {
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 groundNormal = hit.normal;
            Vector3 forward = Vector3.Cross(groundNormal, transform.right);
            Quaternion tiltRotation = Quaternion.LookRotation(forward, groundNormal);
            body.rotation = Quaternion.Slerp(body.rotation, tiltRotation, Time.deltaTime * tiltAmount);
        }
    }
}
