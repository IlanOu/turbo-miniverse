using System;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] float sidesSpeed = 2f;
    
    float horizontalAxis;

    private void Update()
    {
        horizontalAxis = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        rb.AddForce(new Vector3(horizontalAxis * sidesSpeed * Time.deltaTime * 100, 0, 0), ForceMode.VelocityChange);
    }
}
