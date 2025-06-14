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
        [Header("Car Selection")] [SerializeField]
        private ChooseCar carSelector;

        [SerializeField] private Transform carSpawnPoint;

        [Header("UI Elements")] [SerializeField]
        private Button previousButton;

        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI carNameText;
        [SerializeField] private TextMeshProUGUI carStatsText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;
        [SerializeField] private Image carPreviewImage;
        [SerializeField] private RectTransform carInfoPanel;
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private List<Button> allButtons = new List<Button>(); // Liste de tous les boutons à animer

        [Header("Animation Settings")] [SerializeField]
        private float openAnimDuration = 0.5f;

        [SerializeField] private float closeAnimDuration = 0.4f; // Durée spécifique pour la fermeture
        [SerializeField] private float switchCarAnimDuration = 0.3f;
        [SerializeField] private float buttonClickAnimDuration = 0.2f;
        [SerializeField] private float buyAnimDuration = 0.5f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack; // Ease spécifique pour la fermeture
        [SerializeField] private Ease switchEase = Ease.InOutQuad;
        [SerializeField] private float staggerDelay = 0.05f; // Délai entre les animations des éléments

        [Header("Preview Settings")] [SerializeField]
        private Color unlockedColor = Color.white;

        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gris à 50%

        [Header("Car Info")] [SerializeField] private List<CarData> carsData = new List<CarData>();

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

            // Ajouter les boutons principaux à la liste s'ils n'y sont pas déjà
            if (!allButtons.Contains(previousButton) && previousButton != null)
                allButtons.Add(previousButton);
            if (!allButtons.Contains(nextButton) && nextButton != null)
                allButtons.Add(nextButton);
            if (!allButtons.Contains(actionButton) && actionButton != null)
                allButtons.Add(actionButton);

            // Configuration des boutons avec animations
            if (previousButton != null)
            {
                previousButton.onClick.AddListener(() =>
                {
                    AnimateButtonClick(previousButton.transform);
                    PreviousCar();
                });
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() =>
                {
                    AnimateButtonClick(nextButton.transform);
                    NextCar();
                });
            }

            if (actionButton != null)
            {
                actionButton.onClick.AddListener(() =>
                {
                    AnimateButtonClick(actionButton.transform);
                    OnActionButtonClick();
                });
            }

            // Configurer tous les boutons de la liste avec des effets de survol
            foreach (Button button in allButtons)
            {
                if (button != null)
                    AddHoverEffect(button);
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
            clickSequence.Append(buttonTransform.DOScale(0.85f, buttonClickAnimDuration / 2).SetEase(Ease.OutQuad));
            clickSequence.Append(buttonTransform.DOScale(1.1f, buttonClickAnimDuration / 3).SetEase(Ease.OutBack));
            clickSequence.Append(buttonTransform.DOScale(1f, buttonClickAnimDuration / 6).SetEase(Ease.OutQuad));
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
            float xOffset = isNext ? -150f : 150f;
            exitSequence.Join(carPreviewImage.transform.DOLocalMoveX(xOffset, switchCarAnimDuration / 2)
                .SetEase(switchEase));
            exitSequence.Join(carPreviewImage.DOFade(0, switchCarAnimDuration / 2));

            // Faire sortir les textes avec un effet de cascade
            if (carNameText != null)
                exitSequence.Join(carNameText.DOFade(0, switchCarAnimDuration / 2));

            if (carStatsText != null)
                exitSequence.Join(carStatsText.DOFade(0, switchCarAnimDuration / 2));

            // Après la sortie, mettre à jour les données et animer l'entrée
            exitSequence.OnComplete(() =>
            {
                UpdateCarDisplay();

                // Animer l'entrée des nouveaux éléments
                Sequence enterSequence = DOTween.Sequence();

                // Positionner l'image de l'autre côté avant l'animation
                carPreviewImage.transform.localPosition =
                    new Vector3(-xOffset, carPreviewImage.transform.localPosition.y, 0);
                carPreviewImage.color = new Color(carPreviewImage.color.r, carPreviewImage.color.g,
                    carPreviewImage.color.b, 0);

                // Faire entrer l'image avec un effet de rebond
                enterSequence.Join(carPreviewImage.transform.DOLocalMoveX(0, switchCarAnimDuration / 2)
                    .SetEase(Ease.OutBack));
                enterSequence.Join(carPreviewImage.DOFade(1, switchCarAnimDuration / 2));

                // Faire entrer les textes avec un effet de cascade
                if (carNameText != null)
                    enterSequence.Join(carNameText.DOFade(1, switchCarAnimDuration / 2).SetDelay(0.05f));

                if (carStatsText != null)
                    enterSequence.Join(carStatsText.DOFade(1, switchCarAnimDuration / 2).SetDelay(0.1f));

                enterSequence.OnComplete(() => { isAnimating = false; });
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
                    carPreviewImage.color =
                        new Color(targetColor.r, targetColor.g, targetColor.b, carPreviewImage.color.a);
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
                // Animation de refus d'achat plus élaborée
                if (actionButton != null)
                {
                    Sequence refuseSequence = DOTween.Sequence();
                    refuseSequence.Append(actionButton.transform.DOShakePosition(0.4f, 10, 20, 90, false, true));
                    refuseSequence.Join(actionButton.transform.DOShakeRotation(0.4f, 8, 5, 90, false));

                    // Effet de flash rouge sur le bouton
                    Image buttonImage = actionButton.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        Color originalColor = buttonImage.color;
                        refuseSequence.Join(buttonImage.DOColor(Color.red, 0.1f));
                        refuseSequence.Append(buttonImage.DOColor(originalColor, 0.3f));
                    }
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

            // Faire tourner l'image avec des effets plus élaborés
            if (carPreviewImage != null)
            {
                // Effet de pulsation et rotation
                purchaseSequence.Append(carPreviewImage.transform.DOScale(1.3f, buyAnimDuration / 3)
                    .SetEase(Ease.OutBack));
                purchaseSequence.Join(carPreviewImage.transform.DORotate(new Vector3(0, 360, 0), buyAnimDuration,
                    RotateMode.FastBeyond360));
                purchaseSequence.Append(carPreviewImage.transform.DOScale(0.9f, buyAnimDuration / 3)
                    .SetEase(Ease.InOutQuad));
                purchaseSequence.Append(
                    carPreviewImage.transform.DOScale(1f, buyAnimDuration / 3).SetEase(Ease.OutBack));

                // Changer la couleur de grisé à normal avec un effet de flash
                purchaseSequence.Join(carPreviewImage.DOColor(Color.yellow, buyAnimDuration / 4));
                purchaseSequence.Append(carPreviewImage.DOColor(unlockedColor, buyAnimDuration / 2));

                // Créer un effet de brillance temporaire
                GameObject glowObj = new GameObject("PurchaseGlow");
                glowObj.transform.SetParent(carPreviewImage.transform, false);
                RectTransform glowRect = glowObj.AddComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.sizeDelta = Vector2.zero;

                Image glowImage = glowObj.AddComponent<Image>();
                glowImage.sprite = carPreviewImage.sprite;
                glowImage.color = new Color(1f, 1f, 0.5f, 0f);

                // IMPORTANT: Désactiver les raycasts sur l'image de brillance
                glowImage.raycastTarget = false;

                // Animer l'effet de brillance
                purchaseSequence.Join(glowImage.DOFade(0.7f, buyAnimDuration / 4));
                purchaseSequence.Append(glowImage.DOFade(0f, buyAnimDuration / 2));
                purchaseSequence.OnComplete(() => { Destroy(glowObj); });

                // Ajouter un effet de particules (simulé avec des petites images)
                for (int i = 0; i < 8; i++)
                {
                    GameObject particle = new GameObject("Particle" + i);
                    particle.transform.SetParent(carPreviewImage.transform, false);

                    RectTransform particleRect = particle.AddComponent<RectTransform>();
                    particleRect.sizeDelta = new Vector2(20, 20);
                    particleRect.anchoredPosition = Vector2.zero;

                    Image particleImage = particle.AddComponent<Image>();
                    particleImage.color = new Color(1f, 0.8f, 0.2f, 1f);

                    // IMPORTANT: Désactiver les raycasts sur les particules
                    particleImage.raycastTarget = false;

                    // Calculer une direction aléatoire
                    float angle = i * 45f; // 8 directions espacées de 45 degrés
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                    // Animer la particule
                    float delay = i * 0.05f;
                    float distance = Random.Range(100f, 200f);

                    Sequence particleSequence = DOTween.Sequence();
                    particleSequence.SetDelay(delay);
                    particleSequence.Join(particleRect.DOAnchorPos(direction * distance, buyAnimDuration)
                        .SetEase(Ease.OutQuad));
                    particleSequence.Join(particleImage.DOFade(0, buyAnimDuration).SetEase(Ease.InQuad));
                    particleSequence.OnComplete(() => { Destroy(particle); });
                }
            }

            // Animer le bouton d'action
            if (actionButton != null)
            {
                purchaseSequence.Join(actionButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f),
                    buyAnimDuration / 2, 5, 0.5f));
            }

            // Mettre à jour l'interface après l'animation
            purchaseSequence.OnComplete(() =>
            {
                // Mettre à jour le bouton d'action
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

            // Animation plus élaborée pour la fermeture
            if (mainCanvasGroup != null)
            {
                // Faire disparaître progressivement le canvas
                closeSequence.Append(mainCanvasGroup.DOFade(0, closeAnimDuration).SetEase(closeEase));

                // Ajouter un effet de zoom arrière
                if (carInfoPanel != null)
                {
                    closeSequence.Join(carInfoPanel.DOScale(0.7f, closeAnimDuration).SetEase(closeEase));
                }
            }
            else
            {
                // Animer chaque élément individuellement avec un effet de cascade
                float delay = 0f;

                // Animer les boutons en premier avec un effet de cascade
                foreach (Button button in allButtons)
                {
                    if (button != null)
                    {
                        CanvasGroup buttonCG = button.GetComponent<CanvasGroup>();
                        if (buttonCG == null)
                        {
                            buttonCG = button.gameObject.AddComponent<CanvasGroup>();
                        }

                        closeSequence.Join(buttonCG.DOFade(0, closeAnimDuration * 0.7f).SetDelay(delay));
                        closeSequence.Join(button.transform.DOScale(0.8f, closeAnimDuration * 0.7f).SetEase(closeEase)
                            .SetDelay(delay));
                        delay += staggerDelay;
                    }
                }

                // Animer les textes
                if (carStatsText != null)
                {
                    closeSequence.Join(carStatsText.DOFade(0, closeAnimDuration * 0.8f).SetDelay(delay));
                    closeSequence.Join(carStatsText.transform.DOLocalMoveY(-20f, closeAnimDuration * 0.8f)
                        .SetRelative(true).SetEase(closeEase).SetDelay(delay));
                    delay += staggerDelay;
                }

                if (carNameText != null)
                {
                    closeSequence.Join(carNameText.DOFade(0, closeAnimDuration * 0.8f).SetDelay(delay));
                    closeSequence.Join(carNameText.transform.DOLocalMoveY(-20f, closeAnimDuration * 0.8f)
                        .SetRelative(true).SetEase(closeEase).SetDelay(delay));
                    delay += staggerDelay;
                }

                // Animer l'image en dernier avec un effet plus prononcé
                if (carPreviewImage != null)
                {
                    closeSequence.Join(carPreviewImage.DOFade(0, closeAnimDuration).SetDelay(delay));
                    closeSequence.Join(carPreviewImage.transform.DOScale(0.5f, closeAnimDuration).SetEase(closeEase)
                        .SetDelay(delay));
                    closeSequence.Join(carPreviewImage.transform.DOLocalMoveY(-50f, closeAnimDuration).SetRelative(true)
                        .SetEase(closeEase).SetDelay(delay));
                }

                // Animer le panneau principal
                if (carInfoPanel != null)
                {
                    closeSequence.Join(carInfoPanel.DOScale(0.7f, closeAnimDuration).SetEase(closeEase));
                }
            }

            // Ajouter un effet de transition global
            closeSequence.OnComplete(() =>
            {
                // Créer un effet de flash final (optionnel)
                GameObject flashObj = new GameObject("CloseFlash");
                flashObj.transform.SetParent(transform, false);

                RectTransform flashRect = flashObj.AddComponent<RectTransform>();
                flashRect.anchorMin = Vector2.zero;
                flashRect.anchorMax = Vector2.one;
                flashRect.sizeDelta = Vector2.zero;

                Image flashImage = flashObj.AddComponent<Image>();
                flashImage.color = new Color(1f, 1f, 1f, 0f);

                // Séquence de flash rapide
                Sequence flashSequence = DOTween.Sequence();
                flashSequence.Append(flashImage.DOFade(0.3f, 0.1f));
                flashSequence.Append(flashImage.DOFade(0f, 0.1f));
                flashSequence.OnComplete(() =>
                {
                    Destroy(flashObj);
                    shouldCloseUI = true;
                    gameObject.SetActive(false);
                    isAnimating = false;
                });
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

            // Réinitialiser la position et rotation
            selectedCar.transform.position = carSpawnPoint.position;
            selectedCar.transform.rotation = carSpawnPoint.rotation;

            // Positionner tous les enfants au même endroit que le spawnpoint
            for (int i = 0; i < selectedCar.transform.childCount; i++)
            {
                Transform child = selectedCar.transform.GetChild(i);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
            }

            // Animation de la voiture sélectionnée avec un effet plus élaboré
            selectedCar.transform.localScale = Vector3.zero;

            Sequence carAppearSequence = DOTween.Sequence();
            carAppearSequence.Append(selectedCar.transform.DOScale(0, 0.01f));

            // Effet de rebond avec une légère rotation
            carAppearSequence.Append(selectedCar.transform.DOScale(1.2f, 0.4f).SetEase(Ease.OutQuad));
            carAppearSequence.Join(selectedCar.transform.DORotate(new Vector3(0, 15f, 0), 0.4f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutQuad));
            carAppearSequence.Append(selectedCar.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InOutQuad));
            carAppearSequence.Join(selectedCar.transform
                .DORotate(new Vector3(0, -30f, 0), 0.2f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
            carAppearSequence.Append(selectedCar.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            carAppearSequence.Join(selectedCar.transform.DORotate(new Vector3(0, 15f, 0), 0.2f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutQuad));

            // Initialiser la caméra pour la nouvelle voiture
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                SmoothCamera smoothCam = mainCamera.GetComponent<SmoothCamera>();
                if (smoothCam != null)
                {
                    smoothCam.target = selectedCar.transform;
                    smoothCam.ResetCameraPosition();
                }

                DynamicFOVController fovController = mainCamera.GetComponent<DynamicFOVController>();
                if (fovController != null)
                {
                    fovController.target = selectedCar.transform;
                }
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
                    carPreviewImage.color = new Color(carPreviewImage.color.r, carPreviewImage.color.g,
                        carPreviewImage.color.b, 0);
                    carPreviewImage.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    carPreviewImage.transform.localPosition = new Vector3(carPreviewImage.transform.localPosition.x,
                        carPreviewImage.transform.localPosition.y - 50f,
                        carPreviewImage.transform.localPosition.z);
                }

                if (carNameText != null)
                    carNameText.alpha = 0;

                if (carStatsText != null)
                    carStatsText.alpha = 0;

                // Préparer tous les boutons
                foreach (Button button in allButtons)
                {
                    if (button != null)
                    {
                        CanvasGroup buttonCG = button.GetComponent<CanvasGroup>();
                        if (buttonCG == null)
                        {
                            buttonCG = button.gameObject.AddComponent<CanvasGroup>();
                        }

                        buttonCG.alpha = 0;
                        button.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    }
                }

                if (carInfoPanel != null)
                    carInfoPanel.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }

            // Séquence d'animation pour l'ouverture
            Sequence openSequence = DOTween.Sequence();

            // Animer l'apparition progressive
            if (mainCanvasGroup != null)
            {
                openSequence.Append(mainCanvasGroup.DOFade(1, openAnimDuration).SetEase(openEase));

                // Animer le panneau principal
                if (carInfoPanel != null)
                {
                    carInfoPanel.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    openSequence.Join(carInfoPanel.DOScale(1, openAnimDuration).SetEase(openEase));
                }
            }
            else
            {
                // Animer chaque élément individuellement avec un effet de cascade
                float delay = 0f;

                // Animer le panneau principal en premier
                if (carInfoPanel != null)
                {
                    openSequence.Append(carInfoPanel.DOScale(1, openAnimDuration).SetEase(openEase));
                }

                // Animer l'image avec un effet d'entrée
                if (carPreviewImage != null)
                {
                    openSequence.Join(carPreviewImage.transform
                        .DOLocalMoveY(carPreviewImage.transform.localPosition.y + 50f, openAnimDuration)
                        .SetEase(openEase).SetDelay(delay));
                    openSequence.Join(carPreviewImage.DOFade(1, openAnimDuration).SetDelay(delay));
                    openSequence.Join(carPreviewImage.transform.DOScale(1, openAnimDuration).SetEase(openEase)
                        .SetDelay(delay));
                    delay += staggerDelay;
                }

                // Animer les textes avec un effet de cascade
                if (carNameText != null)
                {
                    openSequence.Join(carNameText.DOFade(1, openAnimDuration).SetDelay(delay));
                    openSequence.Join(carNameText.transform.DOLocalMoveY(20f, openAnimDuration).SetRelative(true)
                        .SetEase(openEase).SetDelay(delay));
                    delay += staggerDelay;
                }

                if (carStatsText != null)
                {
                    openSequence.Join(carStatsText.DOFade(1, openAnimDuration).SetDelay(delay));
                    openSequence.Join(carStatsText.transform.DOLocalMoveY(20f, openAnimDuration).SetRelative(true)
                        .SetEase(openEase).SetDelay(delay));
                    delay += staggerDelay;
                }

                // Animer les boutons en dernier avec un effet de cascade
                foreach (Button button in allButtons)
                {
                    if (button != null)
                    {
                        CanvasGroup buttonCG = button.GetComponent<CanvasGroup>();
                        if (buttonCG == null)
                        {
                            buttonCG = button.gameObject.AddComponent<CanvasGroup>();
                        }

                        openSequence.Join(buttonCG.DOFade(1, openAnimDuration).SetDelay(delay));
                        openSequence.Join(button.transform.DOScale(1, openAnimDuration).SetEase(Ease.OutBack)
                            .SetDelay(delay));
                        delay += staggerDelay;
                    }
                }
            }

            // Ajouter un effet de transition global
            GameObject flashObj = new GameObject("OpenFlash");
            flashObj.transform.SetParent(transform, false);

            RectTransform flashRect = flashObj.AddComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.sizeDelta = Vector2.zero;

            Image flashImage = flashObj.AddComponent<Image>();
            flashImage.color = new Color(1f, 1f, 1f, 0.3f);

            // Séquence de flash rapide
            Sequence flashSequence = DOTween.Sequence();
            flashSequence.Append(flashImage.DOFade(0f, 0.3f));
            flashSequence.OnComplete(() => { Destroy(flashObj); });

            // Terminer l'animation
            openSequence.OnComplete(() =>
            {
                isAnimating = false;

                // Ajouter un petit effet de rebond sur les boutons pour attirer l'attention
                foreach (Button button in allButtons)
                {
                    if (button != null && button.interactable)
                    {
                        float randomDelay = Random.Range(0f, 0.5f);
                        button.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 1, 0.5f)
                            .SetDelay(randomDelay);
                    }
                }
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
        private void AddHoverEffect(Button button)
        {
            // Ajouter des écouteurs d'événements pour le survol
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ??
                                   button.gameObject.AddComponent<EventTrigger>();

            // Nettoyer les entrées existantes pour éviter les doublons
            trigger.triggers.Clear();

            // Entrée du survol
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) =>
            {
                if (!isAnimating && button.interactable)
                {
                    // Effet de survol plus élaboré
                    button.transform.DOKill(false);
                    Sequence hoverSequence = DOTween.Sequence();
                    hoverSequence.Append(button.transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad));

                    // Ajouter un effet de rotation légère
                    hoverSequence.Join(button.transform.DORotate(new Vector3(0, 0, -2f), 0.2f).SetEase(Ease.OutQuad));

                    // Effet de brillance (si le bouton a une image)
                    Image buttonImage = button.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        Color originalColor = buttonImage.color;
                        Color brighterColor = new Color(
                            Mathf.Min(originalColor.r + 0.1f, 1f),
                            Mathf.Min(originalColor.g + 0.1f, 1f),
                            Mathf.Min(originalColor.b + 0.1f, 1f),
                            originalColor.a
                        );
                        hoverSequence.Join(buttonImage.DOColor(brighterColor, 0.2f));
                    }
                }
            });
            trigger.triggers.Add(entryEnter);

            // Sortie du survol
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) =>
            {
                if (!isAnimating)
                {
                    button.transform.DOKill(false);
                    Sequence exitSequence = DOTween.Sequence();
                    exitSequence.Append(button.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad));
                    exitSequence.Join(button.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad));

                    // Restaurer la couleur d'origine
                    Image buttonImage = button.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        Color originalColor = button.interactable
                            ? buttonImage.color
                            : new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0.5f);
                        exitSequence.Join(buttonImage.DOColor(originalColor, 0.2f));
                    }
                }
            });
            trigger.triggers.Add(entryExit);

            // Effet de pression
            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) =>
            {
                if (!isAnimating && button.interactable)
                {
                    button.transform.DOKill(false);
                    button.transform.DOScale(0.95f, 0.1f).SetEase(Ease.OutQuad);
                }
            });
            trigger.triggers.Add(entryDown);

            // Effet de relâchement
            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) =>
            {
                if (!isAnimating && button.interactable)
                {
                    button.transform.DOKill(false);
                    button.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad);
                }
            });
            trigger.triggers.Add(entryUp);
        }

// Méthode pour nettoyer les tweens à la destruction
        private void OnDestroy()
        {
            // Tuer tous les tweens associés aux objets
            DOTween.Kill(carPreviewImage);
            DOTween.Kill(carNameText);
            DOTween.Kill(carStatsText);

            foreach (Button button in allButtons)
            {
                if (button != null)
                    DOTween.Kill(button.transform);
            }

            DOTween.Kill(carInfoPanel);
            DOTween.Kill(mainCanvasGroup);
        }

// Méthode pour la fermeture du garage par le joueur (bouton externe)
        public void CloseGarage()
        {
            if (!isAnimating)
            {
                AnimateClose();
            }
        }
    }
}