using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentBrakeForce;
    private bool isBraking;
    private float currentSpeed = 0f;
    private bool _isDrifting = false;
    [SerializeField] private float driftThreshold = 0.5f; // Seuil pour détecter un drift

    // Settings - Ajustés pour un style arcade/jouet
    [SerializeField] private float maxMotorForce = 3000f;  // Augmenté pour une accélération rapide
    [SerializeField] private float brakeForce = 5000f;     // Freins plus puissants pour arrêts rapides
    [SerializeField] private float maxSteerAngle = 45f;    // Angle de braquage exagéré pour virages serrés
    [SerializeField] private float maxSpeed = 200f;        // Vitesse max plus élevée
    [SerializeField] private float accelerationCurve = 0.5f; // Réduit pour une accélération instantanée
    [SerializeField] private float additionalGravity = 20f; // Force vers le bas pour éviter les sauts excessifs
    
    // Paramètres de drift ajustables
    [Range(0.2f, 2.0f)]
    [SerializeField] private float driftFactor = 0.8f;      // Contrôle le glissement (plus bas = plus de drift)
    [Range(0.1f, 2.0f)]
    [SerializeField] private float gripFactor = 1.0f;       // Contrôle l'adhérence générale (plus haut = plus d'adhérence)
    [Range(5f, 50f)]
    [SerializeField] private float driftAngleThreshold = 15f; // Angle à partir duquel le drift est détecté
    [Range(0.1f, 1.0f)]
    [SerializeField] private float turnGripFactor = 0.5f;   // Adhérence dans les virages (plus bas = plus de drift)

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    // UI Elements
    [SerializeField] private TextMeshProUGUI speedText;

    private Rigidbody carRigidbody;

    // Getter pour isDrifting
    public bool isDrifting
    {
        get { return _isDrifting; }
    }

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        
        // Réduire la masse pour une sensation plus légère
        if (carRigidbody != null)
        {
            // carRigidbody.mass = 500f; // Masse réduite
            carRigidbody.linearDamping = 0.2f; // Moins de résistance à l'air
            carRigidbody.angularDamping = 0.5f; // Moins de résistance aux rotations
            carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0.1f); // Centre de masse très bas
        }
        
        // Configurer les roues pour un style arcade
        ConfigureWheelColliders();
    }

    private void ConfigureWheelColliders()
    {
        // Configurer les roues avant pour une meilleure adhérence
        ConfigureWheelForGrip(frontLeftWheelCollider, gripFactor * 1.2f);
        ConfigureWheelForGrip(frontRightWheelCollider, gripFactor * 1.2f);
        
        // Configurer les roues arrière pour permettre plus de drift
        ConfigureWheelForGrip(rearLeftWheelCollider, gripFactor * driftFactor);
        ConfigureWheelForGrip(rearRightWheelCollider, gripFactor * driftFactor);
    }
    
    private void ConfigureWheelForGrip(WheelCollider wheel, float stiffness)
    {
        // Adhérence avant/arrière
        WheelFrictionCurve fwdFriction = wheel.forwardFriction;
        fwdFriction.stiffness = stiffness * 2.0f;
        wheel.forwardFriction = fwdFriction;
        
        // Adhérence latérale (contrôle le drift)
        WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
        sideFriction.stiffness = stiffness;
        wheel.sidewaysFriction = sideFriction;
        
        // Suspensions plus rigides
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = 50000f;
        spring.damper = 4500f;
        wheel.suspensionSpring = spring;
        wheel.suspensionDistance = 0.1f; // Course de suspension réduite
    }

    private void FixedUpdate() 
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        ApplyAdditionalGravity();
        UpdateUI();
        CheckDrifting();
        
        // Réajuster dynamiquement l'adhérence en fonction de la direction
        AdjustGripDuringTurning();
    }
    
    private void AdjustGripDuringTurning()
    {
        // Réduire l'adhérence des roues arrière pendant les virages pour favoriser le drift contrôlé
        if (Mathf.Abs(horizontalInput) > 0.1f && currentSpeed > 30f)
        {
            float turnGrip = gripFactor * driftFactor * turnGripFactor;
            ConfigureWheelForGrip(rearLeftWheelCollider, turnGrip);
            ConfigureWheelForGrip(rearRightWheelCollider, turnGrip);
        }
        else
        {
            // Revenir à l'adhérence normale quand on ne tourne pas
            ConfigureWheelForGrip(rearLeftWheelCollider, gripFactor * driftFactor);
            ConfigureWheelForGrip(rearRightWheelCollider, gripFactor * driftFactor);
        }
    }

    private void CheckDrifting()
    {
        // Calcule l'angle entre la direction du véhicule et sa vélocité
        if (carRigidbody.linearVelocity.magnitude > 5f) // Seulement si la voiture bouge assez vite
        {
            Vector3 forward = transform.forward;
            Vector3 velocity = carRigidbody.linearVelocity.normalized;
            float angle = Vector3.Angle(forward, velocity);
            
            // Si l'angle est supérieur au seuil et que la vitesse est suffisante, la voiture drift
            _isDrifting = angle > driftAngleThreshold && Mathf.Abs(horizontalInput) > 0.1f;
        }
        else
        {
            _isDrifting = false;
        }
    }

    private void ApplyAdditionalGravity()
    {
        // Ajoute une force vers le bas pour garder la voiture collée au sol
        carRigidbody.AddForce(Vector3.down * additionalGravity * carRigidbody.mass);
    }

    private void GetInput() 
    {
        // Direction - réponse plus directe
        horizontalInput = Input.GetAxis("Horizontal");

        // Accélération - réponse plus directe
        verticalInput = Input.GetAxis("Vertical");

        // Freinage standard
        isBraking = Input.GetKey(KeyCode.Space);
        
        // Calcul de la vitesse actuelle en km/h
        currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;
    }

    private void HandleMotor() 
    {
        float motorTorque = 0f;
        
        if (verticalInput > 0)
        {
            // Accélération beaucoup plus instantanée
            motorTorque = verticalInput * maxMotorForce;
            
            // Limiter la vitesse maximale
            if (currentSpeed >= maxSpeed)
            {
                motorTorque = 0;
            }
        }
        else if (verticalInput < 0)
        {
            // Marche arrière rapide
            motorTorque = verticalInput * maxMotorForce * 0.7f;
        }
        
        // Appliquer le couple moteur à toutes les roues pour une traction intégrale
        frontLeftWheelCollider.motorTorque = motorTorque;
        frontRightWheelCollider.motorTorque = motorTorque;
        rearLeftWheelCollider.motorTorque = motorTorque;
        rearRightWheelCollider.motorTorque = motorTorque;
        
        // Gérer le freinage
        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking() 
    {
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering() 
    {
        // Direction très directe et réactive, indépendante de la vitesse
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() 
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) 
    {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
    
    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = Mathf.Round(currentSpeed).ToString() + " km/h";
        }
    }
    
    // Fonction pour un boost de vitesse temporaire (style arcade)
    public void ApplyBoost(float boostForce, float duration)
    {
        StartCoroutine(BoostCoroutine(boostForce, duration));
    }
    
    private IEnumerator BoostCoroutine(float boostForce, float duration)
    {
        float originalMaxMotorForce = maxMotorForce;
        maxMotorForce += boostForce;
        yield return new WaitForSeconds(duration);
        maxMotorForce = originalMaxMotorForce;
    }
    
    // Méthode publique pour ajuster l'adhérence en temps réel (peut être appelée depuis un slider UI)
    public void SetGripFactor(float value)
    {
        gripFactor = Mathf.Clamp(value, 0.1f, 2.0f);
        ConfigureWheelColliders();
    }
    
    // Méthode publique pour ajuster le facteur de drift en temps réel
    public void SetDriftFactor(float value)
    {
        driftFactor = Mathf.Clamp(value, 0.2f, 2.0f);
        ConfigureWheelColliders();
    }
    
    // Méthode publique pour ajuster le seuil de détection du drift
    public void SetDriftAngleThreshold(float value)
    {
        driftAngleThreshold = Mathf.Clamp(value, 5f, 50f);
    }
    
    // Méthode publique pour ajuster l'adhérence dans les virages
    public void SetTurnGripFactor(float value)
    {
        turnGripFactor = Mathf.Clamp(value, 0.1f, 1.0f);
    }
}