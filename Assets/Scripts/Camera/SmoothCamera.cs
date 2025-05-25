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

    [Header("Anti-Shake Settings")]
    [SerializeField] private bool useAntiShake = true;
    [SerializeField] private float shakeThreshold = 0.5f;
    [SerializeField] private float antiShakeStrength = 0.8f;
    [SerializeField] private float velocitySmoothing = 10f;

    // Privates variables
    private Vector3 targetPosition;
    [HideInInspector] public Quaternion targetRotation;
    private Rigidbody targetRigidbody;
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    private Vector3 smoothedVelocity;
    private Vector3 lastCameraPosition;
    private Vector3 cameraVelocity;

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
        smoothedVelocity = Vector3.zero;
        
        // Set camera position on target on start
        transform.position = CalculateTargetPosition();
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);
    }

    private void Update()
    {
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
        
        // Calculate raw target velocity
        Vector3 rawVelocity = (target.position - lastTargetPosition) / deltaTime;
        lastTargetPosition = target.position;
        
        // Apply anti-shake filtering
        if (useAntiShake)
        {
            // Smooth the velocity to reduce jitter
            smoothedVelocity = Vector3.Lerp(smoothedVelocity, rawVelocity, velocitySmoothing * deltaTime);
            
            // Use smoothed velocity if the raw velocity change is too sudden (shake detection)
            Vector3 velocityDifference = rawVelocity - smoothedVelocity;
            if (velocityDifference.magnitude > shakeThreshold)
            {
                targetVelocity = Vector3.Lerp(rawVelocity, smoothedVelocity, antiShakeStrength);
            }
            else
            {
                targetVelocity = rawVelocity;
            }
        }
        else
        {
            targetVelocity = rawVelocity;
        }
        
        // Calculate target position and rotation
        targetPosition = CalculateTargetPosition();
        
        // Apply position interpolation with anti-shake
        float adjustedPositionSpeed = useAntiShake ? positionLerpSpeed * 0.7f : positionLerpSpeed;
        transform.position = Vector3.Lerp(transform.position, targetPosition, adjustedPositionSpeed * deltaTime);
        
        // Calculate look at point with smoothed velocity prediction
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight + (targetVelocity * 0.05f);
        targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
        
        // Apply rotation interpolation with anti-shake
        float adjustedRotationSpeed = useAntiShake ? rotationLerpSpeed * 0.8f : rotationLerpSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, adjustedRotationSpeed * deltaTime);
        
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
            // Use smoothed velocity instead of raw rigidbody velocity for anti-shake
            Vector3 velocityToUse = useAntiShake ? smoothedVelocity : targetRigidbody.linearVelocity;
            float speed = velocityToUse.magnitude;
            
            // Apply a progressive braking effect based on speed
            float speedOffset = Mathf.Min(speed * speedEffect, maxSpeedEffect);
            desiredPosition -= target.forward * speedOffset;
            
            // Small height offset based on speed
            float heightOffset = Mathf.Lerp(0, 0.5f, speed / 50f);
            desiredPosition.y += heightOffset;
            
            // Add a small prediction for smoother movement using smoothed velocity
            desiredPosition += velocityToUse * 0.05f;
        }
        
        return desiredPosition;
    }
}
