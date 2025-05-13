using Builds;
using Car;
using Guns;
using UnityEngine;

namespace Menu
{
    public class ConfigurationManager : MonoBehaviour
    {
        [SerializeField] private CarManager carManager;
        [SerializeField] private GunManager gunManager;
        [SerializeField] private StationManager stationManager;
    
        // Référence au GarageUIManager
        [SerializeField] private GarageUIManager garageUI;
    
        private void Start()
        {
            if (garageUI == null)
            {
                garageUI = GetComponent<GarageUIManager>();
            }
        
            // Initialiser les configurations actuelles
            if (carManager != null && carManager.CurrentConfig != null)
            {
                // Trouver l'entrée de configuration correspondante
                var carConfigEntry = FindCarConfigEntry(carManager.CurrentConfig);
                if (carConfigEntry != null)
                {
                    garageUI.SetCurrentCarConfig(carConfigEntry);
                }
            }
        
            if (gunManager != null && gunManager.CurrentConfig != null)
            {
                var gunConfigEntry = FindGunConfigEntry(gunManager.CurrentConfig);
                if (gunConfigEntry != null)
                {
                    garageUI.SetCurrentGunConfig(gunConfigEntry);
                }
            }
        
            if (stationManager != null && stationManager.CurrentStationPrefab != null)
            {
                var stationConfigEntry = FindStationConfigEntry(stationManager.CurrentStationPrefab);
                if (stationConfigEntry != null)
                {
                    garageUI.SetCurrentStationConfig(stationConfigEntry);
                }
            }
        
            // S'abonner aux événements de sélection
            garageUI.OnCarConfigSelected += ApplyCarConfig;
            garageUI.OnGunConfigSelected += ApplyGunConfig;
            garageUI.OnStationConfigSelected += ApplyStationConfig;
        }
    
        private void OnDestroy()
        {
            // Se désabonner des événements
            if (garageUI != null)
            {
                garageUI.OnCarConfigSelected -= ApplyCarConfig;
                garageUI.OnGunConfigSelected -= ApplyGunConfig;
                garageUI.OnStationConfigSelected -= ApplyStationConfig;
            }
        }
    
        private void ApplyCarConfig(CarConfigEntry config)
        {
            if (carManager != null && config != null)
            {
                carManager.ApplyConfiguration(config.CarConfig);
                Debug.Log($"Applied car config: {config.DisplayName}");
            }
        }
    
        private void ApplyGunConfig(GunConfigEntry config)
        {
            if (gunManager != null && config != null)
            {
                gunManager.ApplyConfiguration(config.GunConfig);
                Debug.Log($"Applied gun config: {config.DisplayName}");
            }
        }
    
        private void ApplyStationConfig(StationConfigEntry config)
        {
            if (stationManager != null && config != null)
            {
                stationManager.SpawnStation(config.Prefab);
                Debug.Log($"Applied station config: {config.DisplayName}");
            }
        }
        
        private CarConfigEntry FindCarConfigEntry(CarConfig config)
        {
            foreach (var entry in garageUI.CarConfigs)
            {
                if (entry.CarConfig == config)
                {
                    return entry;
                }
            }
            return null;
        }
        
        private GunConfigEntry FindGunConfigEntry(GunConfig config)
        {
            foreach (var entry in garageUI.GunConfigs)
            {
                if (entry.GunConfig == config)
                {
                    return entry;
                }
            }
            return null;
        }
        
        private StationConfigEntry FindStationConfigEntry(GameObject prefab)
        {
            foreach (var entry in garageUI.StationConfigs)
            {
                if (entry.Prefab == prefab)
                {
                    return entry;
                }
            }
            return null;
        }
    }
}