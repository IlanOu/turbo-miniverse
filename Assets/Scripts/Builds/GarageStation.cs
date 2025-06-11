using System;
using Car;
using Menu;
using UnityEngine;

namespace Builds
{
    public class GarageStation: MonoBehaviour
    {
        [SerializeField] private GameObject garageUIObject;
        private GarageUI garageUI;

        private void Start()
        {
            // Récupérer le composant GarageUI
            garageUI = garageUIObject.GetComponent<GarageUI>();
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
            // Utiliser la méthode OpenGarage du script GarageUI au lieu de SetActive
            if (garageUI != null)
            {
                garageUI.OpenGarage();
            }
        }
        
        private void CloseGarageUI()
        {
            // Utiliser la méthode CloseGarage du script GarageUI au lieu de SetActive
            if (garageUI != null)
            {
                garageUI.CloseGarage();
            }
        }
    }
}