using System;
using System.Collections;
using System.Collections.Generic;
using Builds;
using Car;
using Guns;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// Interface commune pour tous les types de configuration
    /// </summary>
    public interface IConfigEntry
    {
        GameObject Prefab { get; }
        string DisplayName { get; }
    }

    [Serializable]
    public class CarConfigEntry : IConfigEntry
    {
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private string displayName;

        public CarConfig CarConfig => carConfig;
        public GameObject Prefab => carPrefab;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? carConfig?.name ?? "Voiture" : displayName;
    }
    
    [Serializable]
    public class GunConfigEntry : IConfigEntry
    {
        [SerializeField] private GunConfig gunConfig;
        [SerializeField] private GameObject gunPrefab;
        [SerializeField] private string displayName;

        public GunConfig GunConfig => gunConfig;
        public GameObject Prefab => gunPrefab;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? gunConfig?.name ?? "Arme" : displayName;
    }
    
    [Serializable]
    public class StationConfigEntry : IConfigEntry
    {
        [SerializeField] private GameObject stationPrefab;
        [SerializeField] private string displayName = "Station";

        public GameObject Prefab => stationPrefab;
        public string DisplayName => displayName;
    }
    
    /// <summary>
    /// Manages the garage UI, including car, gun, and station slots.
    /// </summary>
    public class GarageUIManager : MonoBehaviour
    {
        
        
        public void SetCurrentCarConfig(CarConfigEntry config)
        {
            currentCarConfig = config;
        }
    
        public void SetCurrentGunConfig(GunConfigEntry config)
        {
            currentGunConfig = config;
        }
    
        public void SetCurrentStationConfig(StationConfigEntry config)
        {
            currentStationConfig = config;
        }
        
        [Serializable]
        private class SlotGroup
        {
            [SerializeField] private GameObject slotListContainer;
            [SerializeField] private List<Toggle> instantiatedSlots = new List<Toggle>();

            public GameObject Container => slotListContainer;
            public List<Toggle> Slots => instantiatedSlots;

            public void AddSlot(Toggle slot)
            {
                instantiatedSlots.Add(slot);
            }

            public void ClearSlots()
            {
                foreach (var slot in instantiatedSlots)
                {
                    if (slot != null)
                    {
                        Destroy(slot.gameObject);
                    }
                }
                instantiatedSlots.Clear();
            }
        }

        #region Serialized Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject slotPrefab;

        [Header("Slot Groups")]
        [SerializeField] private SlotGroup carSlotGroup = new SlotGroup();
        [SerializeField] private SlotGroup gunSlotGroup = new SlotGroup();
        [SerializeField] private SlotGroup stationSlotGroup = new SlotGroup();

        [Header("UI Elements")]
        [SerializeField] private GameObject parametersBtnList;

        [Header("Configs")]
        [Tooltip("Liste des configurations de voiture")]
        [SerializeField] private List<CarConfigEntry> carConfigs = new List<CarConfigEntry>();
        
        [Tooltip("Liste des configurations de guns")]
        [SerializeField] private List<GunConfigEntry> gunConfigs = new List<GunConfigEntry>();
        
        [Tooltip("Liste des stations")]
        [SerializeField] private List<StationConfigEntry> stationConfigs = new List<StationConfigEntry>();
        #endregion
        
        public List<CarConfigEntry> CarConfigs => carConfigs;
        public List<GunConfigEntry> GunConfigs => gunConfigs;
        public List<StationConfigEntry> StationConfigs => stationConfigs;

        private Toggle currentSelectedSlot;
        
        // Délegates
        public delegate void CarConfigSelectedHandler(CarConfigEntry config);
        public delegate void GunConfigSelectedHandler(GunConfigEntry config);
        public delegate void StationConfigSelectedHandler(StationConfigEntry config);

        // Evénements
        public event CarConfigSelectedHandler OnCarConfigSelected;
        public event GunConfigSelectedHandler OnGunConfigSelected;
        public event StationConfigSelectedHandler OnStationConfigSelected;
        
        private CarConfigEntry currentCarConfig;
        private GunConfigEntry currentGunConfig;
        private StationConfigEntry currentStationConfig;
        
        private void Awake()
        {
            ValidateReferences();
        }
        
        private void OnEnable()
        {
            InitializeUI();
        }

        private void OnDisable()
        {
            ClearAllSlots();
        }

        private void ValidateReferences()
        {
            if (slotPrefab == null)
            {
                Debug.LogError("GarageUIManager: slotPrefab is not assigned!");
            }

            if (carSlotGroup.Container == null)
            {
                Debug.LogError("GarageUIManager: carSlotList is not assigned!");
            }

            if (gunSlotGroup.Container == null)
            {
                Debug.LogError("GarageUIManager: gunSlotList is not assigned!");
            }

            if (stationSlotGroup.Container == null)
            {
                Debug.LogError("GarageUIManager: stationSlotList is not assigned!");
            }

            if (parametersBtnList == null)
            {
                Debug.LogError("GarageUIManager: parametersBtnList is not assigned!");
            }
        }
        
        private void InitializeUI()
        {
            ClearAllSlots();
            
            // Initialiser les slots avec les valeurs par défaut
            parametersBtnList.SetActive(false);
            
            // Créer les slots pour chaque type de configuration
            CreateSlots(carConfigs, carSlotGroup);
            CreateSlots(gunConfigs, gunSlotGroup);
            CreateSlots(stationConfigs, stationSlotGroup);
        }

        private void ClearAllSlots()
        {
            carSlotGroup.ClearSlots();
            gunSlotGroup.ClearSlots();
            stationSlotGroup.ClearSlots();
        }
        
        private void CreateSlots<T>(List<T> configs, SlotGroup slotGroup) where T : IConfigEntry
        {
            if (slotPrefab == null || slotGroup.Container == null)
            {
                Debug.LogWarning("GarageUIManager: Missing references for creating slots");
                return;
            }

            foreach (var config in configs)
            {
                if (config == null || config.Prefab == null)
                {
                    Debug.LogWarning("GarageUIManager: Skipping null config or prefab");
                    continue;
                }

                GameObject slotObject = Instantiate(slotPrefab, slotGroup.Container.transform);
                if (slotObject == null)
                {
                    Debug.LogError("GarageUIManager: Failed to instantiate slot");
                    continue;
                }

                Toggle slotToggle = slotObject.GetComponent<Toggle>();
                if (slotToggle == null)
                {
                    Debug.LogError("GarageUIManager: Slot prefab does not have a Toggle component");
                    Destroy(slotObject);
                    continue;
                }

                // Configurer le slot avec les informations de l'élément
                Text slotText = slotObject.GetComponentInChildren<Text>();
                if (slotText != null)
                {
                    slotText.text = config.DisplayName;
                }

                // Vérifier si ce slot correspond à la configuration actuelle
                bool shouldBeSelected = false;
                if (config is CarConfigEntry carConfig && currentCarConfig != null)
                {
                    shouldBeSelected = carConfig.CarConfig == currentCarConfig.CarConfig;
                }
                else if (config is GunConfigEntry gunConfig && currentGunConfig != null)
                {
                    shouldBeSelected = gunConfig.GunConfig == currentGunConfig.GunConfig;
                }
                else if (config is StationConfigEntry stationConfig && currentStationConfig != null)
                {
                    shouldBeSelected = stationConfig.Prefab == currentStationConfig.Prefab;
                }

                // Stocker la référence à la configuration dans le slot
                slotToggle.onValueChanged.AddListener((isOn) => OnSlotSelected(slotToggle, isOn, config));
                slotGroup.AddSlot(slotToggle);
                
                // Important: définir isOn après avoir ajouté le listener pour éviter de déclencher l'événement
                if (shouldBeSelected)
                {
                    // Utiliser un délai d'une frame pour éviter les problèmes de timing
                    StartCoroutine(SelectToggleNextFrame(slotToggle));
                }
            }
        }
        
        private IEnumerator SelectToggleNextFrame(Toggle toggle)
        {
            yield return null; // Attendre une frame
            toggle.isOn = true;
            currentSelectedSlot = toggle;
        }
        
        private void OnSlotSelected<T>(Toggle toggle, bool isOn, T config) where T : IConfigEntry
        {
            if (isOn)
            {
                // Désélectionner l'ancien slot si nécessaire
                if (currentSelectedSlot != null && currentSelectedSlot != toggle)
                {
                    currentSelectedSlot.isOn = false;
                }
    
                currentSelectedSlot = toggle;
                parametersBtnList.SetActive(true);
    
                // Mettre à jour la configuration actuelle et déclencher l'événement
                if (config is CarConfigEntry carConfig)
                {
                    currentCarConfig = carConfig;
                    OnCarConfigSelected?.Invoke(carConfig);
                }
                else if (config is GunConfigEntry gunConfig)
                {
                    currentGunConfig = gunConfig;
                    OnGunConfigSelected?.Invoke(gunConfig);
                }
                else if (config is StationConfigEntry stationConfig)
                {
                    currentStationConfig = stationConfig;
                    OnStationConfigSelected?.Invoke(stationConfig);
                }
    
                Debug.Log($"Selected: {config.DisplayName}");
            }
            else if (toggle == currentSelectedSlot)
            {
                currentSelectedSlot = null;
                parametersBtnList.SetActive(false);
            }
        }
    }
}
