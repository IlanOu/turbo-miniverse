using System.Collections;
using TMPro;
using UnityEngine;

public class CarBattery : MonoBehaviour
{
    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float currentBattery = 100f;
    [SerializeField] private float consumptionFactor = 0.5f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI batteryText;

    [Header("Components to Disable When Battery is Empty")]
    [SerializeField] private MonoBehaviour[] disableOnEmpty;

    [Header("Engine Off Deceleration Settings")]
    [SerializeField] private float decelerationFactor = 0.5f;

    [Header("Car Controller Reference")]
    [SerializeField] private CarController carController; // <- Ajout ici

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        UpdateBatteryUI();

        if (carController == null)
        {
            Debug.LogWarning("🚨 CarBattery: La référence à CarController n’est pas assignée !");
        }
    }

    private void Update()
    {
        ConsumeBattery();
        CheckBatteryStatus();
    }

    private void FixedUpdate()
    {
        if (currentBattery <= 0f && rb != null)
        {
            ApplyEngineOffDeceleration();
        }
    }

    private void ConsumeBattery()
    {
        if (rb == null || carController == null || !carController.IsAccelerating())
            return;

        float speed = rb.linearVelocity.magnitude;
        float consumption = speed * consumptionFactor * Time.deltaTime;
        currentBattery = Mathf.Max(currentBattery - consumption, 0f);
        UpdateBatteryUI();
    }


    private void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            batteryText.text = "Battery: " + Mathf.RoundToInt(currentBattery) + " / " + Mathf.RoundToInt(maxBattery);
        }
    }

    private void CheckBatteryStatus()
    {
        if (currentBattery <= 0f)
        {
            // Appel de StopCar
            if (carController != null)
            {
                carController.StopCar();
            }

            foreach (MonoBehaviour comp in disableOnEmpty)
            {
                if (comp != null)
                    comp.enabled = false;
            }
        }
        else
        {
            if (carController != null)
            {
                carController.StartCar();
            }
            
            foreach (MonoBehaviour comp in disableOnEmpty)
            {
                if (comp != null)
                    comp.enabled = true;
            }
        }
    }

    private void ApplyEngineOffDeceleration()
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, decelerationFactor * Time.fixedDeltaTime);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, decelerationFactor * Time.fixedDeltaTime);
    }

    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0f, maxBattery);
        UpdateBatteryUI();
    }

    public bool IsBatteryEmpty()
    {
        return currentBattery <= 0f;
    }
}
