using System;
using UnityEngine;

namespace Builds
{
    public class GarageStation: MonoBehaviour
    {
        [SerializeField] private GameObject garageUI;

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
            garageUI.SetActive(true);
        }
        
        private void CloseGarageUI()
        {
            garageUI.SetActive(false);
        }
    }
}