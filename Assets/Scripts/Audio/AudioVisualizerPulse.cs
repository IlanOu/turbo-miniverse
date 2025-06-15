using UnityEngine;

public class AudioVisualizerPulse : MonoBehaviour
{
    [Header("Cartoon Pulse Settings")]
    [SerializeField] private float pulseAmount = 0.2f;
    [SerializeField] private float squashStretch = 1.5f; // > 1 = plus d'effet squash & stretch
    [SerializeField] private bool exaggerateVertical = true; // Effet cartoon typique: étirer verticalement, comprimer horizontalement
    
    [Header("BPM Settings")]
    [SerializeField] private float defaultBPM = 120f;
    [SerializeField] private float beatMultiplier = 1f; // 1 = chaque beat, 0.5 = demi-beat, 2 = tous les 2 beats
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("References")]
    [SerializeField] private Transform objectToPulse;
    [SerializeField] private AudioPlaylistManager playlistManager;
    
    private Vector3 originalScale;
    private float currentBPM;
    private float beatInterval;
    private float beatTimer;
    private string currentTrackTitle = "";
    
    private void Start()
    {
        // Si aucun objet n'est assigné, utiliser cet objet
        if (objectToPulse == null)
        {
            objectToPulse = transform;
        }
        
        // Trouver l'AudioPlaylistManager si non assigné
        if (playlistManager == null)
        {
            playlistManager = FindObjectOfType<AudioPlaylistManager>();
            if (playlistManager == null)
            {
                Debug.LogError("CartoonSpeakerPulse: No AudioPlaylistManager found!");
            }
        }
        
        // Sauvegarder l'échelle originale
        originalScale = objectToPulse.localScale;
        
        // Initialiser le BPM
        SetBPM(defaultBPM);
        
        // S'abonner à l'événement de changement de piste
        if (playlistManager != null)
        {
            playlistManager.OnTrackChanged.AddListener(OnTrackChanged);
            
            // Vérifier si une piste est déjà en cours de lecture
            if (playlistManager.CurrentTrack != null)
            {
                OnTrackChanged(playlistManager.CurrentTrack);
            }
        }
    }
    
    private void OnEnable()
    {
        // S'abonner à l'événement quand le script est activé
        if (playlistManager != null)
        {
            playlistManager.OnTrackChanged.AddListener(OnTrackChanged);
            
            // Vérifier si une piste est déjà en cours de lecture
            if (playlistManager.CurrentTrack != null)
            {
                OnTrackChanged(playlistManager.CurrentTrack);
            }
        }
    }
    
    private void OnDisable()
    {
        // Se désabonner de l'événement quand le script est désactivé
        if (playlistManager != null)
        {
            playlistManager.OnTrackChanged.RemoveListener(OnTrackChanged);
        }
    }
    
    private void OnDestroy()
    {
        // Se désabonner de l'événement
        if (playlistManager != null)
        {
            playlistManager.OnTrackChanged.RemoveListener(OnTrackChanged);
        }
    }
    
    private void OnTrackChanged(AudioTrack track)
    {
        if (track != null)
        {
            // Mettre à jour le BPM en fonction de la piste
            currentTrackTitle = track.title;
            float trackBPM = track.bpm > 0 ? track.bpm : defaultBPM;
            
            if (showDebugInfo)
            {
                Debug.Log($"CartoonSpeakerPulse: Track changed to '{track.title}' with BPM {trackBPM}");
            }
            
            SetBPM(trackBPM);
        }
    }
    
    private void SetBPM(float bpm)
    {
        currentBPM = bpm;
        beatInterval = 60f / (currentBPM * beatMultiplier);
        beatTimer = 0f; // Réinitialiser le timer pour synchroniser avec le nouveau BPM
        
        if (showDebugInfo)
        {
            Debug.Log($"CartoonSpeakerPulse: BPM set to {currentBPM}, beat interval: {beatInterval}s");
        }
    }
    
    private void Update()
    {
        // Vérifier si la musique est en cours de lecture
        bool isPlaying = (playlistManager != null) ? playlistManager.IsPlaying : true;
        
        if (!isPlaying)
        {
            // Réinitialiser l'échelle si la musique n'est pas en cours de lecture
            objectToPulse.localScale = originalScale;
            return;
        }
        
        // Vérifier si la piste a changé (double vérification)
        if (playlistManager != null && playlistManager.CurrentTrack != null)
        {
            if (playlistManager.CurrentTrack.title != currentTrackTitle)
            {
                OnTrackChanged(playlistManager.CurrentTrack);
            }
        }
        
        // Incrémenter le timer
        beatTimer += Time.deltaTime;
        
        // Vérifier si on atteint un nouveau beat
        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval; // Conserver le reste pour une meilleure précision
            DoPulse();
        }
        else
        {
            // Animation entre les beats
            float beatProgress = beatTimer / beatInterval;
            
            // Effet cartoon: retour élastique à la normale
            float t = Mathf.Clamp01(beatProgress * 3f); // Retour plus rapide à la normale (3x)
            
            if (t < 1f)
            {
                // Appliquer l'effet squash & stretch
                ApplyCartoonScale(1f - t);
            }
            else
            {
                // Revenir à l'échelle normale
                objectToPulse.localScale = originalScale;
            }
        }
    }
    
    private void DoPulse()
    {
        // Effet cartoon: squash & stretch exagéré
        ApplyCartoonScale(1f);
    }
    
    private void ApplyCartoonScale(float intensity)
    {
        if (exaggerateVertical)
        {
            // Effet cartoon typique: étirer verticalement, comprimer horizontalement
            float verticalScale = 1f + (pulseAmount * intensity * squashStretch);
            float horizontalScale = 1f - (pulseAmount * intensity * 0.5f);
            
            objectToPulse.localScale = new Vector3(
                originalScale.x * horizontalScale,
                originalScale.y * verticalScale,
                originalScale.z * horizontalScale
            );
        }
        else
        {
            // Effet uniforme dans toutes les directions
            float mainScale = 1f + (pulseAmount * intensity);
            objectToPulse.localScale = originalScale * mainScale;
        }
    }
    
    // Méthode pour réinitialiser l'échelle
    public void ResetScale()
    {
        if (objectToPulse != null)
        {
            objectToPulse.localScale = originalScale;
        }
    }
    
    // Méthode pour déclencher manuellement une pulsation
    public void TriggerPulse()
    {
        DoPulse();
    }
    
    // Méthode pour définir manuellement le BPM
    public void SetManualBPM(float bpm)
    {
        SetBPM(bpm);
    }
}
