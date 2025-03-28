using System;
using UnityEngine;

public class ObstacleMovements : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] float forwardSpeed = 10f;
    [SerializeField] float maxSpeed = 60f;
    void FixedUpdate()
    {
        rb.AddForce(new Vector3(0, 0, -forwardSpeed * Time.deltaTime * 100));
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, Mathf.Clamp(rb.linearVelocity.z, -maxSpeed, maxSpeed));
    }
}
