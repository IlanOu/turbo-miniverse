using System.Collections;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentBrakeForce;
    private bool isBraking;
    private float currentSpeed = 0f;
    private bool _isDrifting = false;

    [Header("Configuration")]
    [SerializeField] private CarConfig config;

    [Header("Car Components")]
    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    // UI Elements
    [SerializeField] private TextMeshProUGUI speedText;

    private Rigidbody carRigidbody;

    public bool isDrifting => _isDrifting;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        
        if (carRigidbody != null)
        {
            carRigidbody.linearDamping = 0.2f;
            carRigidbody.angularDamping = 0.5f;
            carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0.1f);
        }
        
        ConfigureWheelColliders();
    }

    private void ConfigureWheelColliders()
    {
        // Configuration pour un style arcade : 
        // Les roues avant bénéficient d'une adhérence renforcée, et l'arrière est plus permissif pour le drift
        ConfigureWheelForGrip(frontLeftWheelCollider, config.gripFactor * 1.2f);
        ConfigureWheelForGrip(frontRightWheelCollider, config.gripFactor * 1.2f);
        ConfigureWheelForGrip(rearLeftWheelCollider, config.gripFactor * config.driftFactor);
        ConfigureWheelForGrip(rearRightWheelCollider, config.gripFactor * config.driftFactor);
    }
    
    private void ConfigureWheelForGrip(WheelCollider wheel, float stiffness)
    {
        WheelFrictionCurve fwdFriction = wheel.forwardFriction;
        fwdFriction.stiffness = stiffness * 2.0f;
        wheel.forwardFriction = fwdFriction;
        
        WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
        sideFriction.stiffness = stiffness;
        wheel.sidewaysFriction = sideFriction;
        
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = 50000f;
        spring.damper = 4500f;
        wheel.suspensionSpring = spring;
        wheel.suspensionDistance = 0.1f;
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
        AdjustGripDuringTurning();
    }
    
    private void AdjustGripDuringTurning()
    {
        if (Mathf.Abs(horizontalInput) > 0.1f && currentSpeed > 30f)
        {
            float turnGrip = config.gripFactor * config.driftFactor * config.turnGripFactor;
            ConfigureWheelForGrip(rearLeftWheelCollider, turnGrip);
            ConfigureWheelForGrip(rearRightWheelCollider, turnGrip);
        }
        else
        {
            ConfigureWheelForGrip(rearLeftWheelCollider, config.gripFactor * config.driftFactor);
            ConfigureWheelForGrip(rearRightWheelCollider, config.gripFactor * config.driftFactor);
        }
    }

    private void CheckDrifting()
    {
        if (carRigidbody.linearVelocity.magnitude > 5f)
        {
            Vector3 forward = transform.forward;
            Vector3 velocity = carRigidbody.linearVelocity.normalized;
            float angle = Vector3.Angle(forward, velocity);
            _isDrifting = angle > config.driftAngleThreshold && Mathf.Abs(horizontalInput) > 0.1f;
        }
        else
        {
            _isDrifting = false;
        }
    }

    private void ApplyAdditionalGravity()
    {
        carRigidbody.AddForce(Vector3.down * config.additionalGravity * carRigidbody.mass);
    }

    private void GetInput() 
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
        currentSpeed = carRigidbody.linearVelocity.magnitude * 3.6f;
    }

    private void HandleMotor() 
    {
        float motorTorque = 0f;
        
        if (verticalInput > 0)
        {
            motorTorque = verticalInput * config.maxMotorForce;
            if (currentSpeed >= config.maxSpeed)
                motorTorque = 0;
        }
        else if (verticalInput < 0)
        {
            motorTorque = verticalInput * config.maxMotorForce * 0.7f;
        }
        
        frontLeftWheelCollider.motorTorque = motorTorque;
        frontRightWheelCollider.motorTorque = motorTorque;
        rearLeftWheelCollider.motorTorque = motorTorque;
        rearRightWheelCollider.motorTorque = motorTorque;
        
        currentBrakeForce = isBraking ? config.brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking() 
    {
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering() 
    {
        currentSteerAngle = config.maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels() 
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) 
    {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
    
    private void UpdateUI()
    {
        if (speedText != null)
        {
            speedText.text = Mathf.Round(currentSpeed).ToString() + " km/h";
        }
    }
    
    public void ApplyBoost(float boostForce, float duration)
    {
        StartCoroutine(BoostCoroutine(boostForce, duration));
    }
    
    private IEnumerator BoostCoroutine(float boostForce, float duration)
    {
        float originalForce = config.maxMotorForce;
        config.maxMotorForce += boostForce;
        yield return new WaitForSeconds(duration);
        config.maxMotorForce = originalForce;
    }
    
    public void SetGripFactor(float value)
    {
        config.gripFactor = Mathf.Clamp(value, 0.1f, 2.0f);
        ConfigureWheelColliders();
    }
    
    public void SetDriftFactor(float value)
    {
        config.driftFactor = Mathf.Clamp(value, 0.2f, 2.0f);
        ConfigureWheelColliders();
    }
    
    public void SetDriftAngleThreshold(float value)
    {
        config.driftAngleThreshold = Mathf.Clamp(value, 5f, 50f);
    }
    
    public void SetTurnGripFactor(float value)
    {
        config.turnGripFactor = Mathf.Clamp(value, 0.1f, 1.0f);
    }
}
