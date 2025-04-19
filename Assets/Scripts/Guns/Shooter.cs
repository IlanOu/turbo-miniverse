namespace Guns
{
    using UnityEngine;

    public class Shooter : MonoBehaviour
    {
        public Transform bulletSpawnPoint;
        public IShooterConfig config;
        private float nextFireTime;

        public void AimAt(Vector3 targetPosition)
        {
            Vector3? aimDir = BallisticHelper.CalculateBallisticDirection(
                bulletSpawnPoint.position, targetPosition, config.BulletSpeed
            );

            if (aimDir.HasValue)
                bulletSpawnPoint.rotation = Quaternion.LookRotation(aimDir.Value);
            else
                bulletSpawnPoint.LookAt(targetPosition);
        }

        public void AimAtMovingTarget(Vector3 targetPosition, Vector3 targetVelocity)
        {
            Vector3 futurePos = BallisticHelper.PredictFuturePosition(
                bulletSpawnPoint.position, targetPosition, targetVelocity, config.BulletSpeed
            );

            AimAt(futurePos);
        }

        public bool TryShootAt(Vector3 targetPosition)
        {
            if (Time.time < nextFireTime || config == null) return false;

            GameObject bullet = Instantiate(config.BulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.transform.localScale = Vector3.one * config.BulletSize;

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