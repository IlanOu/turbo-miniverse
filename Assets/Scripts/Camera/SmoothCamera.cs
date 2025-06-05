using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Target Settings")] [SerializeField]
    private Transform target;

    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2.5f, -6f);
    [SerializeField] private float lookAtHeight = 1f;

    [Header("Smoothing Settings")] [SerializeField]
    private float positionSmoothTime = 0.2f;

    [SerializeField] private float rotationSmoothTime = 0.2f;
    [SerializeField] private bool useFixedUpdate = true;

    [Header("Direction Settings")] [SerializeField]
    private float backwardOffsetMultiplier = 1.5f;

    [SerializeField] private float directionChangeSpeed = 5f;
    [SerializeField] private float minDistanceToTarget = 4f;

    [Header("Dynamic Camera Settings")] [SerializeField]
    private bool useDynamicCamera = true;

    [SerializeField] private float speedEffect = 0.1f;
    [SerializeField] private float maxSpeedEffect = 3f;
    [SerializeField] private float heightSpeedMultiplier = 0.02f;

    private Vector3 smoothVelocity;
    private Vector3 lastTargetPosition;
    private float currentDirectionBlend;
    private Vector3 positionVelocity;
    private Vector3 currentRotationVelocity;
    private Vector3 targetRotationEuler;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to camera!");
            enabled = false;
            return;
        }

        lastTargetPosition = target.position;
        ResetCameraPosition();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            UpdateCamera(Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (!useFixedUpdate)
            UpdateCamera(Time.deltaTime);
    }

    private void UpdateCamera(float deltaTime)
    {
        if (target == null) return;

        // Calcul de la vélocité de la cible
        Vector3 targetVelocity = (target.position - lastTargetPosition) / deltaTime;
        lastTargetPosition = target.position;
        smoothVelocity = Vector3.Lerp(smoothVelocity, targetVelocity, deltaTime * 5f);

        // Détermination de la direction de mouvement
        float dotProduct = Vector3.Dot(target.forward, smoothVelocity.normalized);
        float targetBlend = dotProduct < 0 ? 1 : 0;
        currentDirectionBlend = Mathf.Lerp(currentDirectionBlend, targetBlend, directionChangeSpeed * deltaTime);

        // Calcul de l'offset de base
        Vector3 dynamicOffset = offsetPosition;
        dynamicOffset.z *= (1 + (currentDirectionBlend * (backwardOffsetMultiplier - 1)));

        // Ajout des effets dynamiques
        if (useDynamicCamera)
        {
            float speed = smoothVelocity.magnitude;
            float speedOffset = Mathf.Clamp(speed * speedEffect, 0, maxSpeedEffect);
            dynamicOffset.y += speed * heightSpeedMultiplier;
            dynamicOffset.z -= speedOffset;
        }

        // Calcul de la position cible
        Vector3 targetPosition = target.position + target.TransformDirection(dynamicOffset);

        // Application du smooth damp pour la position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        // Vérification de la distance minimale
        Vector3 directionToTarget = (transform.position - target.position).normalized;
        float currentDistance = Vector3.Distance(transform.position, target.position);
        if (currentDistance < minDistanceToTarget)
        {
            transform.position = target.position + directionToTarget * minDistanceToTarget;
        }

        // Rotation de la caméra
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            deltaTime / rotationSmoothTime
        );
    }

    public void ResetCameraPosition()
    {
        if (target == null) return;

        transform.position = target.position + target.TransformDirection(offsetPosition);
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);

        currentDirectionBlend = 0;
        smoothVelocity = Vector3.zero;
        positionVelocity = Vector3.zero;
        currentRotationVelocity = Vector3.zero;
    }
}