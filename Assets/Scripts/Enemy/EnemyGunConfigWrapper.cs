using Guns;
using UnityEngine;

namespace Enemy
{
    public class EnemyGunConfigWrapper : MonoBehaviour, IShooterConfig
    {
        public GunConfig config;

        public GameObject BulletPrefab => config.bulletPrefab;
        public float BulletSpeed => config.bulletSpeed;
        public float BulletSize => config.bulletSize;
        public float BulletMass => config.bulletMass;
        public float FireRate => config.fireRate;
    }
}