using System;
using Car;
using Menu;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

namespace Builds
{
    public class GarageStation: MonoBehaviour
    {
        [SerializeField] private GarageUI garageUI;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        [Header("Prompt UI")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Image keyImage;
        
        [Header("Animation")]
        [SerializeField] private float pulseDuration = 0.5f;
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private Ease pulseEaseType = Ease.InOutSine;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        
        private bool playerInTrigger = false;
        private bool garageOpen = false;
        private Sequence pulseSequence;

        private void Start()
        {
            if (garageUI == null)
            {
                Debug.LogError("GarageUI component not found on the referenced GameObject");
            }
            
            // S'assurer que le prompt est caché au démarrage
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            // Nettoyer les animations DOTween
            if (pulseSequence != null && pulseSequence.IsActive())
            {
                pulseSequence.Kill();
            }
        }
        
        private void Update()
        {
            // Vérifier si le joueur est dans le trigger et appuie sur la touche d'interaction
            if (playerInTrigger && Input.GetKeyDown(interactionKey))
            {
                if (!garageOpen)
                {
                    OpenGarageUI();
                }
                else
                {
                    CloseGarageUI();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("Player entered garage");
            
            // Afficher le prompt d'interaction
            playerInTrigger = true;
            ShowPrompt();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            // Cacher le prompt et fermer l'UI si elle est ouverte
            playerInTrigger = false;
            HidePrompt();
            CloseGarageUI();
        }
        
        private void ShowPrompt()
        {
            if (promptUI != null)
            {
                // Activer le GameObject
                promptUI.SetActive(true);
                
                // Mettre à jour le texte du prompt si nécessaire
                if (promptText != null)
                {
                    promptText.text = $"E";
                }
                
                // Animer l'apparition avec DOTween
                if (promptUI.GetComponent<CanvasGroup>() == null)
                {
                    promptUI.AddComponent<CanvasGroup>();
                }
                
                CanvasGroup canvasGroup = promptUI.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration);
                
                // Animer l'image de la touche avec un effet de pulsation
                if (keyImage != null)
                {
                    // Réinitialiser l'échelle
                    keyImage.transform.localScale = Vector3.one;
                    
                    // Créer une séquence de pulsation
                    pulseSequence = DOTween.Sequence();
                    
                    // Ajouter les animations de pulsation
                    pulseSequence.Append(keyImage.transform.DOScale(pulseScale, pulseDuration / 2).SetEase(pulseEaseType))
                                .Append(keyImage.transform.DOScale(1f, pulseDuration / 2).SetEase(pulseEaseType))
                                .SetLoops(-1); // Répéter indéfiniment
                }
            }
        }
        
        private void HidePrompt()
        {
            if (promptUI != null)
            {
                // Arrêter l'animation de pulsation
                if (pulseSequence != null && pulseSequence.IsActive())
                {
                    pulseSequence.Kill();
                }
                
                // Animer la disparition avec DOTween
                CanvasGroup canvasGroup = promptUI.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() => {
                        promptUI.SetActive(false);
                    });
                }
                else
                {
                    promptUI.SetActive(false);
                }
            }
        }

        private void OpenGarageUI()
        {
            if (garageUI != null)
            {
                garageUI.OpenGarage();
                garageOpen = true;
                
                // Cacher le prompt quand l'UI est ouverte
                HidePrompt();
            }
        }
        
        private void CloseGarageUI()
        {
            if (garageUI != null && garageOpen)
            {
                // Fermer l'UI
                garageUI.gameObject.SetActive(false);
                garageOpen = false;
                
                // Réafficher le prompt si le joueur est toujours dans le trigger
                if (playerInTrigger)
                {
                    ShowPrompt();
                }
            }
        }
    }
}
