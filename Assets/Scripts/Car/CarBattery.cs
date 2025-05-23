﻿using System.Collections;
using Car;
using TMPro;
using UnityEngine;

public class CarBattery : MonoBehaviour
{
    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 100f;
    private float _currentBattery = 100f;
    [SerializeField] private float consumptionFactor = 0.5f;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI batteryText;

    [Header("Components to Disable When Battery is Empty")]
    [SerializeField] private MonoBehaviour[] disableOnEmpty;

    [Header("Engine Off Deceleration Settings")]
    [SerializeField] private float decelerationFactor = 0.5f;

    [Header("Car Controller Reference")]
    [SerializeField] private CarController carController;
    [SerializeField] private Rigidbody carRigidbody;

    private bool wasBatteryEmpty = false;

    private void Start()
    {
        FillBatteryToMax();
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
        if (_currentBattery <= 0f && carRigidbody != null)
        {
            ApplyEngineOffDeceleration();
        }
    }

    private void ConsumeBattery()
    {
        if (carRigidbody == null || carController == null || !carController.IsAccelerating())
            return;

        float speed = carRigidbody.linearVelocity.magnitude;
        float consumption = speed * consumptionFactor * Time.deltaTime;
        _currentBattery = Mathf.Max(_currentBattery - consumption, 0f);
        UpdateBatteryUI();
    }
    
    public bool TryConsumeEnergy(float amount)
    {
        if (_currentBattery <= 0f || amount <= 0f)
            return false;

        _currentBattery = Mathf.Max(_currentBattery - amount, 0f);
        UpdateBatteryUI();
        return true;
    }


    private void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            batteryText.text = "Battery: " + Mathf.RoundToInt(_currentBattery) + " / " + Mathf.RoundToInt(maxBattery);
        }
    }

    private void CheckBatteryStatus()
    {
        bool isBatteryEmpty = _currentBattery <= 0f;

        // Éviter de répéter les actions si l’état n’a pas changé
        if (isBatteryEmpty != wasBatteryEmpty)
        {
            wasBatteryEmpty = isBatteryEmpty;

            if (carController != null)
            {
                if (isBatteryEmpty)
                    carController.StopCar();
                else
                    carController.StartCar();
            }

            foreach (MonoBehaviour comp in disableOnEmpty)
            {
                if (comp != null)
                    comp.enabled = !isBatteryEmpty;
            }
        }
    }

    private void ApplyEngineOffDeceleration()
    {
        carRigidbody.linearVelocity = Vector3.Lerp(carRigidbody.linearVelocity, Vector3.zero, decelerationFactor * Time.fixedDeltaTime);
        carRigidbody.angularVelocity = Vector3.Lerp(carRigidbody.angularVelocity, Vector3.zero, decelerationFactor * Time.fixedDeltaTime);
    }

    public void RechargeBattery(float amount)
    {
        _currentBattery = Mathf.Clamp(_currentBattery + amount, 0f, maxBattery);
        UpdateBatteryUI();
    }

    public bool IsBatteryEmpty()
    {
        return _currentBattery <= 0f;
    }
    
    public void SetMaxBattery(float newMaxBattery)
    {
        maxBattery = newMaxBattery;
        _currentBattery = Mathf.Clamp(_currentBattery, 0f, maxBattery);
        UpdateBatteryUI();
    }
    
    public void FillBatteryToMax()
    {
        _currentBattery = maxBattery;
        UpdateBatteryUI();
    }
}
