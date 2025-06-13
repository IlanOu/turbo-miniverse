using System;
using Car;
using Menu;
using UnityEngine;
using TMPro;

namespace Builds
{
    public class GarageStation: MonoBehaviour
    {
        [SerializeField] private GarageUI garageUI;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        [Header("Prompt UI")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private TextMeshProUGUI promptText;
        
        private bool playerInTrigger = false;
        private bool garageOpen = false;

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
                promptUI.SetActive(true);
                
                // Mettre à jour le texte du prompt si nécessaire
                if (promptText != null)
                {
                    promptText.text = $"E";
                }
            }
        }
        
        private void HidePrompt()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(false);
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
