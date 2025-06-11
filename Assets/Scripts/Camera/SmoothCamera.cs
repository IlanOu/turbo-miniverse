using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] public Transform target;
    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2.5f, -4.5f);
    [SerializeField] private float lookAtHeight = 0.5f;

    [Header("Camera Behavior")]
    [SerializeField] private float positionSmoothTime = 0.15f;
    [SerializeField] private float rotationSmoothTime = 0.15f;
    [SerializeField] private bool useFixedUpdate = true;
    [SerializeField] private float minDistanceToTarget = 3f;
    [SerializeField] private float maxAllowedDistance = 15f; // Distance max avant réinitialisation

    [Header("Direction Settings")]
    [SerializeField] private float backwardOffsetMultiplier = 1.2f;
    [SerializeField] private float directionChangeSpeed = 3f;
    [SerializeField] private float directionThreshold = 0.3f;

    [Header("Racing Camera Effects")]
    [SerializeField] private float speedTiltFactor = 0.01f;
    [SerializeField] private float maxTiltAngle = 3f;
    [SerializeField] private float corneringTiltFactor = 0.5f;
    [SerializeField] private float maxCorneringTilt = 5f;
    [SerializeField] private float speedFOVFactor = 0.008f;
    [SerializeField] private float maxAdditionalFOV = 10f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private bool avoidObstacles = true;
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
    private Vector3 lastLocalTargetPosition; // Position locale de la cible au dernier frame

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
        lastLocalTargetPosition = target.localPosition; // Stocker la position locale initiale
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

    private void LateUpdate()
    {
        // Vérifier si la cible a été téléportée en comparant sa position locale
        if (target != null && Vector3.Distance(lastLocalTargetPosition, target.localPosition) > 1f)
        {
            // La position locale a changé significativement, probablement une téléportation
            ResetCameraPosition();
            lastLocalTargetPosition = target.localPosition;
        }
        
        // Vérifier si la caméra est trop loin de la cible
        if (target != null && Vector3.Distance(transform.position, target.position) > maxAllowedDistance)
        {
            ResetCameraPosition();
        }
    }

    private void UpdateCamera(float deltaTime)
    {
        if (target == null) return;

        // Mettre à jour la position locale de la cible
        lastLocalTargetPosition = target.localPosition;
        
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
        
        // Position de base de la caméra - utilise correctement l'offset Y
        Vector3 targetPosition = target.position + target.TransformDirection(new Vector3(dynamicOffset.x, 0, dynamicOffset.z));
        // Ajouter la hauteur Y directement (pas transformée) pour qu'elle soit toujours relative au monde
        targetPosition.y = target.position.y + offsetPosition.y;
        
        // Gestion des obstacles
        if (avoidObstacles)
        {
            HandleObstacleAvoidance(ref targetPosition);
        }

        // Application du smooth damp pour la position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
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
                    if (Physics.Raycast(target.position, directionToCamera + rightOffset.normalized, obstacleDetectionDistance, obstacleLayers))
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

        // Position correcte avec la hauteur Y appliquée directement
        Vector3 position = target.position + target.TransformDirection(new Vector3(offsetPosition.x, 0, -Mathf.Abs(offsetPosition.z)));
        position.y = target.position.y + offsetPosition.y;
        
        // Définir directement la position locale de la caméra par rapport au parent
        if (transform.parent != null && target.parent == transform.parent)
        {
            // Si la caméra et la cible ont le même parent, utiliser la position locale
            Vector3 localPosition = transform.parent.InverseTransformPoint(position);
            transform.localPosition = localPosition;
        }
        else
        {
            // Sinon, utiliser la position mondiale
            transform.position = position;
        }
        
        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position);

        currentDirectionBlend = 0;
        currentTilt = 0;
        targetTilt = 0;
        smoothVelocity = Vector3.zero;
        positionVelocity = Vector3.zero;
        
        if (cam != null)
        {
            cam.fieldOfView = baseFOV;
        }
    }

}
