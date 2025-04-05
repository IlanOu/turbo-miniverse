using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References & Configuration")]
    [SerializeField] private Transform gunHoldPosition;
    [SerializeField] private GameObject gunPrefab;
    [Tooltip("Vertical offset for the target")][SerializeField] private float aimHeightOffset = 1.5f;
    [SerializeField] private string targetTag = "Mob";

    private Gun _currentGun;

    void Start()
    {
        EquipGun();
    }

    void Update()
    {
        // Find the closest target with the "Mob" tag
        GameObject target = FindClosestTarget();
        if (target != null && _currentGun != null)
        {
            // Adjust the target position by adding a vertical offset
            Vector3 adjustedTargetPos = target.transform.position + Vector3.up * aimHeightOffset;
            
            // Calculate the ballistic trajectory to compensate for gravity
            Vector3 ballisticVelocity = CalculateBallisticVelocity(
                adjustedTargetPos, 
                _currentGun.GetBulletSpawnPoint().position, 
                _currentGun.BulletSpeed, 
                Mathf.Abs(Physics.gravity.y)
            );
            
            // Deduce the direction from the ballistic velocity
            Vector3 aimDirection = ballisticVelocity.normalized;
            
            // Calculate the target rotation based on the calculated direction
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            // Interpolate for a smooth rotation
            gunHoldPosition.rotation = Quaternion.Lerp(gunHoldPosition.rotation, targetRotation, _currentGun.config.autoAimRotationSpeed * Time.deltaTime);
            
            // Automatic firing if the target is within the defined distance
            if (Vector3.Distance(gunHoldPosition.position, target.transform.position) <= _currentGun.config.autoFireDistance)
            {
                _currentGun.TryFire();
            }
        }
    }

    /// <summary>
    /// Calculates the initial velocity required for the projectile to reach the target, taking into account gravity.
    /// </summary>
    /// <param name="target">Adjusted target position</param>
    /// <param name="origin">Origin position (bullet spawn point)</param>
    /// <param name="speed">Initial projectile speed</param>
    /// <param name="gravity">Gravity magnitude</param>
    /// <returns>Initial velocity as a vector</returns>
    Vector3 CalculateBallisticVelocity(Vector3 target, Vector3 origin, float speed, float gravity)
    {
        Vector3 toTarget = target - origin;
        // Separate into horizontal component
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float d = toTargetXZ.magnitude; // Horizontal distance
        float y = toTarget.y;           // Height difference

        float speedSqr = speed * speed;
        // Calculate the part under the square root
        float underSqrt = speedSqr * speedSqr - gravity * (gravity * d * d + 2 * y * speedSqr);
        
        // If the discriminant is negative, no real solution exists:
        // Aim directly at the adjusted target.
        if (underSqrt < 0)
        {
            return (target - origin).normalized * speed;
        }
        
        float sqrt = Mathf.Sqrt(underSqrt);
        // Calculate the firing angle (solution for the lowest angle)
        float angle = Mathf.Atan((speedSqr - sqrt) / (gravity * d));
        
        // Combine horizontal and vertical components to get the initial velocity
        Vector3 velocity = toTargetXZ.normalized * speed * Mathf.Cos(angle) + Vector3.up * speed * Mathf.Sin(angle);
        return velocity;
    }

    /// <summary>
    /// Finds and returns the closest target with the "Mob" tag.
    /// </summary>
    GameObject FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = gunHoldPosition.position;

        foreach (GameObject t in targets)
        {
            Vector3 directionToTarget = t.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = t;
            }
        }
        return closest;
    }

    /// <summary>
    /// Equips the gun corresponding to the provided index.
    /// </summary>
    public void EquipGun()
    {
        if (_currentGun != null)
            Destroy(_currentGun.gameObject);

        GameObject gunObj = Instantiate(gunPrefab, gunHoldPosition);
        gunObj.transform.localPosition = Vector3.zero;
        gunObj.transform.localRotation = Quaternion.identity;

        Gun newGun = gunObj.GetComponent<Gun>();

        // Initialize the new gun
        CarBattery battery = GetComponentInParent<CarBattery>();
        if (newGun != null && battery != null)
        {
            newGun.Initialize(battery);
        }

        _currentGun = newGun;
    }

}
