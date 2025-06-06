using UnityEngine;

public class ElectricCarSound : MonoBehaviour
{
    [SerializeField] private AudioSource motorSound;
    
    [Header("Paramètres du son")]
    [SerializeField] private float minPitch = 0.7f;      // Pitch minimum plus élevé pour un son moins grave
    [SerializeField] private float maxPitch = 1.2f;      // Pitch maximum réduit pour moins d'agressivité
    [SerializeField] private float maxVolume = 0.4f;     // Volume maximum réduit
    [SerializeField] private float pitchChangeSpeed = 2.0f;
    [SerializeField] private float volumeChangeSpeed = 3.0f;
    
    [Header("Paramètres de vitesse")]
    [SerializeField] private float maxSpeedKmh = 200f;
    [SerializeField] private float pitchMaxAtSpeedKmh = 120f;
    [SerializeField] [Range(0.5f, 3f)] private float pitchCurve = 0.8f;
    
    private float currentPitch;
    private float targetPitch;
    private float currentVolume;
    private float targetVolume;
    
    private float currentSpeedKmh = 0f;
    
    private void Start()
    {
        if (motorSound == null)
        {
            motorSound = gameObject.AddComponent<AudioSource>();
            motorSound.loop = true;
            motorSound.playOnAwake = false;
        }
        
        // Réduire la distance de propagation du son
        motorSound.maxDistance = 15f;
        motorSound.spatialBlend = 1f;  // Son entièrement 3D
        motorSound.rolloffMode = AudioRolloffMode.Linear;
        
        motorSound.volume = 0;
        motorSound.Play();
        
        currentPitch = minPitch;
        currentVolume = 0;
    }
    
    private void Update()
    {
        // Calculer le ratio de vitesse normalisé
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / pitchMaxAtSpeedKmh);
        
        // Appliquer une courbe non linéaire
        float pitchFactor = Mathf.Pow(speedRatio, pitchCurve);
        
        // Calculer le pitch cible
        targetPitch = Mathf.Lerp(minPitch, maxPitch, pitchFactor);
        
        // Calculer le volume cible avec une courbe plus douce
        float volumeFactor = Mathf.Pow(speedRatio, 1.2f); // Courbe légèrement différente pour le volume
        
        // Volume minimal très faible au ralenti, nul à l'arrêt
        float minIdleVolume = 0.02f;
        targetVolume = currentSpeedKmh < 0.5f ? 0 : Mathf.Lerp(minIdleVolume, maxVolume, volumeFactor);
        
        // Transition douce du pitch
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * pitchChangeSpeed);
        motorSound.pitch = currentPitch;
        
        // Transition douce du volume
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * volumeChangeSpeed);
        motorSound.volume = currentVolume;
    }
    
    // Méthode pour définir la vitesse en km/h
    public void SetSpeed(float speedKmh)
    {
        currentSpeedKmh = Mathf.Clamp(speedKmh, 0f, maxSpeedKmh);
    }
    
    // Alternative: définir la vitesse comme pourcentage (0-1) de la vitesse maximale
    public void SetSpeedNormalized(float normalizedSpeed)
    {
        currentSpeedKmh = Mathf.Clamp01(normalizedSpeed) * maxSpeedKmh;
    }
}
