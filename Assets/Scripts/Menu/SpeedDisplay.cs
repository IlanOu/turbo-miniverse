using TMPro;
using UnityEngine;

namespace Menu
{
    public class SpeedDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private string speedUnit = "km/h";
        [SerializeField] private bool roundSpeed = true;
        [SerializeField] private string speedFormat = "0"; // Format de nombre, ex: "0.0" pour 1 décimale
        
        private Rigidbody targetRigidbody;
        private float currentSpeed = 0f;
        
        public void Initialize(Rigidbody vehicleRigidbody)
        {
            targetRigidbody = vehicleRigidbody;
            if (speedText != null)
                speedText.text = "0 " + speedUnit;
        }
        
        // Méthode publique pour mettre à jour manuellement la vitesse
        public void UpdateSpeed(float speed)
        {
            currentSpeed = speed;
            
            if (speedText == null) return;
            
            string speedDisplay = roundSpeed ? 
                Mathf.Round(currentSpeed).ToString(speedFormat) : 
                currentSpeed.ToString(speedFormat);
                
            speedText.text = speedDisplay + " " + speedUnit;
        }
    }
}