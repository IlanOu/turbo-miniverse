using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    [Header("Recharge Settings")]
    [SerializeField] private float rechargeRate = 10f; // Battery units recharged per second

    /// <summary>
    /// When a collider stays in the trigger zone, check if it has a CarBattery component and recharge it.
    /// </summary>
    /// <param name="other">Collider of the object in the trigger zone.</param>
    private void OnTriggerStay(Collider other)
    {
        CarBattery battery = other.GetComponent<CarBattery>();
        if (battery == null)
        {
            battery = other.GetComponentInParent<CarBattery>();
        }
        if (battery != null)
        {
            // Recharge the battery gradually based on the recharge rate
            battery.RechargeBattery(rechargeRate * Time.deltaTime);
        }
    }
}