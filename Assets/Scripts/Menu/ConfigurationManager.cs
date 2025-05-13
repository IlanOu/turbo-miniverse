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
    }
}