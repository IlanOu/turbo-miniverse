using Car;
using UnityEngine;
using DG.Tweening;

public class JumpJuice : MonoBehaviour
{
    [SerializeField] private Transform carMesh;
    [SerializeField] private CarController carController;
    
    [Header("Jump Effect")]
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private float stretchFactor = 1.3f;
    
    private Vector3 originalScale;
    
    void Start()
    {
        if (carMesh == null && transform.childCount > 0)
            carMesh = transform.GetChild(0);
            
        if (carController == null)
            carController = GetComponent<CarController>();
            
        originalScale = carMesh.localScale;
        
        carController.onJump.AddListener(OnJump);
    }
    
    void OnJump()
    {
        // Arrêter toute animation en cours
        carMesh.DOKill();
        
        // Séquence simple d'étirement et retour
        Sequence jumpSequence = DOTween.Sequence();
        
        // Étirement vertical
        Vector3 stretchScale = new Vector3(
            originalScale.x * 0.9f,
            originalScale.y * stretchFactor,
            originalScale.z * 0.9f
        );
        
        jumpSequence.Append(carMesh.DOScale(stretchScale, effectDuration * 0.3f).SetEase(Ease.OutQuad));
        jumpSequence.Append(carMesh.DOScale(originalScale, effectDuration * 0.7f).SetEase(Ease.OutElastic));
    }
    
    void OnDestroy()
    {
        if (carController != null)
            carController.onJump.RemoveListener(OnJump);
            
        carMesh.DOKill();
    }
}