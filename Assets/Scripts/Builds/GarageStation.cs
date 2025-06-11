using System;
using Car;
using Menu;
using UnityEngine;

namespace Builds
{
    public class GarageStation: MonoBehaviour
    {
        [SerializeField] private GarageUI garageUI;

        private void Start()
        {
            if (garageUI == null)
            {
                Debug.LogError("GarageUI component not found on the referenced GameObject");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("Player entered garage");
            OpenGarageUI();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            CloseGarageUI();
        }

        private void OpenGarageUI()
        {
            if (garageUI != null)
            {
                garageUI.OpenGarage();
            }
        }
        
        private void CloseGarageUI()
        {
            if (garageUI != null)
            {
                // Fermer directement l'UI quand le joueur quitte la zone
                garageUI.gameObject.SetActive(false);
            }
        }
    }
}