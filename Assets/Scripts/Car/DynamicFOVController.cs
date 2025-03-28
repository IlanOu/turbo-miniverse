using UnityEngine;

public class DynamicFOVController : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float speedThreshold = 150f; // En km/h
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float maxAdditionalFOV = 20f;
    [SerializeField] private float fovLerpSpeed = 5f;
    [SerializeField] private float maxSpeed = 300f; // Vitesse à laquelle le FOV max est atteint

    // Variables privées
    private Rigidbody targetRigidbody;
    private float currentFOV;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Veuillez assigner une cible au contrôleur FOV dans l'inspecteur!");
            return;
        }

        if (targetCamera == null)
        {
            // Essayer de trouver la caméra sur cet objet
            targetCamera = GetComponent<Camera>();
            
            if (targetCamera == null)
            {
                Debug.LogError("Aucune caméra assignée ou trouvée sur cet objet!");
                return;
            }
        }

        // Récupérer le Rigidbody de la cible
        targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody == null)
        {
            Debug.LogError("La cible doit avoir un Rigidbody!");
            return;
        }

        // Initialiser le FOV actuel
        currentFOV = targetCamera.fieldOfView;
    }

    private void Update()
    {
        if (targetRigidbody == null || targetCamera == null)
            return;

        // Calculer la vitesse en km/h
        float speedKmh = targetRigidbody.linearVelocity.magnitude * 3.6f; // Convertir m/s en km/h

        // Calculer le FOV cible
        float targetFOV = defaultFOV;
        
        if (speedKmh > speedThreshold)
        {
            // Calculer le pourcentage de vitesse au-delà du seuil
            float speedFactor = Mathf.Clamp01((speedKmh - speedThreshold) / (maxSpeed - speedThreshold));
            
            // Appliquer une augmentation progressive du FOV
            targetFOV = defaultFOV + (maxAdditionalFOV * speedFactor);
        }

        // Appliquer le FOV de façon smooth
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovLerpSpeed * Time.deltaTime);
        targetCamera.fieldOfView = currentFOV;
    }
}