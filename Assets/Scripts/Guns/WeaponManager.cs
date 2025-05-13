using UnityEngine;

namespace Guns
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Références")]
        [SerializeField] private Transform gunHoldPosition;
        [SerializeField] private GameObject gunPrefab;
        [SerializeField] private string targetTag = "Mob";
        [SerializeField] private float minDist = 50f;
        
        [Header("Configuration")]
        [SerializeField] private GunConfig defaultGunConfig;
        
        private Shooter _currentGun;
        private Transform _currentTarget;
        private Rigidbody currentTargetRb;
        
        // Propriété publique pour définir la configuration du gun
        public GunConfig CurrentGunConfig { get; set; }

        void Start()
        {
            // Utiliser la configuration par défaut si aucune n'est définie
            if (CurrentGunConfig == null)
            {
                CurrentGunConfig = defaultGunConfig;
            }
            
            EquipGun();
        }

        void Update()
        {
            GameObject targetGo = FindClosestTarget();
            if (targetGo == null) return;
            if (_currentGun == null) return;
            
            var targetRb = targetGo.GetComponent<Rigidbody>();

            _currentGun.AimAtMovingTarget(targetGo, targetRb);
            _currentGun.TryShootAt(targetGo);
        }

        GameObject FindClosestTarget()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
            GameObject closest = null;

            foreach (var t in targets)
            {
                float dist = Vector3.Distance(t.transform.position, gunHoldPosition.position);
                if (dist < minDist)
                {
                    closest = t;
                }
            }
            return closest;
        }

        void EquipGun()
        {
            if (_currentGun != null)
                Destroy(_currentGun.gameObject);

            GameObject gunObj = Instantiate(gunPrefab, gunHoldPosition);
            gunObj.transform.localPosition = Vector3.zero;
            gunObj.transform.localRotation = Quaternion.identity;

            _currentGun = gunObj.GetComponent<Shooter>();

            var configWrapper = gunObj.GetComponent<GunConfigWrapper>();
            if (configWrapper == null)
            {
                Debug.LogError("Le gunPrefab ne contient pas de GunConfigWrapper !");
            }
            else
            {
                // Assigner la configuration au wrapper
                configWrapper.config = CurrentGunConfig;
                _currentGun.config = configWrapper;
            }
        }
        
        // Méthode publique pour changer la configuration et réinstancier l'arme
        public void ChangeGunConfig(GunConfig newConfig)
        {
            if (newConfig != null)
            {
                CurrentGunConfig = newConfig;
                EquipGun();
            }
        }
    }
}
