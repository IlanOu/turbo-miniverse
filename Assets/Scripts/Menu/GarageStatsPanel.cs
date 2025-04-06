using UnityEngine;
using TMPro;

public class GarageStatsPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI line1Text;
    public TextMeshProUGUI line2Text;
    public TextMeshProUGUI line3Text;

    private void OnEnable()
    {
        FindObjectOfType<GarageSlotSelector>().OnSlotSelected += UpdateStats;
    }

    private void OnDisable()
    {
        FindObjectOfType<GarageSlotSelector>().OnSlotSelected -= UpdateStats;
    }

    private void UpdateStats(int index)
    {
        // Ici tu peux lier avec tes configs ou ScriptableObjects pour afficher des données réelles

        line1Text.text = $"Slot {index + 1} - Puissance : {Random.Range(100, 200)}";
        line2Text.text = $"Vitesse max : {Random.Range(50, 150)}";
        line3Text.text = $"Modifié : {(Random.value > 0.5f ? "Oui" : "Non")}";
    }
}