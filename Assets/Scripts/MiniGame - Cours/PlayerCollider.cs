
using System;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            Debug.Log("Collision with obstacle");
        }
    }
}