using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    [System.Serializable]
    public class CarConfigEntry
    {
        public CarConfig carConfig;
        public GameObject carPrefab;
    }
    
    /// <summary>
    /// Manages the garage UI, including car, gun, and station slots.
    /// </summary>
    public class GarageUIManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject slotPrefab;

        [Header("Slot Lists")]
        [SerializeField] private GameObject carSlotList;
        [SerializeField] private GameObject gunSlotList;
        [SerializeField] private GameObject stationSlotList;

        [Header("Buttons")]
        [SerializeField] private GameObject parametersBtnList;

        [Header("Configs")]
        [Tooltip("Liste des configurations de voiture")]
        [SerializeField] private List<CarConfigEntry> carConfigs;
        #endregion
        
        private void Start()
        {
            foreach (var carConfigEntry in carConfigs)
            {
                GameObject slot = Instantiate(slotPrefab, carSlotList.transform);
                slot.GetComponent<Toggle>().onValueChanged.AddListener(OnSlotSelected);
            }
        }
        
        private void OnSlotSelected(bool arg0)
        {
            parametersBtnList.SetActive(arg0);
        }
    }
}