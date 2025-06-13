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
        
        [Header("Preview Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gris à 50%
        
        [Header("Car Info")]
        [SerializeField] private List<CarData> carsData = new List<CarData>();
        
        private int currentCarIndex = 0;
        private int selectedCarIndex = 0;
        private MoneyManager moneyManager;
        
        // Variable pour contrôler si l'UI doit se fermer automatiquement
        private bool shouldCloseUI = false;
        
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
        
        private void Start()
        {
            moneyManager = MoneyManager.Instance;
            
            // Configuration des boutons
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousCar);
                
            if (nextButton != null)
                nextButton.onClick.AddListener(NextCar);
                
            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionButtonClick);
            
            // La première voiture est possédée par défaut
            if (carsData.Count > 0)
                carsData[0].owned = true;
            
            // Initialiser avec la première voiture
            selectedCarIndex = 0;
            
            // Configuration de base de l'image
            if (carPreviewImage != null)
            {
                // Préserver le ratio d'aspect
                carPreviewImage.preserveAspect = true;
            }
            
            // Désactiver l'UI au démarrage
            gameObject.SetActive(false);
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
            if (carPreviewImage != null)
            {
                if (data.previewImage != null)
                {
                    // Assigner le sprite
                    carPreviewImage.sprite = data.previewImage;
                    carPreviewImage.enabled = true;
                    
                    // Appliquer la couleur en fonction du statut de déverrouillage
                    carPreviewImage.color = data.owned ? unlockedColor : lockedColor;
                }
                else
                {
                    // Aucune image disponible
                    carPreviewImage.sprite = null;
                    carPreviewImage.enabled = false;
                }
            }
            
            // Mise à jour des textes
            if (carNameText != null)
                carNameText.text = data.carName;
            
            if (carStatsText != null)
            {
                carStatsText.text = $"<b>Vitesse:</b>\n {GenerateStars(data.speed)}\n" +
                                    $"<b>Accélération:</b>\n {GenerateStars(data.acceleration)}\n" +
                                    $"<b>Maniabilité:</b>\n {GenerateStars(data.handling)}\n\n" +
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
            
            if (data.owned)
            {
                bool isSelected = (currentCarIndex == selectedCarIndex);
                actionButtonText.text = isSelected ? "Sélectionnée" : "Sélectionner";
                actionButton.interactable = !isSelected;
            }
            else
            {
                actionButtonText.text = $"Acheter ({data.price})";
                actionButton.interactable = moneyManager != null && moneyManager.GetMoney() >= data.price;
            }
        }
        
        private void OnActionButtonClick()
        {
            CarData data = carsData[currentCarIndex];
            
            if (data.owned)
            {
                // Voiture déjà possédée, la sélectionner
                SelectCar();
            }
            else
            {
                // Tenter d'acheter la voiture
                BuyCar();
            }
        }
        
        private void BuyCar()
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
            UpdateCarDisplay(); // Mettre à jour l'affichage complet pour changer la couleur
        }
        
        private void SelectCar()
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
            
            // IMPORTANT: Fermer l'UI UNIQUEMENT lors de la sélection
            shouldCloseUI = true;
            gameObject.SetActive(false);
        }
        
        private void PositionSelectedCar()
        {
            if (carSpawnPoint == null || carSelector == null || 
                selectedCarIndex >= carSelector.cars.Count)
                return;
                
            GameObject selectedCar = carSelector.cars[selectedCarIndex];
            if (selectedCar == null)
                return;
            
            selectedCar.transform.position = carSpawnPoint.position;
            selectedCar.transform.rotation = carSpawnPoint.rotation;
            
            // Positionner tous les enfants au même endroit que le spawnpoint
            for (int i = 0; i < selectedCar.transform.childCount; i++)
            {
                Transform child = selectedCar.transform.GetChild(i);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
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
            // Réinitialiser le flag
            shouldCloseUI = false;
            
            // Activer l'UI
            gameObject.SetActive(true);
            
            // Afficher la voiture actuellement sélectionnée
            currentCarIndex = selectedCarIndex;
            UpdateCarDisplay();
        }
        
        // Méthode pour vérifier si les sprites sont correctement chargés
        private void OnEnable()
        {
            // Vérifier les sprites au moment où l'UI est activée
            foreach (CarData car in carsData)
            {
                if (car.previewImage == null)
                {
                    Debug.LogWarning($"Attention: L'image de preview pour la voiture '{car.carName}' est manquante!");
                }
            }
            
            // Forcer la mise à jour de l'affichage
            UpdateCarDisplay();
        }
    }
}
