using System.Collections.Generic;
using Car;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class GarageUI : MonoBehaviour
    {
        [Header("Car Selection")]
        [SerializeField] private ChooseCar carSelector;
        
        [Header("UI Elements")]
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI carNameText;
        [SerializeField] private TextMeshProUGUI carStatsText;
        
        [Header("Car Info")]
        [SerializeField] private List<CarInfo> carsInfo = new List<CarInfo>();
        
        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 30f; // Vitesse de rotation du modèle
        [SerializeField] private Transform displayPlatform; // Plateforme qui tourne
        
        private int currentCarIndex = 0;
        
        [System.Serializable]
        public class CarInfo
        {
            public string carName = "Voiture";
            public string description = "Description de la voiture";
            [Range(1, 10)] public int speed = 5;
            [Range(1, 10)] public int acceleration = 5;
            [Range(1, 10)] public int handling = 5;
            public Color carColor = Color.white;
        }
        
        private void Start()
        {
            gameObject.SetActive(false);
            // Configurer les boutons
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousCar);
                
            if (nextButton != null)
                nextButton.onClick.AddListener(NextCar);
            
            // Initialiser avec la première voiture
            UpdateUI();
        }
        
        private void Update()
        {
            // Faire tourner la plateforme d'affichage
            if (displayPlatform != null)
            {
                displayPlatform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            // Navigation avec les touches du clavier (optionnel)
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                PreviousCar();
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                NextCar();
        }
        
        public void NextCar()
        {
            currentCarIndex++;
            if (currentCarIndex >= carSelector.cars.Count)
                currentCarIndex = 0;
                
            carSelector.ChangeCar(currentCarIndex);
            UpdateUI();
        }
        
        public void PreviousCar()
        {
            currentCarIndex--;
            if (currentCarIndex < 0)
                currentCarIndex = carSelector.cars.Count - 1;
                
            carSelector.ChangeCar(currentCarIndex);
            UpdateUI();
        }
        
        public void SelectCar(int index)
        {
            if (index >= 0 && index < carSelector.cars.Count)
            {
                currentCarIndex = index;
                carSelector.ChangeCar(currentCarIndex);
                UpdateUI();
            }
        }
        
        private void UpdateUI()
        {
            // Mettre à jour le texte avec les informations de la voiture actuelle
            if (currentCarIndex < carsInfo.Count)
            {
                CarInfo info = carsInfo[currentCarIndex];
                
                if (carNameText != null)
                    carNameText.text = info.carName;
                
                if (carStatsText != null)
                {
                    carStatsText.text = $"<b>Vitesse:</b> {GenerateStars(info.speed)}\n" +
                                        $"<b>Accélération:</b> {GenerateStars(info.acceleration)}\n" +
                                        $"<b>Maniabilité:</b> {GenerateStars(info.handling)}\n\n" +
                                        $"{info.description}";
                }
            }
        }
        
        private string GenerateStars(int value)
        {
            string stars = "";
            for (int i = 0; i < 10; i++)
            {
                if (i < value)
                    stars += "■"; // Étoile pleine
                else
                    stars += "□"; // Étoile vide
            }
            return stars;
        }
    }
}
