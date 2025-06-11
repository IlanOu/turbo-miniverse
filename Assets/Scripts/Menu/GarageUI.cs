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
        [SerializeField] private Transform carSpawnPoint;
        
        [Header("UI Elements")]
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI carNameText;
        [SerializeField] private TextMeshProUGUI carStatsText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;
        [SerializeField] private Image carPreviewImage;
        
        [Header("Car Info")]
        [SerializeField] private List<CarData> carsData = new List<CarData>();
        
        private int currentCarIndex = 0;
        private int selectedCarIndex = 0;
        private MoneyManager moneyManager;
        private bool isInitialized = false;
        
        [System.Serializable]
        public class CarData
        {
            public string carName = "Voiture";
            public string description = "Description de la voiture";
            [Range(1, 10)] public int speed = 5;
            [Range(1, 10)] public int acceleration = 5;
            [Range(1, 10)] public int handling = 5;
            public Sprite previewImage;
            public int price = 0;
            public bool owned = false;
        }
        
        private void Awake()
        {
            // S'assurer que l'UI est désactivée au démarrage
            gameObject.SetActive(false);
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            if (isInitialized) return;
            
            moneyManager = MoneyManager.Instance;
            
            // Configuration des boutons
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousCar);
                
            if (nextButton != null)
                nextButton.onClick.AddListener(NextCar);
                
            if (actionButton != null)
                actionButton.onClick.AddListener(HandleActionButtonClick);
            
            // La première voiture est possédée par défaut
            if (carsData.Count > 0)
                carsData[0].owned = true;
            
            // Initialiser avec la première voiture
            selectedCarIndex = 0;
            
            isInitialized = true;
        }
        
        public void NextCar()
        {
            currentCarIndex++;
            if (currentCarIndex >= carsData.Count)
                currentCarIndex = 0;
                
            UpdateCarDisplay();
        }
        
        public void PreviousCar()
        {
            currentCarIndex--;
            if (currentCarIndex < 0)
                currentCarIndex = carsData.Count - 1;
                
            UpdateCarDisplay();
        }
        
        private void UpdateCarDisplay()
        {
            if (currentCarIndex < 0 || currentCarIndex >= carsData.Count)
                return;
                
            CarData data = carsData[currentCarIndex];
            
            // Mise à jour de l'image
            if (carPreviewImage != null && data.previewImage != null)
            {
                carPreviewImage.sprite = data.previewImage;
            }
            
            // Mise à jour des textes
            if (carNameText != null)
                carNameText.text = data.carName;
            
            if (carStatsText != null)
            {
                carStatsText.text = $"<b>Vitesse:</b> {GenerateStars(data.speed)}\n" +
                                    $"<b>Accélération:</b> {GenerateStars(data.acceleration)}\n" +
                                    $"<b>Maniabilité:</b> {GenerateStars(data.handling)}\n\n" +
                                    $"{data.description}";
            }
            
            // Mise à jour du bouton d'action
            UpdateActionButton();
        }
        
        private void UpdateActionButton()
        {
            if (actionButton == null || actionButtonText == null)
                return;
            
            CarData data = carsData[currentCarIndex];
            
            if (!data.owned)
            {
                // Voiture non possédée - afficher le bouton d'achat
                actionButtonText.text = $"Acheter ({data.price})";
                actionButton.interactable = moneyManager != null && moneyManager.GetMoney() >= data.price;
            }
            else
            {
                // Voiture possédée - afficher le bouton de sélection
                bool isSelected = (currentCarIndex == selectedCarIndex);
                actionButtonText.text = isSelected ? "Sélectionnée" : "Sélectionner";
                actionButton.interactable = !isSelected;
            }
        }
        
        private void HandleActionButtonClick()
        {
            if (currentCarIndex >= carsData.Count) return;
            
            CarData data = carsData[currentCarIndex];
            
            if (!data.owned)
            {
                // Tenter d'acheter la voiture
                BuyCurrentCar();
            }
            else
            {
                // Sélectionner la voiture
                SelectCurrentCar();
            }
        }
        
        private void BuyCurrentCar()
        {
            CarData data = carsData[currentCarIndex];
            
            if (moneyManager == null || moneyManager.GetMoney() < data.price)
            {
                Debug.Log("Pas assez d'argent pour acheter cette voiture");
                return;
            }
            
            // Effectuer l'achat
            moneyManager.SpendMoney(data.price);
            data.owned = true;
            carsData[currentCarIndex] = data;
            
            Debug.Log($"Voiture {data.carName} achetée pour {data.price} pièces");
            
            // Mettre à jour l'interface après l'achat
            // IMPORTANT: Ne pas fermer l'UI ici
            UpdateActionButton();
        }
        
        private void SelectCurrentCar()
        {
            // Mettre à jour l'index de la voiture sélectionnée
            selectedCarIndex = currentCarIndex;
            
            // Activer la voiture via ChooseCar
            if (carSelector != null)
            {
                carSelector.ChangeCar(selectedCarIndex);
                
                // Positionner la voiture au point de spawn
                PositionSelectedCar();
            }
            
            Debug.Log($"Voiture {carsData[currentCarIndex].carName} sélectionnée et activée");
            
            // Fermer l'interface UNIQUEMENT lors de la sélection
            CloseGarage();
        }
        
        private void PositionSelectedCar()
        {
            if (carSpawnPoint == null || carSelector == null || 
                selectedCarIndex >= carSelector.cars.Count)
                return;
                
            GameObject selectedCar = carSelector.cars[selectedCarIndex];
            if (selectedCar == null)
                return;
                
            // Positionner tous les enfants au même endroit que le spawnpoint
            for (int i = 0; i < selectedCar.transform.childCount; i++)
            {
                Transform child = selectedCar.transform.GetChild(i);
                child.position = carSpawnPoint.position;
                child.rotation = carSpawnPoint.rotation;
            }
            
            // Réactiver les composants de gameplay
            EnableGameplayComponents(selectedCar);
        }
        
        private void DisableGameplayComponents(GameObject car)
        {
            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
        private void EnableGameplayComponents(GameObject car)
        {
            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
        
        private string GenerateStars(int value)
        {
            string stars = "";
            for (int i = 0; i < 10; i++)
            {
                stars += (i < value) ? "■" : "□";
            }
            return stars;
        }
        
        public void OpenGarage()
        {
            // S'assurer que l'initialisation est faite
            Initialize();
            
            // Activer l'UI
            gameObject.SetActive(true);
            
            // Afficher la voiture actuellement sélectionnée
            currentCarIndex = selectedCarIndex;
            UpdateCarDisplay();
            
            Debug.Log("Garage UI ouverte");
        }
        
        public void CloseGarage()
        {
            gameObject.SetActive(false);
            Debug.Log("Garage UI fermée");
        }
    }
}
