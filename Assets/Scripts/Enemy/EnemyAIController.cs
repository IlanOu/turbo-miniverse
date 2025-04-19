using Guns;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(Shooter))]
    public class EnemyAIController : MonoBehaviour
    {
        public float shootingDistance = 5f;

        private EnemyMovement movement;
        private Shooter shooter;

        void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            shooter = GetComponent<Shooter>();
        }
        
        void Update()
        {
            if (movement.DistanceToTarget() > shootingDistance)
            {
                movement.MoveToTarget();
            }
            else
            {
                movement.Stop();

                GameObject targetGo = GameObject.FindWithTag("Player");
                if (targetGo == null) return;

                var targetRb = targetGo.GetComponent<Rigidbody>();

                var configWrapper = GetComponent<IShooterConfig>();
                if (configWrapper == null)
                    Debug.LogError("Le gunPrefab ne contient pas de IShooterConfig !");
                else
                    shooter.config = configWrapper;
                
                shooter.AimAtMovingTarget(targetGo, targetRb);
                shooter.TryShootAt(targetGo);
            }
        }
    }
}