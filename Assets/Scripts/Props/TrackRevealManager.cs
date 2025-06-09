using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackRevealManager : MonoBehaviour
{
    [Header("Track Pieces")]
    [SerializeField] private List<TrackPiece> allTrackPieces = new List<TrackPiece>();
    
    [Header("Reveal Camera")]
    [SerializeField] private Camera revealCamera; // Une seule caméra de révélation fixe
    
    [Header("Animation Settings")]
    [SerializeField] private float cameraFadeDuration = 0.5f;
    [SerializeField] private float pieceRevealDelay = 0.5f;
    [SerializeField] private float piecesInterval = 1.0f;
    [SerializeField] private float totalRevealDuration = 2f;
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Completion Sequence Settings")]
    [SerializeField] private float completionCameraDistance = 10f; // Distance de la caméra au centre
    [SerializeField] private float completionCameraHeight = 5f; // Hauteur de la caméra
    [SerializeField] private float completionRotationDuration = 5f; // Durée de la rotation
    [SerializeField] private float completionRotationSpeed = 1f; // Vitesse de rotation (multiplicateur)
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem confettiEffect;
    [SerializeField] private AudioClip revealSound;
    [SerializeField] private AudioClip completionSound;
    
    // Référence au fondu noir
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    
    private AudioSource audioSource;
    private bool isRevealing = false;
    private int unlockedPiecesCount = 0;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Créer le fondu noir s'il n'existe pas
        if (fadeCanvasGroup == null)
        {
            CreateFadeCanvas();
        }
        
        // S'assurer que le fondu est transparent au démarrage
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        
        // Désactiver la caméra de révélation au démarrage
        if (revealCamera != null)
        {
            revealCamera.gameObject.SetActive(false);
        }
        
        // Cacher toutes les pièces de circuit au démarrage
        foreach (TrackPiece piece in allTrackPieces)
        {
            if (piece != null)
            {
                piece.HideInstantly();
            }
        }
    }
    
    private void CreateFadeCanvas()
    {
        // Créer un canvas pour le fondu
        GameObject fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // S'assurer qu'il est au-dessus de tout
        
        // Ajouter un CanvasScaler
        CanvasScaler scaler = fadeCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Créer un panneau noir
        GameObject panel = new GameObject("BlackPanel");
        panel.transform.SetParent(fadeCanvas.transform, false);
        
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        
        // Ajouter le CanvasGroup
        fadeCanvasGroup = panel.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.blocksRaycasts = false;
        
        // Ne pas détruire lors du changement de scène
        DontDestroyOnLoad(fadeCanvas);
    }
    
    // Appelé lorsqu'un coffre est ouvert
    public void RevealTrackPieces(List<int> pieceIndices)
    {
        if (isRevealing || pieceIndices == null || pieceIndices.Count == 0 || revealCamera == null)
            return;
            
        StartCoroutine(RevealSequence(pieceIndices));
    }
    
    private IEnumerator RevealSequence(List<int> pieceIndices)
    {
        isRevealing = true;
        
        // Fondu au noir
        yield return FadeToBlack(true);
        
        // Désactiver la caméra principale et activer la caméra de révélation
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }
        revealCamera.gameObject.SetActive(true);
        
        // Fondu depuis le noir
        yield return FadeToBlack(false);
        
        // Révéler chaque pièce de circuit avec un délai entre elles
        foreach (int pieceIndex in pieceIndices)
        {
            if (pieceIndex >= 0 && pieceIndex < allTrackPieces.Count)
            {
                yield return RevealTrackPieceAnimation(pieceIndex);
                
                // Attendre un moment avant de passer à la pièce suivante
                yield return new WaitForSeconds(piecesInterval);
                
                // Incrémenter le compteur de pièces débloquées
                unlockedPiecesCount++;
            }
        }
        
        // Attendre un moment pour que le joueur puisse voir les pièces complètes
        yield return new WaitForSeconds(1.5f);
        
        // Vérifier si toutes les pièces sont débloquées
        if (unlockedPiecesCount >= allTrackPieces.Count)
        {
            yield return PlayCompletionSequence();
        }
        
        // Fondu au noir
        yield return FadeToBlack(true);
        
        // Désactiver la caméra de révélation et réactiver la caméra principale
        revealCamera.gameObject.SetActive(false);
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }
        
        // Fondu depuis le noir
        yield return FadeToBlack(false);
        
        isRevealing = false;
    }
    
    private IEnumerator FadeToBlack(bool fadeIn)
    {
        if (fadeCanvasGroup == null)
            yield break;
            
        float startAlpha = fadeIn ? 0 : 1;
        float endAlpha = fadeIn ? 1 : 0;
        float elapsed = 0;
        
        fadeCanvasGroup.alpha = startAlpha;
        fadeCanvasGroup.blocksRaycasts = fadeIn;
        
        while (elapsed < cameraFadeDuration)
        {
            float t = elapsed / cameraFadeDuration;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        fadeCanvasGroup.alpha = endAlpha;
        fadeCanvasGroup.blocksRaycasts = fadeIn;
    }
    
    private IEnumerator RevealTrackPieceAnimation(int pieceIndex)
    {
        TrackPiece piece = allTrackPieces[pieceIndex];
        if (piece == null)
            yield break;
            
        // Jouer le son de révélation
        if (revealSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(revealSound);
        }
        
        // Attendre un court délai
        yield return new WaitForSeconds(pieceRevealDelay);
        
        // Déclencher l'animation de la pièce
        piece.SetRevealProgress(0.1f);
        
        // Attendre que l'animation se termine
        float estimatedDuration = piece.GetEstimatedDuration();
        yield return new WaitForSeconds(estimatedDuration);
        
        // Jouer l'effet de confettis si disponible
        if (confettiEffect != null)
        {
            Vector3 confettiPosition = piece.transform.position + Vector3.up * 2f;
            Instantiate(confettiEffect, confettiPosition, Quaternion.identity).Play();
        }
    }
    
    private IEnumerator PlayCompletionSequence()
    {
        // Jouer le son de complétion
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        // Calculer le centre du circuit
        Vector3 circuitCenter = CalculateCircuitCenter();
        
        // Faire tourner la caméra autour du circuit
        float elapsed = 0f;
        
        Vector3 startPos = revealCamera.transform.position;
        Quaternion startRot = revealCamera.transform.rotation;
        
        while (elapsed < completionRotationDuration)
        {
            // Calculer l'angle en fonction du temps et de la vitesse
            float angle = elapsed / completionRotationDuration * 360f * completionRotationSpeed;
            
            // Calculer la nouvelle position de la caméra
            Vector3 newPos = circuitCenter + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * completionCameraDistance,
                completionCameraHeight,
                Mathf.Sin(angle * Mathf.Deg2Rad) * completionCameraDistance
            );
            
            // Appliquer la position et faire regarder la caméra vers le centre
            revealCamera.transform.position = newPos;
            revealCamera.transform.LookAt(circuitCenter);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restaurer la position de la caméra de révélation
        revealCamera.transform.position = startPos;
        revealCamera.transform.rotation = startRot;
        
        yield return new WaitForSeconds(1f);
    }
    
    private Vector3 CalculateCircuitCenter()
    {
        if (allTrackPieces.Count == 0)
            return Vector3.zero;
            
        Vector3 sum = Vector3.zero;
        foreach (TrackPiece piece in allTrackPieces)
        {
            if (piece != null)
            {
                sum += piece.transform.position;
            }
        }
        
        return sum / allTrackPieces.Count;
    }
}
