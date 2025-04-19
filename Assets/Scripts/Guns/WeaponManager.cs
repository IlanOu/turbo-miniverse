using UnityEngine;

namespace Guns
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Références")]
        [SerializeField] private Transform gunHoldPosition;
        [SerializeField] private GameObject gunPrefab;
        [SerializeField] private string targetTag = "Mob";

        private Shooter _currentGun;
        private Transform _currentTarget;
        private Rigidbody currentTargetRb;

        void Start()
        {
            EquipGun();
        }

        void Update()
        {
            GameObject targetGo = FindClosestTarget();
            if (targetGo == null) return;
            if (_currentGun == null) return;
            
            var targetPos = targetGo.transform.position;
            var targetRb = targetGo.GetComponent<Rigidbody>();

            _currentGun.AimAtMovingTarget(targetPos, targetRb.linearVelocity);
            _currentGun.TryShootAt(targetPos);
        }

        GameObject FindClosestTarget()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
            GameObject closest = null;
            float minDist = Mathf.Infinity;

            foreach (var t in targets)
            {
                float dist = Vector3.SqrMagnitude(t.transform.position - gunHoldPosition.position);
                if (dist < minDist)
                {
                    minDist = dist;
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

            var configWrapper = gunObj.GetComponent<IShooterConfig>();
            if (configWrapper == null)
                Debug.LogError("Le gunPrefab ne contient pas de IShooterConfig !");
            else
                _currentGun.config = configWrapper;
        }

    }
}