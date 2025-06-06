using UnityEngine;

[System.Serializable]
public class MotorSettings
{
    [Tooltip("Force maximale du moteur")] public float maxMotorForce = 3000f;

    [Tooltip("Force de freinage")] public float brakeForce = 5000f;

    [Tooltip("Vitesse maximale en km/h")] public float maxSpeed = 200f;

    [Tooltip("Multiplicateur pour la marche arrière (0-1)")] [Range(0.1f, 1.0f)]
    public float reverseMultiplier = 0.7f;

    [Tooltip("Courbe d'accélération (0-1)")] [Range(0.1f, 1.0f)]
    public float accelerationCurve = 0.5f;
}

[System.Serializable]
public class SteeringSettings
{
    [Tooltip("Angle maximum de braquage des roues")] [Range(20f, 60f)]
    public float maxSteerAngle = 45f;

    [Tooltip("Vitesse de retour du volant au centre")] [Range(1f, 10f)]
    public float steeringReturnSpeed = 5f;
}

[System.Serializable]
public class WheelSettings
{
    [Tooltip("Rigidité de base pour toutes les roues")] [Range(0.5f, 2.0f)]
    public float baseStiffness = 1.0f;

    [Tooltip("Multiplicateur de grip pour les roues avant")] [Range(0.5f, 2.0f)]
    public float frontGripMultiplier = 1.2f;

    [Tooltip("Multiplicateur de grip pour les roues arrière")] [Range(0.5f, 2.0f)]
    public float rearGripMultiplier = 1.0f;
}

[System.Serializable]
public class SuspensionSettings
{
    [Tooltip("Force du ressort de suspension")] [Range(20000f, 70000f)]
    public float springForce = 50000f;

    [Tooltip("Force d'amortissement")] [Range(2000f, 7000f)]
    public float damperForce = 4500f;

    [Tooltip("Distance de compression maximale de la suspension")] [Range(0.05f, 0.3f)]
    public float suspensionDistance = 0.1f;
}

[System.Serializable]
public class DriftSettings
{
    [Tooltip("Seuil d'angle pour considérer que la voiture est en drift")] [Range(5f, 50f)]
    public float driftAngleThreshold = 15f;

    [Tooltip("Facteur de grip pendant les virages")] [Range(0.1f, 1.0f)]
    public float turnGripFactor = 0.5f;

    [Tooltip("Vitesse minimale pour activer le drift (km/h)")] [Range(10f, 50f)]
    public float minSpeedForDrift = 30f;

    [Tooltip("Facteur de contrôle pendant le drift")] [Range(0.1f, 1.0f)]
    public float driftControlFactor = 0.8f;
}

[System.Serializable]
public class RigidbodySettings
{
    [Tooltip("Amortissement linéaire du Rigidbody")] [Range(0.0f, 1.0f)]
    public float linearDamping = 0.2f;

    [Tooltip("Amortissement angulaire du Rigidbody")] [Range(0.0f, 1.0f)]
    public float angularDamping = 0.5f;

    [Tooltip("Gravité additionnelle appliquée à la voiture")] [Range(0f, 50f)]
    public float additionalGravity = 20f;
}

[CreateAssetMenu(fileName = "CarConfig", menuName = "Car/Configuration", order = 1)]
public class CarConfig : ScriptableObject
{
    [Header("Paramètres du moteur")] public MotorSettings motorSettings = new MotorSettings();

    [Header("Paramètres de direction")] public SteeringSettings steeringSettings = new SteeringSettings();

    [Header("Paramètres des roues")] public WheelSettings wheelSettings = new WheelSettings();

    [Header("Paramètres de suspension")] public SuspensionSettings suspensionSettings = new SuspensionSettings();

    [Header("Paramètres de drift")] public DriftSettings driftSettings = new DriftSettings();

    [Header("Paramètres du Rigidbody")] public RigidbodySettings rigidbodySettings = new RigidbodySettings();

    // Méthodes utilitaires pour créer des configurations prédéfinies

    public static CarConfig CreateSportConfig()
    {
        CarConfig config = CreateInstance<CarConfig>();

        // Configuration moteur sportive - puissante et réactive
        config.motorSettings.maxMotorForce = 4500f;
        config.motorSettings.brakeForce = 6000f;
        config.motorSettings.maxSpeed = 200f;
        config.motorSettings.reverseMultiplier = 0.75f;
        config.motorSettings.accelerationCurve = 0.4f;

        // Direction précise et réactive
        config.steeringSettings.maxSteerAngle = 20f;
        config.steeringSettings.steeringReturnSpeed = 6f;

        // Adhérence sportive - équilibrée mais ferme
        config.wheelSettings.baseStiffness = 1.3f;
        config.wheelSettings.frontGripMultiplier = 1.8f;
        config.wheelSettings.rearGripMultiplier = 2f;

        // Suspension ferme pour meilleure tenue de route
        config.suspensionSettings.springForce = 55000f;
        config.suspensionSettings.damperForce = 4800f;
        config.suspensionSettings.suspensionDistance = 0.15f;

        // Drift contrôlé mais possible
        config.driftSettings.driftAngleThreshold = 12f;
        config.driftSettings.turnGripFactor = 0.55f;
        config.driftSettings.minSpeedForDrift = 40f;
        config.driftSettings.driftControlFactor = 0.85f;

        // Rigidbody stable avec bonne tenue de route
        config.rigidbodySettings.linearDamping = 0.2f;
        config.rigidbodySettings.angularDamping = 0.5f;
        config.rigidbodySettings.additionalGravity = 22f;

        return config;
    }

