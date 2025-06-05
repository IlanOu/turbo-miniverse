using UnityEngine;

[RequireComponent(typeof(SmoothCamera))]
public class DynamicCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool useFixedUpdate = true;
    
    [Header("Dynamic Camera Settings")]
    [SerializeField] private float maxTiltAngle = 5f;
    [SerializeField] private float tiltSmoothTime = 0.2f;
    [SerializeField] private float inputThreshold = 0.1f;
    [SerializeField] private AnimationCurve tiltCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float currentTilt;
    private float targetTilt;
    private float tiltVelocity;
    private SmoothCamera smoothCamera;

    private void Start()
    {
        smoothCamera = GetComponent<SmoothCamera>();
        if (smoothCamera == null)
        {
            Debug.LogError("SmoothCamera component not found!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (!useFixedUpdate)
        {
            UpdateCameraTilt(Time.deltaTime);
        }
    }
    
    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateCameraTilt(Time.fixedDeltaTime);
        }
    }

    private void UpdateCameraTilt(float deltaTime)
    {
        // Get and process input
        float horizontalInput = Input.GetAxis("Horizontal");
        
        // Apply deadzone
        if (Mathf.Abs(horizontalInput) < inputThreshold)
        {
            horizontalInput = 0f;
        }

        // Calculate target tilt with smooth curve
        float normalizedInput = horizontalInput / 1f; // Normalize input to -1 to 1
        float curveValue = tiltCurve.Evaluate(Mathf.Abs(normalizedInput));
        targetTilt = -Mathf.Sign(normalizedInput) * maxTiltAngle * curveValue;

        // Smooth the tilt movement
        currentTilt = Mathf.SmoothDamp(
            currentTilt,
            targetTilt,
            ref tiltVelocity,
            tiltSmoothTime,
            Mathf.Infinity,
            deltaTime
        );

        // Get the base look rotation
        Vector3 lookAtPoint = smoothCamera.transform.position + smoothCamera.transform.forward;
        Quaternion baseRotation = Quaternion.LookRotation(
            lookAtPoint - transform.position,
            Vector3.up
        );

        // Apply tilt
        transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, currentTilt);
    }

    public void ResetTilt()
    {
        currentTilt = 0f;
        targetTilt = 0f;
        tiltVelocity = 0f;
    }
}
