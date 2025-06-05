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

        [Header("Configuration")] public CarConfig config;

        [Header("Physics")] [SerializeField] private Transform centerOfMassTransform;
        [SerializeField] private Vector3 defaultCenterOfMass = new Vector3(0, -0.5f, 0.1f);

        [Header("Car Components")] [SerializeField]
        private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;

        [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
        [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
        [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;
        [SerializeField] private TextMeshProUGUI speedText;

        [Header("Jump Settings")] [SerializeField]
        private float jumpForce = 10f;

        [SerializeField] private float jumpCooldown = 2f;
        [SerializeField] private KeyCode _jumpKey = KeyCode.J;

        [Header("Collision Settings")] [SerializeField]
        private float collisionDragMultiplier = 3f;

        [SerializeField] private float collisionRecoveryTime = 0.5f;
        [SerializeField] private float minCollisionSpeed = 5f;
        [SerializeField] private float velocityReductionFactor = 0.5f;

        private Rigidbody _carRigidbody;
        private bool _hasBattery = true;
        private bool _canJump = true;
        private bool _isRecoveringFromCollision;
        private float _originalDrag;
        private Vector3 _lastCollisionNormal;
        private float _collisionRecoveryTimer;

        public bool IsDrifting => _isDrifting;
        public bool CanJump => _canJump;

        private void Start()
        {
            _carRigidbody = GetComponent<Rigidbody>();
            _originalDrag = _carRigidbody.linearDamping;
            InitializeRigidbody();
            ConfigureWheelColliders();
        }

        private void InitializeRigidbody()
        {
            if (_carRigidbody != null)
            {
                _carRigidbody.linearDamping = config.rigidbodySettings.linearDamping;
                _carRigidbody.angularDamping = config.rigidbodySettings.angularDamping;

                if (centerOfMassTransform != null)
                {
                    _carRigidbody.centerOfMass = transform.InverseTransformPoint(centerOfMassTransform.position);
                }
                else
                {
                    _carRigidbody.centerOfMass = defaultCenterOfMass;
                }
            }
        }

        private void ConfigureWheelColliders()
        {
            ConfigureWheelForGrip(frontLeftWheelCollider, config.wheelSettings.frontGripMultiplier);
            ConfigureWheelForGrip(frontRightWheelCollider, config.wheelSettings.frontGripMultiplier);
            ConfigureWheelForGrip(rearLeftWheelCollider, config.wheelSettings.rearGripMultiplier);
            ConfigureWheelForGrip(rearRightWheelCollider, config.wheelSettings.rearGripMultiplier);
        }

        private void ConfigureWheelForGrip(WheelCollider wheel, float stiffnessMultiplier)
        {
            WheelFrictionCurve fwdFriction = wheel.forwardFriction;
            fwdFriction.stiffness = config.wheelSettings.baseStiffness * stiffnessMultiplier;
            wheel.forwardFriction = fwdFriction;

            WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
            sideFriction.stiffness = config.wheelSettings.baseStiffness * stiffnessMultiplier * 0.5f;
            wheel.sidewaysFriction = sideFriction;

            JointSpring spring = wheel.suspensionSpring;
            spring.spring = config.suspensionSettings.springForce;
            spring.damper = config.suspensionSettings.damperForce;
            wheel.suspensionSpring = spring;
            wheel.suspensionDistance = config.suspensionSettings.suspensionDistance;
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
            HandleJump();
            HandleCollisionRecovery();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_carRigidbody.linearVelocity.magnitude > minCollisionSpeed)
            {
                HandleCollision(collision);
            }
        }

        private void HandleCollision(Collision collision)
        {
            _lastCollisionNormal = collision.contacts[0].normal;
            Vector3 velocityAlongNormal = Vector3.Project(_carRigidbody.linearVelocity, _lastCollisionNormal);
            _carRigidbody.linearVelocity -= velocityAlongNormal * velocityReductionFactor;
            _carRigidbody.linearDamping = _originalDrag * collisionDragMultiplier;
            ReduceWheelForcesAfterCollision();
            _isRecoveringFromCollision = true;
            _collisionRecoveryTimer = collisionRecoveryTime;
        }

        private void HandleCollisionRecovery()
        {
            if (_isRecoveringFromCollision)
            {
                _collisionRecoveryTimer -= Time.fixedDeltaTime;

                if (_collisionRecoveryTimer <= 0)
                {
                    _isRecoveringFromCollision = false;
                    _carRigidbody.linearDamping = _originalDrag;
                    ConfigureWheelColliders();
                }
            }
        }

        private void ReduceWheelForcesAfterCollision()
        {
            WheelCollider[] wheels =
            {
                frontLeftWheelCollider,
                frontRightWheelCollider,
                rearLeftWheelCollider,
                rearRightWheelCollider
            };

            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0;

                WheelFrictionCurve fwdFriction = wheel.forwardFriction;
                fwdFriction.stiffness *= collisionDragMultiplier;
                wheel.forwardFriction = fwdFriction;

                WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
                sideFriction.stiffness *= collisionDragMultiplier;
                wheel.sidewaysFriction = sideFriction;
            }
        }

        private bool IsGrounded()
        {
            return frontLeftWheelCollider.isGrounded &&
                   frontRightWheelCollider.isGrounded &&
                   rearLeftWheelCollider.isGrounded &&
                   rearRightWheelCollider.isGrounded;
        }

        private void HandleJump()
        {
            if (_hasBattery && _canJump && Input.GetKey(_jumpKey) && IsGrounded())
            {
                _carRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _canJump = false;
                StartCoroutine(JumpCooldown());
            }
        }

        private IEnumerator JumpCooldown()
        {
            yield return new WaitForSeconds(jumpCooldown);
            _canJump = true;
        }

        private void AdjustGripDuringTurning()
        {
            float turnIntensity = Mathf.Abs(_horizontalInput);

            if (_currentSpeed > 5f)
            {
                float baseFactor = 1.0f;
                float minFactor = config.driftSettings.turnGripFactor;
                float speedFactor = Mathf.Clamp01(_currentSpeed / config.motorSettings.maxSpeed);
                float targetFactor = Mathf.Lerp(baseFactor, minFactor, speedFactor * turnIntensity);
                float frontGrip = config.wheelSettings.frontGripMultiplier * Mathf.Lerp(1.0f, 0.9f, turnIntensity);
                float rearGrip = config.wheelSettings.rearGripMultiplier * targetFactor;

                ConfigureWheelForGrip(frontLeftWheelCollider, frontGrip);
                ConfigureWheelForGrip(frontRightWheelCollider, frontGrip);
                ConfigureWheelForGrip(rearLeftWheelCollider, rearGrip);
                ConfigureWheelForGrip(rearRightWheelCollider, rearGrip);
            }
            else
            {
                ConfigureWheelForGrip(frontLeftWheelCollider, config.wheelSettings.frontGripMultiplier);
                ConfigureWheelForGrip(frontRightWheelCollider, config.wheelSettings.frontGripMultiplier);
                ConfigureWheelForGrip(rearLeftWheelCollider, config.wheelSettings.rearGripMultiplier);
                ConfigureWheelForGrip(rearRightWheelCollider, config.wheelSettings.rearGripMultiplier);
            }
        }

        private void CheckDrifting()
        {
            if (_carRigidbody.linearVelocity.magnitude > config.driftSettings.minSpeedForDrift * 0.5f)
            {
                Vector3 forward = transform.forward;
                Vector3 velocity = _carRigidbody.linearVelocity.normalized;
                float angle = Vector3.Angle(forward, velocity);
                _isDrifting = angle > config.driftSettings.driftAngleThreshold && Mathf.Abs(_horizontalInput) > 0.1f;
            }
            else
            {
                _isDrifting = false;
            }
        }

        private void ApplyAdditionalGravity()
        {
            _carRigidbody.AddForce(Vector3.down * (config.rigidbodySettings.additionalGravity * _carRigidbody.mass));
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

            if (!_isRecoveringFromCollision)
            {
                if (_verticalInput > 0)
                {
                    // Calculer le pourcentage de vitesse par rapport à la vitesse max
                    float speedRatio = _currentSpeed / config.motorSettings.maxSpeed;
            
                    // Réduire progressivement le couple moteur à l'approche de la vitesse max
                    float speedFactor = Mathf.Clamp01(1 - speedRatio);
                    motorTorque = _verticalInput * config.motorSettings.maxMotorForce * speedFactor;
                }
                else if (_verticalInput < 0)
                {
                    // Calculer le pourcentage de vitesse par rapport à la vitesse max en marche arrière
                    float reverseSpeed = Mathf.Abs(_currentSpeed);
                    float reverseSpeedRatio = reverseSpeed / (config.motorSettings.maxSpeed * config.motorSettings.reverseMultiplier);
            
                    // Réduire progressivement le couple moteur en marche arrière
                    float reverseSpeedFactor = Mathf.Clamp01(1 - reverseSpeedRatio);
                    motorTorque = _verticalInput * config.motorSettings.maxMotorForce * 
                                  config.motorSettings.reverseMultiplier * reverseSpeedFactor;
                }
            }
            else
            {
                motorTorque *= 0.5f;
            }

            // Appliquer le couple aux roues
            frontLeftWheelCollider.motorTorque = motorTorque;
            frontRightWheelCollider.motorTorque = motorTorque;
            rearLeftWheelCollider.motorTorque = motorTorque;
            rearRightWheelCollider.motorTorque = motorTorque;

            _currentBrakeForce = _isBraking ? config.motorSettings.brakeForce : 0f;
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
            _currentSteerAngle = config.steeringSettings.maxSteerAngle * _horizontalInput;
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

        public void UpdateWheelSettings(WheelSettings newSettings)
        {
            config.wheelSettings = newSettings;
            ConfigureWheelColliders();
        }

        public void UpdateSuspensionSettings(SuspensionSettings newSettings)
        {
            config.suspensionSettings = newSettings;
            ConfigureWheelColliders();
        }

        public void UpdateDriftSettings(DriftSettings newSettings)
        {
            config.driftSettings = newSettings;
        }

        public void UpdateMotorSettings(MotorSettings newSettings)
        {
            config.motorSettings = newSettings;
        }

        public void UpdateSteeringSettings(SteeringSettings newSettings)
        {
            config.steeringSettings = newSettings;
        }

        public void UpdateRigidbodySettings(RigidbodySettings newSettings)
        {
            config.rigidbodySettings = newSettings;
            InitializeRigidbody();
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