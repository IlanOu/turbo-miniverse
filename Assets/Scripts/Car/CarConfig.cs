using UnityEngine;

[CreateAssetMenu(fileName = "CarConfig", menuName = "Car/Configuration", order = 1)]
public class CarConfig : ScriptableObject
{
    [Header("Paramètres moteurs et vitesse")]
    public float maxMotorForce = 3000f; // Améliorable
    public float brakeForce = 5000f; // Améliorable
    public float maxSteerAngle = 45f; // Modifiable gratuitement
    public float maxSpeed = 200f; // Améliorable
    public float accelerationCurve = 0.5f;
    public float additionalGravity = 20f; // Améliorable

    [Header("Paramètres de drift")]
    [Range(0.2f, 2.0f)]
    public float driftFactor = 0.8f; // Modifiable gratuitement
    [Range(0.1f, 2.0f)]
    public float gripFactor = 1.0f; // Modifiable gratuitement
    [Range(5f, 50f)]
    public float driftAngleThreshold = 15f; // Modifiable gratuitement
    [Range(0.1f, 1.0f)]
    public float turnGripFactor = 0.5f; // Modifiable gratuitement
}