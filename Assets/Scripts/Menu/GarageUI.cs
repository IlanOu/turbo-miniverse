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
        [SerializeField] private Button selectButton;
        [SerializeField] private TextMeshProUGUI selectButtonText;
        [SerializeField] private Image carPreviewImage;
        
        [Header("Car Info")]
        [SerializeField] private List<CarData> carsData = new List<CarData>();
        
        private int currentCarIndex = 0;
        private int selectedCarIndex = 0;
        private MoneyManager moneyManager;
        
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
            public bool owned = false; // Indique si la voiture est possédée
        }
        
        private void Start()
        {
            moneyManager = MoneyManager.Instance;
            
            // Configurer les boutons
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousCar);
                
            if (nextButton != null)
                nextButton.onClick.AddListener(NextCar);
                
            if (selectButton != null)
                selectButton.onClick.AddListener(SelectOrBuyCar);
            
            // La première voiture est possédée par défaut
            if (carsData.Count > 0)
                carsData[0].owned = true;
            
            // Initialiser avec la première voiture
            UpdateCarPreview(currentCarIndex);
            
            CloseGarage();
        }
        
        public void NextCar()
        {
            currentCarIndex++;
            if (currentCarIndex >= carsData.Count)
                currentCarIndex = 0;
                
            UpdateCarPreview(currentCarIndex);
        }
        
        public void PreviousCar()
        {
            currentCarIndex--;
            if (currentCarIndex < 0)
                currentCarIndex = carsData.Count - 1;
                
            UpdateCarPreview(currentCarIndex);
        }
        
        private void UpdateCarPreview(int index)
        {
            if (index >= 0 && index < carsData.Count)
            {
                CarData data = carsData[index];
                
                // Mettre à jour l'image de prévisualisation
                if (carPreviewImage != null && data.previewImage != null)
                {
                    carPreviewImage.sprite = data.previewImage;
                }
                
                // Mettre à jour les textes
                if (carNameText != null)
                    carNameText.text = data.carName;
                
                if (carStatsText != null)
                {
                    carStatsText.text = $"<b>Vitesse:</b> {GenerateStars(data.speed)}\n" +
                                        $"<b>Accélération:</b> {GenerateStars(data.acceleration)}\n" +
                                        $"<b>Maniabilité:</b> {GenerateStars(data.handling)}\n\n" +
                                        $"{data.description}";
                }
                
                // Mettre à jour le bouton de sélection/achat
                if (selectButton != null && selectButtonText != null)
                {
                    if (data.owned)
                    {
                        bool isSelected = (index == selectedCarIndex);
                        selectButtonText.text = isSelected ? "Sélectionnée" : "Sélectionner";
                        selectButton.interactable = !isSelected;
                    }
                    else
                    {
                        selectButtonText.text = $"Acheter ({data.price})";
                        selectButton.interactable = moneyManager != null && moneyManager.GetMoney() >= data.price;
                    }
                }
            }
        }
        
        public void SelectOrBuyCar()
        {
            if (currentCarIndex < carsData.Count)
            {
                CarData data = carsData[currentCarIndex];
                
                if (data.owned)
                {
                    // Voiture déjà possédée, la sélectionner
                    selectedCarIndex = currentCarIndex;
                    UpdateCarPreview(currentCarIndex); // Mettre à jour l'UI
                    
                    // Activer la voiture sélectionnée
                    carSelector.ChangeCar(selectedCarIndex);
                    
                    // Fermer l'interface du garage
                    CloseGarage();
                    
                    Debug.Log($"Voiture {data.carName} sélectionnée et activée");
                }
                else
                {
                    // Tenter d'acheter la voiture
                    if (moneyManager != null && moneyManager.SpendMoney(data.price))
                    {
                        // Achat réussi
                        data.owned = true;
                        carsData[currentCarIndex] = data;
                        UpdateCarPreview(currentCarIndex);
                        
                        Debug.Log($"Voiture {data.carName} achetée pour {data.price} pièces");
                    }
                    else
                    {
                        // Pas assez d'argent
                        Debug.Log("Pas assez d'argent pour acheter cette voiture");
                    }
                }
            }
        }
        
        private string GenerateStars(int value)
        {
            string stars = "";
            for (int i = 0; i < 10; i++)
            {
                if (i < value)
                    stars += "■";
                else
                    stars += "□";
            }
            return stars;
        }
        
        public void OpenGarage()
        {
            gameObject.SetActive(true);
            UpdateCarPreview(currentCarIndex);
        }
        
        public void CloseGarage()
        {
            gameObject.SetActive(false);
        }
    }
}
