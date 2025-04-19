using UnityEngine;

namespace Guns
{
    [CreateAssetMenu(fileName = "NewGunConfig", menuName = "Weapons/Gun Config")]
    public class GunConfig : ScriptableObject
    {
        [Header("Gun Stats")]
        public GameObject bulletPrefab;
        public float bulletSpeed = 20f;
        public int damage = 10;
        public float fireRate = 0.2f;
        public float bulletSize = 1f;
    
        public float autoAimRotationSpeed = 5f;
        public float autoFireDistance = 50f;
    
        public float bulletMass = 1f;
    
        [Header("Battery")]
        public float energyCostPerShot = -1f; // -1 = auto-calc
    }
}