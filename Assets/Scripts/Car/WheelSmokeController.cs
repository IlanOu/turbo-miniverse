using UnityEngine;

public class WheelSmokeController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private ParticleSystem[] wheelSmokeParticles;
    [SerializeField] private WheelCollider[] wheelColliders;

    [Header("Paramètres de fumée")]
    [SerializeField] private float minSpeedToEmit = 5f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float maxEmissionRate = 50f;

    private ParticleSystem.EmissionModule[] emissions;
    private WheelHit wheelHit;

    void Start()
    {
        emissions = new ParticleSystem.EmissionModule[wheelSmokeParticles.Length];
        for (int i = 0; i < wheelSmokeParticles.Length; i++)
        {
            emissions[i] = wheelSmokeParticles[i].emission;
        }
    }

    void Update()
    {
        float speed = carRigidbody.linearVelocity.magnitude;
        float speedRatio = Mathf.InverseLerp(minSpeedToEmit, maxSpeed, speed);

        for (int i = 0; i < wheelSmokeParticles.Length; i++)
        {
            bool isGrounded = wheelColliders[i].GetGroundHit(out wheelHit);

            if (isGrounded && speed >= minSpeedToEmit)
            {
                emissions[i].rateOverTime = Mathf.Lerp(0f, maxEmissionRate, speedRatio);
                if (!wheelSmokeParticles[i].isPlaying)
                    wheelSmokeParticles[i].Play();
            }
            else
            {
                emissions[i].rateOverTime = 0f;
                if (wheelSmokeParticles[i].isPlaying)
                    wheelSmokeParticles[i].Stop();
            }
        }
    }
}