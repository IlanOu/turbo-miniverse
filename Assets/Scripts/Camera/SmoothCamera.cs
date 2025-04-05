using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2.5f, -6f);
    [SerializeField] private float lookAtHeight = 1f;

    [Header("Smoothing Settings")]
    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationLerpSpeed = 15f;
    [SerializeField] private bool useFixedUpdate = true;

    [Header("Dynamic Camera Settings")]
    [SerializeField] private bool useDynamicCamera = true;
    [SerializeField] private float speedEffect = 0.01f;
    [SerializeField] private float maxSpeedEffect = 3f;
    [SerializeField] private float tiltAngle = 5f;
    [SerializeField] private float tiltLerpSpeed = 5f;

    // Privates variables
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float currentTilt = 0f;
    private Rigidbody targetRigidbody;
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    private Vector3 lastCameraPosition;
    private Vector3 cameraVelocity;
    private float horizontalInput;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Veuillez assigner une cible à la caméra dans l'inspecteur!");
            return;
        }

        targetRigidbody = target.GetComponent<Rigidbody>();
        lastTargetPosition = target.position;
        lastCameraPosition = transform.position;
        
        // Set camera position on target on start
        transform.position = CalculateTargetPosition();
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);
    }

    private void Update()
    {
        // Only get horizontal input 
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Calculate target velocity for prediction
        if (!useFixedUpdate)
        {
            UpdateCamera(Time.deltaTime);
        }
    }
    
    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateCamera(Time.fixedDeltaTime);
        }
    }
    
    private void UpdateCamera(float deltaTime)
    {
        if (target == null)
            return;
        
        // Calculate target velocity
        targetVelocity = (target.position - lastTargetPosition) / deltaTime;
        lastTargetPosition = target.position;
        
        // Calculate target position and rotation
        targetPosition = CalculateTargetPosition();
        
        // Apply position interpolation
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpSpeed * deltaTime);
        
        // Calculate look at point with target velocity prediction for smoother movement
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight + (targetVelocity * 0.05f);
        targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
        
        // Apply tilt for steering
        if (useDynamicCamera)
        {
            float targetTilt = -horizontalInput * tiltAngle;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltLerpSpeed * deltaTime);
            targetRotation *= Quaternion.Euler(0, 0, currentTilt);
        }
        
        // Apply rotation interpolation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * deltaTime);
        
        // Calculate camera velocity
        cameraVelocity = (transform.position - lastCameraPosition) / deltaTime;
        lastCameraPosition = transform.position;
    }
    
    private Vector3 CalculateTargetPosition()
    {
        // Base position in the local space of the target
        Vector3 desiredPosition = target.TransformPoint(offsetPosition);
        
        // Apply dynamic effects if enabled
        if (useDynamicCamera && targetRigidbody != null)
        {
            // Use velocity of the rigidbody for more accurate calculations
            float speed = targetRigidbody.linearVelocity.magnitude;
            
            // Apply a progressive braking effect based on speed
            float speedOffset = Mathf.Min(speed * speedEffect, maxSpeedEffect);
            desiredPosition -= target.forward * speedOffset;
            
            // Small height offset based on speed
            float heightOffset = Mathf.Lerp(0, 0.5f, speed / 50f);
            desiredPosition.y += heightOffset;
            
            // Add a small prediction for smoother movement
            desiredPosition += targetRigidbody.linearVelocity * 0.05f;
        }
        
        return desiredPosition;
    }
    
    // Function for visualizing trajectories in debug mode
    private void OnDrawGizmos()
    {
        if (target != null && Application.isPlaying)
        {
            // Draw a line from the target to the camera
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.position, transform.position);
            
            // Draw a ray in the direction of the camera
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 5f);
        }
    }
}