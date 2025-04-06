using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GarageSlotSelector : MonoBehaviour
{
    private List<Button> slotButtons = new List<Button>();
    private int selectedSlotIndex = -1;

    public delegate void OnSlotSelectedDelegate(int index);
    public event OnSlotSelectedDelegate OnSlotSelected;

    private void Start()
    {
        // Récupère automatiquement tous les boutons enfants
        slotButtons.AddRange(GetComponentsInChildren<Button>());

        for (int i = 0; i < slotButtons.Count; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => SelectSlot(index));
        }

        // Optionnel : sélectionne automatiquement le premier slot au démarrage
        if (slotButtons.Count > 0)
        {
            SelectSlot(0);
        }
    }

    public void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        OnSlotSelected?.Invoke(index);
        // UpdateVisualFeedback();
    }

    private void UpdateVisualFeedback()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            ColorBlock cb = slotButtons[i].colors;
            cb.normalColor = (i == selectedSlotIndex) ? Color.cyan : Color.white;
            slotButtons[i].colors = cb;
        }
    }

    public int GetSelectedSlot() => selectedSlotIndex;
}