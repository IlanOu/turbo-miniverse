using UnityEngine;

[RequireComponent(typeof(SmoothCamera))]
public class DynamicCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool useFixedUpdate = true;
    
    [Header("Dynamic Camera Settings")]
    [SerializeField] private float tiltAngle = 5f;         // Maximum tilt angle in degrees
    [SerializeField] private float tiltLerpSpeed = 5f;       // Smoothing speed for the tilt

    // Private variables
    private float currentTilt = 0f;
    private float horizontalInput;
    private SmoothCamera smoothCamera;

    private void Start()
    {
        smoothCamera = GetComponent<SmoothCamera>();
    }

    private void Update()
    {
        // Only get horizontal input for camera tilt
        horizontalInput = Input.GetAxis("Horizontal");

        if (!useFixedUpdate)
        {
            UpdateDynamicTilt(Time.deltaTime);
        }
    }
    
    private void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateDynamicTilt(Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Updates the dynamic tilt effect and applies it to the camera rotation.
    /// </summary>
    /// <param name="deltaTime">Elapsed time since last update</param>
    private void UpdateDynamicTilt(float deltaTime)
    {
        // Calculate the target tilt based on horizontal input
        float targetTilt = -horizontalInput * tiltAngle;
        // Smooth the tilt transition
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltLerpSpeed * deltaTime);

        // Retrieve the base rotation from the SmoothCamera script
        Quaternion baseRotation = smoothCamera.targetRotation;
        // Apply the tilt around the forward axis (roll) to the base rotation
        Quaternion dynamicRotation = baseRotation * Quaternion.Euler(0f, 0f, currentTilt);
        // Assign the combined rotation to the camera transform
        transform.rotation = dynamicRotation;
    }
}