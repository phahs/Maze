using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 2f;
    public Rigidbody rb;
    public CharacterController cc;
    public float MinimumX = -90F;
    public float MaximumX = 90F;

    private Camera p_Camera;
    private Quaternion p_TargetVerticalRotation;    // look left and right
    private Quaternion p_TargetHorizontalRotation;  // look up and down
    private Vector3 p_TargetMovement;

    // Use this for initialization
    void Start()
    {
        p_Camera = Camera.main;
        p_TargetVerticalRotation = transform.localRotation;
        p_TargetHorizontalRotation = p_Camera.transform.localRotation;
        p_TargetMovement = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float rotAroundY = Input.GetAxis("Mouse X");
        float rotAroundX = Input.GetAxis("Mouse Y");
        float moveZ = Input.GetAxis("Horizontal");
        float moveX = Input.GetAxis("Vertical");

        p_TargetVerticalRotation *= Quaternion.Euler(0f, rotAroundY, 0f);
        p_TargetHorizontalRotation *= Quaternion.Euler(-rotAroundX, 0f, 0f);

        p_TargetHorizontalRotation = ClampRotationAroundXAxis(p_TargetHorizontalRotation);

        transform.localRotation = p_TargetVerticalRotation;
        p_Camera.transform.localRotation = p_TargetHorizontalRotation;

        p_TargetMovement += (moveX * transform.forward + moveZ * transform.right) * speed * Time.deltaTime;
        p_TargetMovement.y = 0;

        cc.Move(p_TargetMovement);

        p_TargetMovement = Vector3.zero;
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}