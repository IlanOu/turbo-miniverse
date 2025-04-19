using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        public enum EnemyState { Patrol, Follow, Attack }
        
        [Header("Target Settings")]
        public Transform playerTarget;
        public float playerDetectionRange = 15f;
        public float attackRange = 5f;
        public float losePlayerRange = 20f;

        [Header("Patrol Settings")]
        public float patrolRadius = 10f;
        
        [Header("Debug")]
        public bool showDebugInfo = false;
        
        private NavMeshAgent agent;
        private EnemyState currentState;
        private Vector3 startPosition;
        private float stuckTimer = 0f;
        private Vector3 lastPosition;
        private bool needsPathReset = false;
        private float pathResetTimer = 0f;
        
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            
            // Désactiver le composant Rigidbody s'il existe
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Conserver le Rigidbody uniquement pour les collisions
                rb.isKinematic = true;
                rb.detectCollisions = true;
            }
            
            agent.stoppingDistance = attackRange * 0.8f;
            agent.speed = 3.5f;
            
            startPosition = transform.position;
            currentState = EnemyState.Patrol;
            lastPosition = transform.position;
        }
        
        void Update()
        {
            // Gestion du reset de chemin si nécessaire
            if (needsPathReset)
            {
                pathResetTimer -= Time.deltaTime;
                if (pathResetTimer <= 0)
                {
                    RestartNavMeshAgent();
                    needsPathReset = false;
                }
                return;
            }
            
            // Vérifier si l'agent est bloqué
            CheckIfStuck();
            
            // Vérifier la distance au joueur
            float distanceToPlayer = playerTarget != null ? 
                Vector3.Distance(transform.position, playerTarget.position) : Mathf.Infinity;
            
            // Mettre à jour l'état en fonction de la distance
            UpdateState(distanceToPlayer);
            
            // Exécuter le comportement correspondant à l'état actuel
            switch (currentState)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Follow:
                    FollowTarget();
                    break;
                case EnemyState.Attack:
                    AttackTarget();
                    break;
            }
            
            // Afficher des informations de débogage
            if (showDebugInfo)
            {
                Debug.Log($"Enemy State: {currentState}, Distance to player: {distanceToPlayer}, IsStuck: {needsPathReset}");
            }
        }
        
        private void CheckIfStuck()
        {
            // Si l'agent est censé se déplacer
            if (!agent.isStopped && agent.hasPath)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                
                // Si l'agent ne bouge presque pas
                if (distanceMoved < 0.01f)
                {
                    stuckTimer += Time.deltaTime;
                    
                    // Si bloqué pendant plus d'une seconde
                    if (stuckTimer > 1.0f)
                    {
                        // Demander un reset du chemin
                        needsPathReset = true;
                        pathResetTimer = 0.2f;
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    // Réinitialiser le timer si l'agent bouge
                    stuckTimer = 0f;
                }
            }
            
            lastPosition = transform.position;
        }
        
        private void RestartNavMeshAgent()
        {
            // Désactiver puis réactiver l'agent
            agent.enabled = false;
            agent.enabled = true;
            
            // Recalculer le chemin en fonction de l'état
            if (currentState == EnemyState.Follow && playerTarget != null)
            {
                agent.SetDestination(playerTarget.position);
            }
            else if (currentState == EnemyState.Patrol)
            {
                SetRandomPatrolPoint();
            }
        }
        
        private void UpdateState(float distanceToPlayer)
        {
            if (playerTarget != null)
            {
                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attack;
                }
                else if (distanceToPlayer <= playerDetectionRange)
                {
                    currentState = EnemyState.Follow;
                }
                else if (distanceToPlayer > losePlayerRange)
                {
                    currentState = EnemyState.Patrol;
                }
            }
            else
            {
                currentState = EnemyState.Patrol;
            }
        }
        
        private void Patrol()
        {
            // Si l'agent est arrivé à destination ou n'a pas de destination
            if (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance || !agent.hasPath))
            {
                SetRandomPatrolPoint();
            }
        }
        
        private void SetRandomPatrolPoint()
        {
            // Choisir un point aléatoire pour patrouiller
            Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
        
        private void FollowTarget()
        {
            if (playerTarget != null)
            {
                // Mettre à jour la destination toutes les frames pour suivre le joueur
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
            }
        }
        
        private void AttackTarget()
        {
            if (playerTarget != null)
            {
                // S'arrêter pour attaquer
                agent.isStopped = true;
                
                // Regarder vers la cible horizontalement
                Vector3 lookPos = playerTarget.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }
        }
        
        public float DistanceToTarget()
        {
            if (playerTarget == null) return Mathf.Infinity;
            return Vector3.Distance(transform.position, playerTarget.position);
        }
        
        public void Stop()
        {
            if (agent != null)
                agent.isStopped = true;
        }
        
        public void MoveToTarget()
        {
            if (playerTarget != null)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
            }
        }
        
        // Forcer un reset complet
        public void ForceReset()
        {
            needsPathReset = true;
            pathResetTimer = 0.1f;
        }
        
        // Visualiser les rayons dans l'éditeur
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}