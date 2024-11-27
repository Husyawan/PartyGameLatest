using UnityEngine;

public class ProceduralArmSwing : MonoBehaviour
{
    public Transform leftArm;
    public Transform rightArm;
    public float swingAmount = 30f;
    public float swingSpeed = 2f;

    private float swingTime = 0;

    void Update()
    {
        swingTime += Time.deltaTime * swingSpeed;
        float swingAngle = Mathf.Sin(swingTime) * swingAmount;

        leftArm.localRotation = Quaternion.Euler(swingAngle, leftArm.localRotation.y, leftArm.localRotation.z);
        rightArm.localRotation = Quaternion.Euler(-swingAngle, rightArm.localRotation.y, rightArm.localRotation.z);
    }
}
