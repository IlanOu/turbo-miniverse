using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Target Settings")] [SerializeField]
    public Transform target;

    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2.5f, -4.5f);
    [SerializeField] private float lookAtHeight = 0.5f;

    [Header("Camera Behavior")] [SerializeField]
    private float positionSmoothTime = 0.15f;

    [SerializeField] private float rotationSmoothTime = 0.15f;
    [SerializeField] private bool useFixedUpdate = true;
    [SerializeField] private float minDistanceToTarget = 3f;

    [Header("Direction Settings")] [SerializeField]
    private float backwardOffsetMultiplier = 1.2f;

    [SerializeField] private float directionChangeSpeed = 3f;
    [SerializeField] private float directionThreshold = 0.3f;

    [Header("Racing Camera Effects")] [SerializeField]
    private float speedTiltFactor = 0.01f;

    [SerializeField] private float maxTiltAngle = 3f;
    [SerializeField] private float corneringTiltFactor = 0.5f;
    [SerializeField] private float maxCorneringTilt = 5f;
    [SerializeField] private float speedFOVFactor = 0.008f;
    [SerializeField] private float maxAdditionalFOV = 10f;

    [Header("Obstacle Avoidance")] [SerializeField]
    private bool avoidObstacles = true;

    [SerializeField] private LayerMask obstacleLayers = 1;
    [SerializeField] private float obstacleDetectionDistance = 5f;
    [SerializeField] private float recoverySpeed = 3f;
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private float heightAdjustmentFactor = 0.5f;

    // Variables privées
    private Vector3 lastTargetPosition;
    private Vector3 targetVelocity;
    private Vector3 smoothVelocity;
    private Vector3 positionVelocity;
    private float currentDirectionBlend;
    private float currentTilt;
    private float targetTilt;
    private float baseFOV;
    private Camera cam;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to camera!");
            enabled = false;
            return;
        }

        cam = GetComponent<Camera>();
        if (cam != null)
        {
            baseFOV = cam.fieldOfView;
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
        targetVelocity = (target.position - lastTargetPosition) / deltaTime;
        lastTargetPosition = target.position;

        // Lissage de la vélocité
        smoothVelocity = Vector3.Lerp(smoothVelocity, targetVelocity, deltaTime * 8f);

        // Détermination de la direction (avant/arrière)
        float dotProduct = Vector3.Dot(target.forward, smoothVelocity.normalized);
        float targetBlend = dotProduct < -directionThreshold ? 1 : 0; // Marche arrière si négatif
        currentDirectionBlend = Mathf.Lerp(currentDirectionBlend, targetBlend, directionChangeSpeed * deltaTime);

        // Calcul de l'offset en fonction de la direction
        Vector3 dynamicOffset = offsetPosition;
        if (currentDirectionBlend > 0.1f) // Si en marche arrière
        {
            // Inverser l'offset Z pour placer la caméra devant
            dynamicOffset.z = Mathf.Abs(offsetPosition.z) * backwardOffsetMultiplier;
        }
        else
        {
            // Caméra derrière en marche avant
            dynamicOffset.z = -Mathf.Abs(offsetPosition.z);
        }

        // Position de base de la caméra
        Vector3 desiredPosition =
            target.position + target.TransformDirection(new Vector3(dynamicOffset.x, 0, dynamicOffset.z));

        // Garantir une hauteur minimale absolue par rapport au sol
        float minAbsoluteHeight = 0.5f; // Hauteur minimale par rapport au sol

        // Raycast pour trouver le sol sous la position désirée
        RaycastHit groundHit;
        if (Physics.Raycast(new Vector3(desiredPosition.x, desiredPosition.y + 10f, desiredPosition.z), Vector3.down,
                out groundHit, 20f, obstacleLayers))
        {
            float groundLevel = groundHit.point.y;
            float minHeightFromGround = groundLevel + minAbsoluteHeight;

            // Garantir que la hauteur désirée est au moins à la hauteur minimale du sol
            desiredPosition.y = Mathf.Max(target.position.y + offsetPosition.y, minHeightFromGround);
        }
        else
        {
            // Si pas de sol détecté, utiliser la hauteur normale
            desiredPosition.y = target.position.y + offsetPosition.y;
        }

        // Vérifier si la ligne entre le target et la caméra traverse la voiture ou le sol
        Vector3 directionToCamera = (desiredPosition - target.position).normalized;
        float distanceToCamera = Vector3.Distance(target.position, desiredPosition);

        RaycastHit[] hits = Physics.RaycastAll(
            target.position + Vector3.up * 0.5f, // Légèrement au-dessus du centre de la voiture
            directionToCamera,
            distanceToCamera,
            obstacleLayers
        );

        bool needsHeightAdjustment = false;

        foreach (RaycastHit hit in hits)
        {
            // Ignorer le hit si c'est la caméra elle-même
            if (hit.transform == transform) continue;

            // Si on touche quelque chose (voiture ou sol), ajuster la hauteur
            if (hit.transform == target || hit.transform.IsChildOf(target) ||
                hit.normal.y > 0.7f) // Si c'est la voiture ou une surface horizontale (sol)
            {
                needsHeightAdjustment = true;
                break;
            }
        }

        if (needsHeightAdjustment)
        {
            // Augmenter significativement la hauteur pour passer au-dessus de l'obstacle
            float heightBoost = 2.0f; // Ajustez selon vos besoins
            desiredPosition.y = Mathf.Max(desiredPosition.y, target.position.y + heightBoost);
        }

        // Gestion des obstacles
        if (avoidObstacles)
        {
            HandleObstacleAvoidance(ref desiredPosition);
        }

        // Application du smooth damp pour la position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        // Vérification de la distance minimale
        Vector3 horizontalTargetPos = new Vector3(target.position.x, 0, target.position.z);
        Vector3 horizontalCameraPos = new Vector3(transform.position.x, 0, transform.position.z);
        float horizontalDistance = Vector3.Distance(horizontalCameraPos, horizontalTargetPos);

        if (horizontalDistance < minDistanceToTarget)
        {
            Vector3 horizontalDirection = (horizontalCameraPos - horizontalTargetPos).normalized;
            Vector3 newHorizontalPos = horizontalTargetPos + horizontalDirection * minDistanceToTarget;
            transform.position = new Vector3(newHorizontalPos.x, transform.position.y, newHorizontalPos.z);
        }

        // Calcul de l'inclinaison en virage
        Vector3 targetRight = target.right;
        float lateralVelocity = Vector3.Dot(smoothVelocity, targetRight);
        float speed = smoothVelocity.magnitude;

        // Inclinaison basée sur la vitesse et les virages
        targetTilt = -lateralVelocity * corneringTiltFactor;
        targetTilt = Mathf.Clamp(targetTilt, -maxCorneringTilt, maxCorneringTilt);

        // Ajout d'une légère inclinaison basée sur la vitesse
        float speedTilt = speed * speedTiltFactor;
        speedTilt = Mathf.Clamp(speedTilt, 0, maxTiltAngle);

        // Lissage de l'inclinaison
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, deltaTime * 5f);

        // Point de visée
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;

        // Rotation de base
        Quaternion baseRotation = Quaternion.LookRotation(lookAtPoint - transform.position);

        // Application de l'inclinaison
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            baseRotation * Quaternion.Euler(0, 0, currentTilt),
            deltaTime / rotationSmoothTime
        );

        // Ajustement du FOV basé sur la vitesse
        if (cam != null)
        {
            float targetFOV = baseFOV + Mathf.Clamp(speed * speedFOVFactor, 0, maxAdditionalFOV);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, deltaTime * 3f);
        }
    }

    private void HandleObstacleAvoidance(ref Vector3 targetPosition)
    {
        // Direction et distance entre la cible et la position souhaitée de la caméra
        Vector3 directionToCamera = (targetPosition - target.position).normalized;
        float distanceToCamera = Vector3.Distance(targetPosition, target.position);

        // Lancer un rayon pour détecter les obstacles
        RaycastHit hit;
        if (Physics.Raycast(target.position, directionToCamera, out hit, obstacleDetectionDistance, obstacleLayers))
        {
            if (hit.distance < distanceToCamera)
            {
                // Obstacle détecté, rapprocher la caméra
                targetPosition = hit.point - (directionToCamera * collisionOffset);

                // Préserver la hauteur Y définie par l'utilisateur autant que possible
                float desiredHeight = target.position.y + offsetPosition.y;
                float currentHeight = targetPosition.y;

                // Si l'obstacle est trop proche, essayer d'ajuster la hauteur légèrement
                if (Vector3.Distance(targetPosition, target.position) < minDistanceToTarget * 0.7f)
                {
                    // Ajuster la hauteur pour voir par-dessus l'obstacle, mais pas trop
                    float heightAdjustment = (desiredHeight - currentHeight) * heightAdjustmentFactor;
                    targetPosition.y += heightAdjustment;
                }
                else
                {
                    // Sinon, essayer de maintenir la hauteur désirée
                    targetPosition.y = Mathf.Lerp(targetPosition.y, desiredHeight, 0.5f);
                }

                // Si l'obstacle est vraiment trop proche, essayer de déplacer latéralement
                if (Vector3.Distance(targetPosition, target.position) < minDistanceToTarget * 0.5f)
                {
                    Vector3 rightOffset = target.right * 1.5f;
                    if (Physics.Raycast(target.position, directionToCamera + rightOffset.normalized,
                            obstacleDetectionDistance, obstacleLayers))
                    {
                        rightOffset = -rightOffset; // Essayer l'autre côté
                    }

                    targetPosition += rightOffset * 0.5f;
                }
            }
        }
    }

    public void ResetCameraPosition()
    {
        if (target == null) return;

        // Réinitialiser toutes les variables de suivi
        lastTargetPosition = target.position;
        targetVelocity = Vector3.zero;
        smoothVelocity = Vector3.zero;
        positionVelocity = Vector3.zero;
        currentDirectionBlend = 0;
        currentTilt = 0;
        targetTilt = 0;

        // Réinitialiser le FOV
        if (cam != null)
        {
            cam.fieldOfView = baseFOV;
        }

        // Position correcte avec la hauteur Y appliquée directement
        Vector3 position = target.position +
                           target.TransformDirection(new Vector3(offsetPosition.x, 0, -Mathf.Abs(offsetPosition.z)));
        position.y = target.position.y + offsetPosition.y;

        // Définir directement la position de la caméra
        transform.position = position;

        // Réinitialiser la rotation
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);
    }
}