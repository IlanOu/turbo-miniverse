using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpeedParticleController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    public Transform target;
    [SerializeField] public List<ParticleSystem> speedEffects = new List<ParticleSystem>();
    
    [Header("Speed Settings")]
    [SerializeField] private float activationSpeed = 80f; // Vitesse d'activation en km/h
    [SerializeField] private float maxSpeed = 200f; // Vitesse pour effet maximum en km/h
    [SerializeField] private float transitionSpeed = 3f; // Vitesse de transition
    
    [Header("Effect Control")]
    [SerializeField] private bool controlEmissionRate = true;
    [SerializeField] private bool controlAlpha = true;
    [SerializeField] private bool useStaggeredActivation = false; // Activer les effets un par un selon la vitesse
    
    // Variables privées
    private Rigidbody targetRigidbody;
    private List<ParticleSystem.EmissionModule> emissionModules = new List<ParticleSystem.EmissionModule>();
    private List<ParticleSystem.MainModule> mainModules = new List<ParticleSystem.MainModule>();
    private List<float> originalEmissionRates = new List<float>();
    private List<Color> originalColors = new List<Color>();
    
    private float currentIntensity = 0f;
    private bool areEffectsActive = false;

    private void Awake()
    {
        // Désactiver tous les systèmes de particules dès le début
        foreach (ParticleSystem ps in speedEffects)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var emission = ps.emission;
                emission.enabled = false;
            }
        }
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Veuillez assigner une cible au contrôleur d'effets de vitesse!");
            return;
        }

        if (speedEffects.Count == 0)
        {
            Debug.LogError("Veuillez assigner au moins un effet de particules!");
            return;
        }

        targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody == null)
        {
            Debug.LogError("La cible doit avoir un Rigidbody!");
            return;
        }

        InitializeParticleSystems();
        
        // S'assurer que tous les effets sont arrêtés au démarrage
        StopAllEffects();
    }

    private void InitializeParticleSystems()
    {
        emissionModules.Clear();
        mainModules.Clear();
        originalEmissionRates.Clear();
        originalColors.Clear();
        
        // Initialiser les modules et sauvegarder les valeurs originales
        foreach (ParticleSystem ps in speedEffects)
        {
            if (ps != null)
            {
                var emission = ps.emission;
                var main = ps.main;
                
                emissionModules.Add(emission);
                mainModules.Add(main);
                originalEmissionRates.Add(emission.rateOverTime.constant);
                originalColors.Add(main.startColor.color);
                
                // S'assurer que le système est arrêté et vide
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                emission.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (targetRigidbody == null || speedEffects.Count == 0)
            return;

        // Calculer la vitesse en km/h
        float speedKmh = targetRigidbody.velocity.magnitude * 3.6f;

        // Déterminer si les effets doivent être actifs
        bool shouldBeActive = speedKmh >= activationSpeed;

        // Gérer l'activation/désactivation des effets
        if (shouldBeActive && !areEffectsActive)
        {
            PlayAllEffects();
            areEffectsActive = true;
        }
        else if (!shouldBeActive && areEffectsActive)
        {
            StopAllEffects();
            areEffectsActive = false;
            currentIntensity = 0f;
            return;
        }

        // Si les effets sont actifs, calculer l'intensité et mettre à jour
        if (areEffectsActive)
        {
            float targetIntensity = Mathf.Clamp01((speedKmh - activationSpeed) / (maxSpeed - activationSpeed));
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, transitionSpeed * Time.deltaTime);
            
            UpdateEffectsIntensity();
        }
    }

    private void PlayAllEffects()
    {
        for (int i = 0; i < speedEffects.Count; i++)
        {
            if (speedEffects[i] != null && i < emissionModules.Count)
            {
                // Activer l'émission avant de jouer
                var emission = emissionModules[i];
                emission.enabled = true;
                
                if (useStaggeredActivation)
                {
                    StartCoroutine(DelayedPlay(speedEffects[i], i * 0.1f));
                }
                else
                {
                    speedEffects[i].Play();
                }
            }
        }
    }

    private void StopAllEffects()
    {
        for (int i = 0; i < speedEffects.Count; i++)
        {
            if (speedEffects[i] != null && i < emissionModules.Count)
            {
                // Désactiver l'émission et arrêter complètement
                var emission = emissionModules[i];
                emission.enabled = false;
                speedEffects[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void UpdateEffectsIntensity()
    {
        for (int i = 0; i < speedEffects.Count; i++)
        {
            if (speedEffects[i] != null && i < emissionModules.Count && i < mainModules.Count)
            {
                // Contrôler le taux d'émission
                if (controlEmissionRate)
                {
                    var emission = emissionModules[i];
                    float newEmissionRate = originalEmissionRates[i] * currentIntensity;
                    var rate = emission.rateOverTime;
                    rate.constant = newEmissionRate;
                    emission.rateOverTime = rate;
                }

                // Contrôler la transparence
                if (controlAlpha)
                {
                    var main = mainModules[i];
                    Color newColor = originalColors[i];
                    newColor.a = originalColors[i].a * currentIntensity;
                    main.startColor = newColor;
                }
            }
        }
    }

    private IEnumerator DelayedPlay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ps != null)
        {
            ps.Play();
        }
    }

    // Méthodes publiques
    public void AddParticleEffect(ParticleSystem newEffect)
    {
        if (newEffect != null && !speedEffects.Contains(newEffect))
        {
            // Arrêter et vider le système avant de l'ajouter
            newEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            speedEffects.Add(newEffect);
            
            // Ajouter aux listes de modules
            var emission = newEffect.emission;
            var main = newEffect.main;
            
            emission.enabled = false;
            
            emissionModules.Add(emission);
            mainModules.Add(main);
            originalEmissionRates.Add(emission.rateOverTime.constant);
            originalColors.Add(main.startColor.color);
        }
    }

    public void RemoveParticleEffect(ParticleSystem effectToRemove)
    {
        int index = speedEffects.IndexOf(effectToRemove);
        if (index >= 0)
        {
            speedEffects.RemoveAt(index);
            emissionModules.RemoveAt(index);
            mainModules.RemoveAt(index);
            originalEmissionRates.RemoveAt(index);
            originalColors.RemoveAt(index);
        }
    }

    public void SetActivationSpeed(float speed) => activationSpeed = speed;
    public void SetMaxSpeed(float speed) => maxSpeed = speed;
    public float GetCurrentIntensity() => currentIntensity;
    public bool AreEffectsActive() => areEffectsActive;
    
    public void ForcePlayAllEffects() => PlayAllEffects();
    public void ForceStopAllEffects() => StopAllEffects();
}
