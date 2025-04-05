using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Références & Configuration")]
    [SerializeField] private Transform gunHoldPosition;
    [SerializeField] private GameObject[] availableGuns; // Liste des prefabs d'armes disponibles
    [SerializeField] private float autoAimRotationSpeed = 5f; // Vitesse d'orientation automatique
    [SerializeField] private float autoFireDistance = 50f;      // Distance maximale pour tir automatique
    [SerializeField] private float aimHeightOffset = 1.5f;        // Décalage vertical pour viser plus haut
    [SerializeField] private string targetTag = "Mob";            // Tag de l'objet cible

    private Gun currentGun;
    private int currentGunIndex = 0;

    void Start()
    {
        EquipGun(0); // Équiper la première arme par défaut
    }

    void Update()
    {
        // Recherche de la cible la plus proche avec le tag "mob"
        GameObject target = FindClosestTarget();
        if (target != null && currentGun != null)
        {
            // On ajuste la position de la cible en y ajoutant un offset
            Vector3 adjustedTargetPos = target.transform.position + Vector3.up * aimHeightOffset;
            
            // Calcul de la trajectoire balistique pour compenser la gravité
            Vector3 ballisticVelocity = CalculateBallisticVelocity(
                adjustedTargetPos, 
                currentGun.GetBulletSpawnPoint().position, 
                currentGun.BulletSpeed, 
                Mathf.Abs(Physics.gravity.y)
            );
            
            // Déduire la direction à partir de la vélocité balistique
            Vector3 aimDirection = ballisticVelocity.normalized;
            
            // Calculer la rotation cible en fonction de la direction calculée
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            // Interpolation pour une rotation fluide
            gunHoldPosition.rotation = Quaternion.Lerp(gunHoldPosition.rotation, targetRotation, autoAimRotationSpeed * Time.deltaTime);
            
            // Tir automatique si la cible est dans la distance définie
            if (Vector3.Distance(gunHoldPosition.position, target.transform.position) <= autoFireDistance)
            {
                currentGun.TryFire();
            }
        }

        // Changement manuel d'arme
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipGun(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipGun(1);
    }

    /// <summary>
    /// Calcule la vélocité initiale nécessaire pour que le projectile atteigne la cible en tenant compte de la gravité.
    /// </summary>
    /// <param name="target">Position ajustée de la cible</param>
    /// <param name="origin">Position d'origine (spawn de la balle)</param>
    /// <param name="speed">Vitesse initiale du projectile</param>
    /// <param name="gravity">Magnitude de la gravité</param>
    /// <returns>Vélocité initiale sous forme de vecteur</returns>
    Vector3 CalculateBallisticVelocity(Vector3 target, Vector3 origin, float speed, float gravity)
    {
        Vector3 toTarget = target - origin;
        // Séparation en composante horizontale
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float d = toTargetXZ.magnitude; // Distance horizontale
        float y = toTarget.y;           // Différence de hauteur

        float speedSqr = speed * speed;
        // Calcul de la partie sous la racine carrée
        float underSqrt = speedSqr * speedSqr - gravity * (gravity * d * d + 2 * y * speedSqr);
        
        // Si le discriminant est négatif, aucune solution réelle n'existe :
        // On vise directement vers la cible ajustée.
        if (underSqrt < 0)
        {
            return (target - origin).normalized * speed;
        }
        
        float sqrt = Mathf.Sqrt(underSqrt);
        // Calcul de l'angle de tir (solution pour l'angle le plus bas)
        float angle = Mathf.Atan((speedSqr - sqrt) / (gravity * d));
        
        // Combinaison des composantes horizontale et verticale pour obtenir la vélocité initiale
        Vector3 velocity = toTargetXZ.normalized * speed * Mathf.Cos(angle) + Vector3.up * speed * Mathf.Sin(angle);
        return velocity;
    }

    /// <summary>
    /// Recherche et renvoie la cible la plus proche ayant le tag "mob".
    /// </summary>
    GameObject FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = gunHoldPosition.position;

        foreach (GameObject t in targets)
        {
            Vector3 directionToTarget = t.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest = t;
            }
        }
        return closest;
    }

    /// <summary>
    /// Équipe l'arme correspondant à l'index fourni.
    /// </summary>
    public void EquipGun(int index)
    {
        if (index < 0 || index >= availableGuns.Length)
            return;

        if (currentGun != null)
            Destroy(currentGun.gameObject);

        GameObject gunObj = Instantiate(availableGuns[index], gunHoldPosition);
        gunObj.transform.localPosition = Vector3.zero;
        gunObj.transform.localRotation = Quaternion.identity;

        Gun newGun = gunObj.GetComponent<Gun>();

        // On initialise correctement le Gun
        CarBattery battery = GetComponentInParent<CarBattery>();
        if (newGun != null && battery != null)
        {
            // Option A: le GunConfig est déjà sur le prefab
            newGun.Initialize(battery);

            // Option B: si tu veux forcer un config via tableau
            // newGun.Initialize(battery, gunConfigs[index]);
        }

        currentGun = newGun;
    }

}
