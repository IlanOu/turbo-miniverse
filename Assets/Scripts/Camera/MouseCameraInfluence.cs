using UnityEngine;

public class MouseCameraInfluence : MonoBehaviour
{
    [Header("Mouse Influence Settings")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float maxHorizontalInfluence = 30.0f;
    [SerializeField] private float influenceReturnSpeed = 1.0f;
    [SerializeField] private bool invertMouseX = false;
    
    [Header("References")]
    [SerializeField] private SmoothCamera smoothCamera;
    
    // Variables privées
    private float currentMouseInfluence = 0f;
    private Transform originalTarget;
    private GameObject mouseTargetObj;
    private Transform mouseTarget;
    
    private void Start()
    {
        if (smoothCamera == null)
        {
            smoothCamera = GetComponent<SmoothCamera>();
            if (smoothCamera == null)
            {
                Debug.LogError("No SmoothCamera component found!");
                enabled = false;
                return;
            }
        }
        
        // Sauvegarder la cible originale
        originalTarget = smoothCamera.target;
        
        // Créer un objet cible virtuel pour l'influence de la souris
        mouseTargetObj = new GameObject("MouseCameraTarget");
        mouseTarget = mouseTargetObj.transform;
        
        // Initialiser la position du mouseTarget à celle de la cible originale
        UpdateMouseTargetPosition();
    }
    
    private void Update()
    {
        // Capturer l'entrée de la souris sur l'axe horizontal
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        if (invertMouseX) mouseX = -mouseX;
        
        // Appliquer l'influence de la souris
        currentMouseInfluence += mouseX;
        currentMouseInfluence = Mathf.Clamp(currentMouseInfluence, -maxHorizontalInfluence, maxHorizontalInfluence);
        
        // Retour progressif à zéro quand la souris n'est pas utilisée
        if (Mathf.Abs(mouseX) < 0.01f)
        {
            currentMouseInfluence = Mathf.Lerp(currentMouseInfluence, 0, Time.deltaTime * influenceReturnSpeed);
        }
        
        // Mettre à jour la position de la cible virtuelle
        UpdateMouseTargetPosition();
        
        // Utiliser la cible virtuelle pour la caméra
        smoothCamera.target = mouseTarget;
    }
    
    private void UpdateMouseTargetPosition()
    {
        if (originalTarget == null || mouseTarget == null) return;
        
        // Copier la position et la rotation de la cible originale
        mouseTarget.position = originalTarget.position;
        mouseTarget.rotation = originalTarget.rotation;
        
        // Appliquer la rotation d'influence de la souris
        mouseTarget.Rotate(Vector3.up, currentMouseInfluence);
    }
    
    private void OnDestroy()
    {
        // Restaurer la cible originale
        if (smoothCamera != null)
        {
            smoothCamera.target = originalTarget;
        }
        
        // Nettoyer l'objet cible virtuel
        if (mouseTargetObj != null)
        {
            Destroy(mouseTargetObj);
        }
    }
}
