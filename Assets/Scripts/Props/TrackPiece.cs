using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackPiece : MonoBehaviour
{
    [Header("Segments")]
    [SerializeField] private List<GameObject> pieceSegments = new List<GameObject>();
    
    [Header("Animation Settings")]
    [SerializeField] private float segmentDropHeight = 15f; // Hauteur de départ des segments
    [SerializeField] private float segmentDropDelay = 0.15f; // Délai entre chaque segment
    [SerializeField] private float segmentDropDuration = 1.2f; // Durée de chute d'un segment (plus lent)
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Materials")]
    [SerializeField] private Material normalMaterial; // Matériau normal (optionnel)
    
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Renderer> renderers = new List<Renderer>();
    
    private void Awake()
    {
        // Collecter tous les renderers et positions originales
        foreach (GameObject segment in pieceSegments)
        {
            if (segment != null)
            {
                originalPositions.Add(segment.transform.position);
                
                Renderer renderer = segment.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderers.Add(renderer);
                }
            }
        }
        
        // Cacher la pièce au démarrage
        HideInstantly();
    }
    
    public void HideInstantly()
    {
        // Cacher tous les segments
        for (int i = 0; i < pieceSegments.Count; i++)
        {
            if (pieceSegments[i] != null)
            {
                // Déplacer vers le haut et cacher
                pieceSegments[i].transform.position = originalPositions[i] + Vector3.up * segmentDropHeight;
                
                if (i < renderers.Count && renderers[i] != null)
                {
                    renderers[i].enabled = false;
                }
            }
        }
    }
    
    public void SetRevealProgress(float progress)
    {
        // Cette méthode est appelée par le TrackRevealManager
        // Nous l'utilisons pour démarrer l'animation de révélation
        if (progress > 0 && progress <= 0.1f)
        {
            StartCoroutine(AnimateSegmentDrop());
        }
    }
    
    private IEnumerator AnimateSegmentDrop()
    {
        // Faire descendre chaque segment un par un
        for (int i = 0; i < pieceSegments.Count; i++)
        {
            if (pieceSegments[i] != null && i < renderers.Count && i < originalPositions.Count)
            {
                // Rendre le segment visible
                renderers[i].enabled = true;
                
                // Position de départ (en haut)
                Vector3 startPos = originalPositions[i] + Vector3.up * segmentDropHeight;
                pieceSegments[i].transform.position = startPos;
                
                // Animer la descente douce
                StartCoroutine(AnimateSingleSegment(pieceSegments[i], startPos, originalPositions[i]));
                
                // Attendre avant de faire descendre le segment suivant
                yield return new WaitForSeconds(segmentDropDelay);
            }
        }
    }
    
    private IEnumerator AnimateSingleSegment(GameObject segment, Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        
        while (elapsed < segmentDropDuration)
        {
            float t = elapsed / segmentDropDuration;
            float curvedT = dropCurve.Evaluate(t);
            
            // Descente douce et fluide
            segment.transform.position = Vector3.Lerp(startPos, endPos, curvedT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // S'assurer que le segment est exactement à sa position finale
        segment.transform.position = endPos;
    }
    
    // Méthode pour estimer la durée totale de l'animation
    public float GetEstimatedDuration()
    {
        return pieceSegments.Count * segmentDropDelay + segmentDropDuration;
    }
}
