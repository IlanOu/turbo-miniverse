using UnityEngine;

public class ObstacleDestroyer : MonoBehaviour
{
    
    [SerializeField] float distanceToDestroy = -10f;
    
    void Update()
    {
        if (transform.position.z < distanceToDestroy)
        {
            Destroy(gameObject);
        }
    }
}