    public static CarConfig CreateDriftConfig()
    {
        CarConfig config = CreateInstance<CarConfig>();

        // Moteur puissant mais contrôlable
        config.motorSettings.maxMotorForce = 2000f;
        config.motorSettings.brakeForce = 5500f;
        config.motorSettings.maxSpeed = 180f;
        config.motorSettings.reverseMultiplier = 0.65f;
        config.motorSettings.accelerationCurve = 0.2f;

        // Direction très réactive pour contrôler les drifts
        config.steeringSettings.maxSteerAngle = 20f;
        config.steeringSettings.steeringReturnSpeed = 6f;

        // Roues avec forte adhérence avant mais faible adhérence arrière
        config.wheelSettings.baseStiffness = 1.3f;
        config.wheelSettings.frontGripMultiplier = 1.8f;
        config.wheelSettings.rearGripMultiplier = 2f;

        // Suspension intermédiaire
        config.suspensionSettings.springForce = 55000f;
        config.suspensionSettings.damperForce = 4800f;
        config.suspensionSettings.suspensionDistance = 0.15f;

        // Drift très facile à déclencher et à maintenir
        config.driftSettings.driftAngleThreshold = 12f;
        config.driftSettings.turnGripFactor = 0.55f;
        config.driftSettings.minSpeedForDrift = 40f;
        config.driftSettings.driftControlFactor = 0.1f;

        // Rigidbody avec moins de stabilité
        config.rigidbodySettings.linearDamping = 0.2f;
        config.rigidbodySettings.angularDamping = 0.5f;
        config.rigidbodySettings.additionalGravity = 22f;

        return config;
    }

    public static CarConfig CreateOffRoadConfig()
    {
        CarConfig config = CreateInstance<CarConfig>();
    
        // Moteur puissant mais progressif
        config.motorSettings.maxMotorForce = 4500f; // Force réduite pour plus de contrôle
        config.motorSettings.brakeForce = 5500f;
        config.motorSettings.maxSpeed = 90f;
        config.motorSettings.reverseMultiplier = 0.7f;
        config.motorSettings.accelerationCurve = 0.3f; // Plus réactif
    
        // Direction plus directe et prévisible
        config.steeringSettings.maxSteerAngle = 35f; // Angle plus généreux pour mieux tourner
        config.steeringSettings.steeringReturnSpeed = 5f; // Retour au centre plus rapide
    
        // Adhérence équilibrée - valeurs modérées
        config.wheelSettings.baseStiffness = 1.4f; // Rigidité modérée
        config.wheelSettings.frontGripMultiplier = 1.18f; // Adhérence avant bonne mais pas excessive
        config.wheelSettings.rearGripMultiplier = 1.735f; // Adhérence arrière légèrement inférieure
    
        // Suspension équilibrée
        config.suspensionSettings.springForce = 35000f; // Ressort plus souple
        config.suspensionSettings.damperForce = 3500f; // Amortissement modéré
        config.suspensionSettings.suspensionDistance = 0.18f; // Course de suspension généreuse
    
        // Paramètres de drift équilibrés
        config.driftSettings.driftAngleThreshold = 20f; // Seuil raisonnable
        config.driftSettings.turnGripFactor = 0.65f; // Valeur critique pour la maniabilité
        config.driftSettings.minSpeedForDrift = 35f; // Vitesse raisonnable
        config.driftSettings.driftControlFactor = 0.8f; // Bon contrôle
    
        // Rigidbody équilibré
        config.rigidbodySettings.linearDamping = 0.25f; // Amortissement modéré
        config.rigidbodySettings.angularDamping = 0.5f; // Rotation plus libre
        config.rigidbodySettings.additionalGravity = 15f; // Gravité réduite
    
        return config;
    }




    // Ajout d'une nouvelle configuration de type Rally
    public static CarConfig CreateRallyConfig()
    {
        CarConfig config = CreateInstance<CarConfig>();

        // Moteur puissant mais équilibré
        config.motorSettings.maxMotorForce = 5000f;
        config.motorSettings.brakeForce = 5800f;
        config.motorSettings.maxSpeed = 200f;
        config.motorSettings.reverseMultiplier = 0.75f;
        config.motorSettings.accelerationCurve = 0.65f;

        // Direction réactive mais pas excessive
        config.steeringSettings.maxSteerAngle = 42f;
        config.steeringSettings.steeringReturnSpeed = 6f;

        // Adhérence équilibrée pour tous terrains
        config.wheelSettings.baseStiffness = 1.3f;
        config.wheelSettings.frontGripMultiplier = 1.4f;
        config.wheelSettings.rearGripMultiplier = 1.4f;

        // Suspension intermédiaire - ferme mais absorbante
        config.suspensionSettings.springForce = 55000f;
        config.suspensionSettings.damperForce = 4800f;
        config.suspensionSettings.suspensionDistance = 0.15f;

        // Drift contrôlable - bon pour les dérapages contrôlés
        config.driftSettings.driftAngleThreshold = 12f;
        config.driftSettings.turnGripFactor = 0.55f;
        config.driftSettings.minSpeedForDrift = 35f;
        config.driftSettings.driftControlFactor = 0.85f;

        // Rigidbody équilibré
        config.rigidbodySettings.linearDamping = 0.2f;
        config.rigidbodySettings.angularDamping = 0.5f;
        config.rigidbodySettings.additionalGravity = 22f;

        return config;
    }
}