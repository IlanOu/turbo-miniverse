using Guns;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(Shooter))]
    public class EnemyAIController : MonoBehaviour
    {
        private EnemyMovement movement;
        private Shooter shooter;
        private GameObject playerTarget;

        void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            shooter = GetComponent<Shooter>();
            
            // Trouver le joueur et le définir comme cible
            playerTarget = GameObject.FindWithTag("Player");
            if (playerTarget != null)
            {
                movement.playerTarget = playerTarget.transform;
            }
            
            // S'assurer que le Shooter a une configuration
            var configWrapper = GetComponent<IShooterConfig>();
            if (configWrapper == null)
                Debug.LogError("Le gunPrefab ne contient pas de IShooterConfig !");
            else
                shooter.config = configWrapper;
        }
        
        void Update()
        {
            // Si le joueur n'a pas été trouvé, essayer à nouveau
            if (playerTarget == null)
            {
                playerTarget = GameObject.FindWithTag("Player");
                if (playerTarget != null)
                {
                    movement.playerTarget = playerTarget.transform;
                }
                return;
            }
            
            // Si l'ennemi est en état d'attaque (à portée de tir)
            if (movement.DistanceToTarget() <= movement.attackRange)
            {
                // S'assurer que l'ennemi s'arrête pour tirer
                movement.Stop();
                
                var targetRb = playerTarget.GetComponent<Rigidbody>();
                
                // Viser le joueur en tenant compte de son mouvement
                if (targetRb != null)
                {
                    shooter.AimAtMovingTarget(playerTarget, targetRb);
                }
                else
                {
                    shooter.AimAt(playerTarget);
                }
                
                shooter.TryShootAt(playerTarget);
            }
        }
    }
}