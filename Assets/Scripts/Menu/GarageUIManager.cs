using UnityEngine;

public class GarageUIManager : MonoBehaviour
{
    public static GarageUIManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void UpdateStatsForSlot(int slotIndex)
    {
        Debug.Log($"🛠️ Mise à jour des stats pour le slot {slotIndex}");
        // Ici tu modifies les valeurs dans la partie Stats and Parameters
    }
}