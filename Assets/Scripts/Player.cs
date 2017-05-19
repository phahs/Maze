using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 1f;
    public Rigidbody rb;

    private Camera p_Camera;
    private Quaternion p_TargetVerticalRotation;
    private Quaternion p_TargetHorizontalRotation;
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
        p_TargetMovement += (moveX * p_Camera.transform.forward + moveZ * p_Camera.transform.right) * speed * Time.deltaTime;


        transform.localRotation = p_TargetVerticalRotation;
        p_Camera.transform.localRotation = p_TargetHorizontalRotation;
        //transform.localPosition = p_TargetMovement;
        rb.MovePosition(p_TargetMovement);
    }
}
