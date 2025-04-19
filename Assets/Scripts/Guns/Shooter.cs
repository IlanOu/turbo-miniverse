namespace Guns
{
    using UnityEngine;

    public class Shooter : MonoBehaviour
    {
        public Transform bulletSpawnPoint;
        public IShooterConfig config;
        public LayerMask obstacleLayerMask; // Ajout pour définir quelles couches sont considérées comme obstacles
        private float nextFireTime;

        public void AimAt(GameObject target)
        {
            if (target == null) return;
            
            Vector3 targetPosition = target.transform.position;
            Vector3? aimDir = BallisticHelper.CalculateBallisticDirection(
                bulletSpawnPoint.position, targetPosition, config.BulletSpeed
            );

            if (aimDir.HasValue)
                bulletSpawnPoint.rotation = Quaternion.LookRotation(aimDir.Value);
            else
                bulletSpawnPoint.LookAt(targetPosition);
        }

        public void AimAtMovingTarget(GameObject target, Rigidbody targetRigidbody)
        {
            if (target == null || targetRigidbody == null) return;
            
            Vector3 targetPosition = target.transform.position;
            Vector3 targetVelocity = targetRigidbody.linearVelocity;
            
            Vector3 futurePos = BallisticHelper.PredictFuturePosition(
                bulletSpawnPoint.position, targetPosition, targetVelocity, config.BulletSpeed
            );

            // On utilise une version surchargée qui prend la position future calculée
            AimAt(futurePos);
        }
        
        // Garde la méthode originale pour compatibilité
        private void AimAt(Vector3 targetPosition)
        {
            Vector3? aimDir = BallisticHelper.CalculateBallisticDirection(
                bulletSpawnPoint.position, targetPosition, config.BulletSpeed
            );

            if (aimDir.HasValue)
                bulletSpawnPoint.rotation = Quaternion.LookRotation(aimDir.Value);
            else
                bulletSpawnPoint.LookAt(targetPosition);
        }

        public bool TryShootAt(GameObject target)
        {
            if (target == null || Time.time < nextFireTime || config == null) return false;
            
            Vector3 targetPosition = target.transform.position;
            
            // Vérification des obstacles
            Vector3 direction = targetPosition - bulletSpawnPoint.position;
            float distance = direction.magnitude;
            
            RaycastHit hit;
            if (Physics.Raycast(bulletSpawnPoint.position, direction.normalized, out hit, distance, obstacleLayerMask))
            {
                // Si le premier objet touché n'est pas la cible, il y a un obstacle
                if (hit.transform.gameObject != target)
                {
                    return false; // Obstacle détecté, ne pas tirer
                }
            }

            GameObject bullet = Instantiate(config.BulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.transform.localScale = Vector3.one * config.BulletSize;
            bullet.transform.LookAt(targetPosition);
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            Vector3? velocity = BallisticHelper.CalculateBallisticVelocity(
                bulletSpawnPoint.position, targetPosition, config.BulletSpeed
            );

            if (rb != null)
            {
                rb.linearVelocity = velocity ?? (bulletSpawnPoint.forward * config.BulletSpeed);
                rb.mass = config.BulletMass > 0 ? config.BulletMass : rb.mass;
            }

            Destroy(bullet, 5f);
            nextFireTime = Time.time + config.FireRate;
            return true;
        }
    }
}