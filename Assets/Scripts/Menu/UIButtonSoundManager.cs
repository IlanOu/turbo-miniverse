using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIButtonSoundManager : MonoBehaviour
{
    [Header("Sons")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    
    [Header("Configuration")]
    [SerializeField] private float clickVolume = 0.7f;
    [SerializeField] private float hoverVolume = 0.5f;
    [SerializeField] private float minTimeBetweenHoverSounds = 0.1f;
    [SerializeField] private bool playHoverSound = true;
    
    [Header("Options avancées")]
    [SerializeField] private bool useButtonSpecificSounds = false;
    [SerializeField] private string buttonSoundTag = "ButtonSound";
    
    private AudioSource audioSource;
    private GameObject lastHoveredButton;
    private float lastHoverSoundTime;
    private EventSystem eventSystem;
    
    private void Awake()
    {
        // Créer un AudioSource si nécessaire
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Son 2D
        }
        
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("UIButtonSoundManager: No EventSystem found in the scene!");
        }
        
        // S'abonner aux événements de clic
        AddClickListenersToAllButtons();
    }
    
    private void Update()
    {
        // Gérer les sons de survol
        if (playHoverSound && hoverSound != null && eventSystem != null)
        {
            // Vérifier si la souris survole un bouton
            GameObject currentButton = GetButtonUnderPointer();
            
            if (currentButton != null && currentButton != lastHoveredButton)
            {
                // Nouveau bouton survolé
                lastHoveredButton = currentButton;
                
                // Vérifier le délai minimum entre les sons de survol
                if (Time.unscaledTime - lastHoverSoundTime >= minTimeBetweenHoverSounds)
                {
                    PlayHoverSound(currentButton);
                    lastHoverSoundTime = Time.unscaledTime;
                }
            }
            else if (currentButton == null)
            {
                // Plus aucun bouton survolé
                lastHoveredButton = null;
            }
        }
    }
    
    private GameObject GetButtonUnderPointer()
    {
        // Créer un rayon depuis la position de la souris
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;
        
        // Effectuer le raycast
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        // Vérifier si un bouton est touché
        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                return result.gameObject;
            }
        }
        
        return null;
    }
    
    private void AddClickListenersToAllButtons()
    {
        // Trouver tous les boutons dans la scène
        Button[] allButtons = FindObjectsOfType<Button>(true);
        
        foreach (Button button in allButtons)
        {
            // Ajouter un écouteur de clic à chaque bouton
            button.onClick.AddListener(() => PlayClickSound(button.gameObject));
        }
        
        Debug.Log($"UIButtonSoundManager: Added click listeners to {allButtons.Length} buttons");
    }
    
    // Méthode pour ajouter un écouteur à un nouveau bouton (utile pour les boutons créés dynamiquement)
    public void AddClickListener(Button button)
    {
        if (button != null)
        {
            button.onClick.AddListener(() => PlayClickSound(button.gameObject));
        }
    }
    
    private void PlayClickSound(GameObject buttonObj)
    {
        if (audioSource == null) return;
        
        AudioClip soundToPlay = clickSound;
        float volume = clickVolume;
        
        // Vérifier si le bouton a un son spécifique
        if (useButtonSpecificSounds)
        {
            ButtonSound buttonSound = buttonObj.GetComponent<ButtonSound>();
            if (buttonSound != null && buttonSound.clickSound != null)
            {
                soundToPlay = buttonSound.clickSound;
                volume = buttonSound.volume;
            }
        }
        
        // Jouer le son
        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay, volume);
        }
    }
    
    private void PlayHoverSound(GameObject buttonObj)
    {
        if (audioSource == null) return;
        
        AudioClip soundToPlay = hoverSound;
        float volume = hoverVolume;
        
        // Vérifier si le bouton a un son spécifique
        if (useButtonSpecificSounds)
        {
            ButtonSound buttonSound = buttonObj.GetComponent<ButtonSound>();
            if (buttonSound != null && buttonSound.hoverSound != null)
            {
                soundToPlay = buttonSound.hoverSound;
                volume = buttonSound.hoverVolume;
            }
        }
        
        // Jouer le son
        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay, volume);
        }
    }
}

// Classe optionnelle pour définir des sons spécifiques par bouton
public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    public AudioClip hoverSound;
    public float volume = 1.0f;
    public float hoverVolume = 0.5f;
}
