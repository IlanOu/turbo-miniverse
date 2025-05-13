using System.Collections;
using TMPro;
using UnityEngine;

namespace Car
{
    public class CarController : MonoBehaviour
    {
        private float _horizontalInput, _verticalInput;
        private float _currentSteerAngle, _currentBrakeForce;
        private bool _isBraking;
        private float _currentSpeed = 0f;
        private bool _isDrifting = false;

        [Header("Configuration")] 
        public CarConfig config;
        
        [Header("Car Components")]
        // Wheel Colliders
        [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
        [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

        // Wheels
        [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
        [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

        // UI Elements
        [SerializeField] private TextMeshProUGUI speedText;

        private Rigidbody _carRigidbody;

        private bool _hasBattery = true;
    
        public bool IsDrifting => _isDrifting;

        private void Start()
        {
            _carRigidbody = GetComponent<Rigidbody>();
        
            if (_carRigidbody != null)
            {
                _carRigidbody.linearDamping = 0.2f;
                _carRigidbody.angularDamping = 0.5f;
                _carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0.1f);
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
            if (Mathf.Abs(_horizontalInput) > 0.1f && _currentSpeed > 30f)
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
            if (_carRigidbody.linearVelocity.magnitude > 5f)
            {
                Vector3 forward = transform.forward;
                Vector3 velocity = _carRigidbody.linearVelocity.normalized;
                float angle = Vector3.Angle(forward, velocity);
                _isDrifting = angle > config.driftAngleThreshold && Mathf.Abs(_horizontalInput) > 0.1f;
            }
            else
            {
                _isDrifting = false;
            }
        }

        private void ApplyAdditionalGravity()
        {
            _carRigidbody.AddForce(Vector3.down * (config.additionalGravity * _carRigidbody.mass));
        }

        private void GetInput() 
        {
            if (_hasBattery)
            {
                _horizontalInput = Input.GetAxis("Horizontal");
                _verticalInput = Input.GetAxis("Vertical");
                _isBraking = Input.GetKey(KeyCode.Space);
                _currentSpeed = _carRigidbody.linearVelocity.magnitude * 3.6f;
            }
            else
            {
                _horizontalInput = 0f;
                _verticalInput = 0f;
                _isBraking = false;
                _currentSpeed = 0f;
            }
        }
    
        public bool IsAccelerating()
        {
            return _hasBattery && Mathf.Abs(_verticalInput) > 0.1f;
        }


        private void HandleMotor() 
        {
            float motorTorque = 0f;
        
            if (_verticalInput > 0)
            {
                motorTorque = _verticalInput * config.maxMotorForce;
                if (_currentSpeed >= config.maxSpeed)
                    motorTorque = 0;
            }
            else if (_verticalInput < 0)
            {
                motorTorque = _verticalInput * config.maxMotorForce * 0.7f;
            }
        
            frontLeftWheelCollider.motorTorque = motorTorque;
            frontRightWheelCollider.motorTorque = motorTorque;
            rearLeftWheelCollider.motorTorque = motorTorque;
            rearRightWheelCollider.motorTorque = motorTorque;
        
            _currentBrakeForce = _isBraking ? config.brakeForce : 0f;
            ApplyBraking();
        }

        private void ApplyBraking() 
        {
            frontLeftWheelCollider.brakeTorque = _currentBrakeForce;
            frontRightWheelCollider.brakeTorque = _currentBrakeForce;
            rearLeftWheelCollider.brakeTorque = _currentBrakeForce;
            rearRightWheelCollider.brakeTorque = _currentBrakeForce;
        }

        private void HandleSteering() 
        {
            _currentSteerAngle = config.maxSteerAngle * _horizontalInput;
            frontLeftWheelCollider.steerAngle = _currentSteerAngle;
            frontRightWheelCollider.steerAngle = _currentSteerAngle;
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
            if (speedText)
            {
                speedText.text = Mathf.Round(_currentSpeed).ToString() + " km/h";
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
    
        public void StopCar()
        {
            _hasBattery = false;
        }

        public void StartCar()
        {
            _hasBattery = true;
        }
    }
}
