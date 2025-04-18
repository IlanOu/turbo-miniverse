using System.Collections.Generic;
using Menu;
using UnityEngine;
using UnityEngine.UI;

public class GarageSlotSelector : MonoBehaviour
{
    [Header("Toggle Group Parent")]
    [SerializeField] private ToggleGroup toggleGroup;

    [Header("Slots sélectionnés dynamiquement")]
    [SerializeField] private List<Toggle> slotToggles = new List<Toggle>();

    private void Awake()
    {
        if (toggleGroup == null)
            toggleGroup = GetComponent<ToggleGroup>();

        // Récupérer tous les Toggles enfants
        slotToggles.Clear();
        foreach (Transform child in transform)
        {
            Toggle toggle = child.GetComponent<Toggle>();
            if (toggle != null)
            {
                slotToggles.Add(toggle);
                toggle.group = toggleGroup;

                // Get the first toggle and set it as selected by default
                if (slotToggles.Count == 1)
                    toggle.isOn = true;
                
                // Abonnement à l'événement de sélection
                toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn)
                        OnSlotSelected(toggle);
                });
            }
        }
    }

    private void OnSlotSelected(Toggle selectedToggle)
    {
        int index = slotToggles.IndexOf(selectedToggle);
        Debug.Log($"🟢 Slot sélectionné : Slot #{index + 1}");

    }

    public int GetSelectedSlotIndex()
    {
        for (int i = 0; i < slotToggles.Count; i++)
        {
            if (slotToggles[i].isOn)
                return i;
        }
        return -1;
    }
}