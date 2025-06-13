using System.Collections.Generic;
using Car;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;

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
        [SerializeField] private RectTransform carInfoPanel;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float openAnimDuration = 0.5f;
        [SerializeField] private float switchCarAnimDuration = 0.3f;
        [SerializeField] private float buttonClickAnimDuration = 0.2f;
        [SerializeField] private float buyAnimDuration = 0.5f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease switchEase = Ease.InOutQuad;
        
        [Header("Preview Settings")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gris à 50%
        
        [Header("Car Info")]
        [SerializeField] private List<CarData> carsData = new List<CarData>();
        
        private int currentCarIndex = 0;
        private int selectedCarIndex = 0;
        private MoneyManager moneyManager;
        private bool isAnimating = false;
        
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
            
            // Configuration des boutons avec animations
            if (previousButton != null)
            {
                previousButton.onClick.AddListener(() => {
                    AnimateButtonClick(previousButton.transform);
                    PreviousCar();
                });
            }
                
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() => {
                    AnimateButtonClick(nextButton.transform);
                    NextCar();
                });
            }
                
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(() => {
                    AnimateButtonClick(actionButton.transform);
                    OnActionButtonClick();
                });
            }
            
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
            
            // Initialiser DOTween si nécessaire
            DOTween.Init();
        }
        
        private void AnimateButtonClick(Transform buttonTransform)
        {
            // Séquence d'animation pour le clic de bouton
            Sequence clickSequence = DOTween.Sequence();
            clickSequence.Append(buttonTransform.DOScale(0.9f, buttonClickAnimDuration / 2));
            clickSequence.Append(buttonTransform.DOScale(1f, buttonClickAnimDuration / 2));
        }
        
        public void NextCar()
        {
            if (isAnimating) return;
            
            currentCarIndex++;
            if (currentCarIndex >= carsData.Count)
                currentCarIndex = 0;
                
            AnimateCarSwitch(true);
        }
        
        public void PreviousCar()
        {
            if (isAnimating) return;
            
            currentCarIndex--;
            if (currentCarIndex < 0)
                currentCarIndex = carsData.Count - 1;
                
            AnimateCarSwitch(false);
        }
        
        private void AnimateCarSwitch(bool isNext)
        {
            isAnimating = true;
            
            // Animer la sortie des éléments actuels
            Sequence exitSequence = DOTween.Sequence();
            
            // Faire sortir l'image par le côté
            float xOffset = isNext ? -100f : 100f;
            exitSequence.Join(carPreviewImage.transform.DOLocalMoveX(xOffset, switchCarAnimDuration / 2).SetEase(switchEase));
            exitSequence.Join(carPreviewImage.DOFade(0, switchCarAnimDuration / 2));
            
            // Faire sortir les textes par le bas
            if (carNameText != null)
                exitSequence.Join(carNameText.DOFade(0, switchCarAnimDuration / 2));
            
            if (carStatsText != null)
                exitSequence.Join(carStatsText.DOFade(0, switchCarAnimDuration / 2));
            
            // Après la sortie, mettre à jour les données et animer l'entrée
            exitSequence.OnComplete(() => {
                UpdateCarDisplay();
                
                // Animer l'entrée des nouveaux éléments
                Sequence enterSequence = DOTween.Sequence();
                
                // Positionner l'image de l'autre côté avant l'animation
                carPreviewImage.transform.localPosition = new Vector3(-xOffset, carPreviewImage.transform.localPosition.y, 0);
                carPreviewImage.color = new Color(carPreviewImage.color.r, carPreviewImage.color.g, carPreviewImage.color.b, 0);
                
                // Faire entrer l'image
                enterSequence.Join(carPreviewImage.transform.DOLocalMoveX(0, switchCarAnimDuration / 2).SetEase(switchEase));
                enterSequence.Join(carPreviewImage.DOFade(1, switchCarAnimDuration / 2));
                
                // Faire entrer les textes
                if (carNameText != null)
                    enterSequence.Join(carNameText.DOFade(1, switchCarAnimDuration / 2));
                
                if (carStatsText != null)
                    enterSequence.Join(carStatsText.DOFade(1, switchCarAnimDuration / 2));
                
                enterSequence.OnComplete(() => {
                    isAnimating = false;
                });
            });
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
                    Color targetColor = data.owned ? unlockedColor : lockedColor;
                    carPreviewImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, carPreviewImage.color.a);
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
            if (isAnimating) return;
            
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
                // Animation de refus d'achat
                if (actionButton != null)
                {
                    actionButton.transform.DOShakePosition(0.5f, 10, 20, 90, false, true);
                }
                
                Debug.Log("Pas assez d'argent pour acheter cette voiture");
                return;
            }
            
            // Effectuer l'achat
            moneyManager.SpendMoney(data.price);
            data.owned = true;
            carsData[currentCarIndex] = data;
            
            Debug.Log($"Voiture {data.carName} achetée pour {data.price} pièces");
            
            // Animation d'achat réussi
            AnimatePurchase();
        }
        
        private void AnimatePurchase()
        {
            isAnimating = true;
            
            // Séquence d'animation pour l'achat
            Sequence purchaseSequence = DOTween.Sequence();
            
            // Faire tourner l'image
            if (carPreviewImage != null)
            {
                purchaseSequence.Append(carPreviewImage.transform.DOScale(1.2f, buyAnimDuration / 2).SetEase(Ease.OutBack));
                purchaseSequence.Join(carPreviewImage.transform.DORotate(new Vector3(0, 360, 0), buyAnimDuration, RotateMode.FastBeyond360));
                purchaseSequence.Append(carPreviewImage.transform.DOScale(1f, buyAnimDuration / 2).SetEase(Ease.OutBack));
                
                // Changer la couleur de grisé à normal
                purchaseSequence.Join(carPreviewImage.DOColor(unlockedColor, buyAnimDuration / 2));
            }
            
            // Mettre à jour l'interface après l'animation
            purchaseSequence.OnComplete(() => {
                UpdateActionButton();
                isAnimating = false;
            });
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
            
            // Animer la fermeture de l'UI
            AnimateClose();
        }
        
        private void AnimateClose()
        {
            isAnimating = true;
            
            // Séquence d'animation pour la fermeture
            Sequence closeSequence = DOTween.Sequence();
            
            // Faire disparaître progressivement tous les éléments
            if (mainCanvasGroup != null)
            {
                closeSequence.Append(mainCanvasGroup.DOFade(0, openAnimDuration).SetEase(Ease.InBack));
            }
            else
            {
                // Animer chaque élément individuellement si pas de CanvasGroup
                if (carPreviewImage != null)
                    closeSequence.Join(carPreviewImage.DOFade(0, openAnimDuration));
                
                if (carNameText != null)
                    closeSequence.Join(carNameText.DOFade(0, openAnimDuration));
                
                if (carStatsText != null)
                    closeSequence.Join(carStatsText.DOFade(0, openAnimDuration));
                
                if (actionButton != null)
                    closeSequence.Join(actionButton.GetComponent<CanvasGroup>()?.DOFade(0, openAnimDuration));
                
                if (previousButton != null)
                    closeSequence.Join(previousButton.GetComponent<CanvasGroup>()?.DOFade(0, openAnimDuration));
                
                if (nextButton != null)
                    closeSequence.Join(nextButton.GetComponent<CanvasGroup>()?.DOFade(0, openAnimDuration));
                
                if (carInfoPanel != null)
                    closeSequence.Join(carInfoPanel.DOScale(0.8f, openAnimDuration).SetEase(Ease.InBack));
            }
            
            // Désactiver l'UI après l'animation
            closeSequence.OnComplete(() => {
                shouldCloseUI = true;
                gameObject.SetActive(false);
                isAnimating = false;
            });
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
            
            // Animation de la voiture sélectionnée
            selectedCar.transform.DOScale(0, 0.01f).OnComplete(() => {
                selectedCar.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
            });
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
            
            // Animer l'ouverture
            AnimateOpen();
        }
        
        private void AnimateOpen()
        {
            isAnimating = true;
            
            // Préparer les éléments pour l'animation
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 0;
            }
            else
            {
                // Préparer chaque élément individuellement
                if (carPreviewImage != null)
                {
                    carPreviewImage.color = new Color(carPreviewImage.color.r, carPreviewImage.color.g, carPreviewImage.color.b, 0);
                    carPreviewImage.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
                
                if (carNameText != null)
                    carNameText.alpha = 0;
                
                if (carStatsText != null)
                    carStatsText.alpha = 0;
                
                if (actionButton != null && actionButton.GetComponent<CanvasGroup>() != null)
                    actionButton.GetComponent<CanvasGroup>().alpha = 0;
                
                if (previousButton != null && previousButton.GetComponent<CanvasGroup>() != null)
                    previousButton.GetComponent<CanvasGroup>().alpha = 0;
                
                if (nextButton != null && nextButton.GetComponent<CanvasGroup>() != null)
                    nextButton.GetComponent<CanvasGroup>().alpha = 0;
                
                if (carInfoPanel != null)
                    carInfoPanel.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            
            // Séquence d'animation pour l'ouverture
            Sequence openSequence = DOTween.Sequence();
            
            // Animer l'apparition progressive
            if (mainCanvasGroup != null)
            {
                openSequence.Append(mainCanvasGroup.DOFade(1, openAnimDuration).SetEase(openEase));
            }
            else
            {
                // Animer chaque élément avec un léger décalage
                if (carInfoPanel != null)
                    openSequence.Join(carInfoPanel.DOScale(1, openAnimDuration).SetEase(openEase));
                
                if (carPreviewImage != null)
                {
                    openSequence.Join(carPreviewImage.DOFade(1, openAnimDuration));
                    openSequence.Join(carPreviewImage.transform.DOScale(1, openAnimDuration).SetEase(openEase));
                }
                
                if (carNameText != null)
                    openSequence.Join(carNameText.DOFade(1, openAnimDuration).SetDelay(0.1f));
                
                if (carStatsText != null)
                    openSequence.Join(carStatsText.DOFade(1, openAnimDuration).SetDelay(0.2f));
                
                if (actionButton != null && actionButton.GetComponent<CanvasGroup>() != null)
                    openSequence.Join(actionButton.GetComponent<CanvasGroup>().DOFade(1, openAnimDuration).SetDelay(0.3f));
                
                if (previousButton != null && previousButton.GetComponent<CanvasGroup>() != null)
                    openSequence.Join(previousButton.GetComponent<CanvasGroup>().DOFade(1, openAnimDuration).SetDelay(0.3f));
                
                if (nextButton != null && nextButton.GetComponent<CanvasGroup>() != null)
                    openSequence.Join(nextButton.GetComponent<CanvasGroup>().DOFade(1, openAnimDuration).SetDelay(0.3f));
            }
            
            // Terminer l'animation
            openSequence.OnComplete(() => {
                isAnimating = false;
            });
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
        
        // Méthode pour ajouter des effets de survol aux boutons
        private void OnHoverEffects()
        {
            // Ajouter des écouteurs d'événements pour les effets de survol
            if (previousButton != null)
            {
                AddHoverEffect(previousButton);
            }
            
            if (nextButton != null)
            {
                AddHoverEffect(nextButton);
            }
            
            if (actionButton != null)
            {
                AddHoverEffect(actionButton);
            }
        }
        
        private void AddHoverEffect(Button button)
        {
            // Ajouter des écouteurs d'événements pour le survol
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();
            
            // Entrée du survol
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => {
                button.transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad);
            });
            trigger.triggers.Add(entryEnter);
            
            // Sortie du survol
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => {
                button.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
            });
            trigger.triggers.Add(entryExit);
        }
        
        // Méthode pour nettoyer les tweens à la destruction
        private void OnDestroy()
        {
            DOTween.Kill(carPreviewImage);
            DOTween.Kill(carNameText);
            DOTween.Kill(carStatsText);
            DOTween.Kill(actionButton);
            DOTween.Kill(previousButton);
            DOTween.Kill(nextButton);
            DOTween.Kill(carInfoPanel);
            DOTween.Kill(mainCanvasGroup);
        }
    }
}
