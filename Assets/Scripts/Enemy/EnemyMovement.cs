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
        
        [Header("Movement Settings")]
        public float moveSpeed = 3.5f;
        public float stuckThreshold = 0.05f; // Augmenté pour une meilleure détection
        
        [Header("Debug")]
        public bool showDebugInfo = true; // Activé par défaut
        
        private NavMeshAgent agent;
        private EnemyState currentState;
        private Vector3 startPosition;
        private float stuckTimer = 0f;
        private Vector3 lastPosition;
        private bool needsPathReset = false;
        private float pathResetTimer = 0f;
        private float stateUpdateTimer = 0f;
        
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            
            // Configuration de base du NavMeshAgent
            agent.stoppingDistance = attackRange * 0.8f;
            agent.speed = moveSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.autoBraking = true;
            
            startPosition = transform.position;
            currentState = EnemyState.Patrol;
            lastPosition = transform.position;
        }
        
        void OnEnable()
        {
            // S'assurer que l'agent est bien activé quand l'objet est activé
            if (agent != null && !agent.enabled)
            {
                agent.enabled = true;
            }
        }
        
        void Update()
        {
            // Debug visuel
            if (showDebugInfo)
            {
                Debug.DrawRay(transform.position, agent.velocity.normalized * 2f, Color.blue);
                Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);
            }
            
            // Gestion du reset de chemin si nécessaire
            if (needsPathReset)
            {
                pathResetTimer -= Time.deltaTime;
                if (pathResetTimer <= 0)
                {
                    RestartNavMeshAgent();
                    needsPathReset = false;
                    if (showDebugInfo) Debug.Log("NavMeshAgent restarted");
                }
                return;
            }
            
            // Vérifier si l'agent est bloqué
            CheckIfStuck();
            
            // Mise à jour de l'état moins fréquemment pour éviter les changements trop rapides
            stateUpdateTimer -= Time.deltaTime;
            if (stateUpdateTimer <= 0)
            {
                float distanceToPlayer = playerTarget != null ? 
                    Vector3.Distance(transform.position, playerTarget.position) : Mathf.Infinity;
                
                // Mettre à jour l'état en fonction de la distance
                UpdateState(distanceToPlayer);
                stateUpdateTimer = 0.2f; // Update toutes les 0.2 secondes
            }
            
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
                Debug.Log($"Enemy State: {currentState}, Velocity: {agent.velocity.magnitude:F2}, HasPath: {agent.hasPath}, IsStuck: {needsPathReset}");
            }
        }
        
        private void CheckIfStuck()
        {
            // Si l'agent est censé se déplacer
            if (!agent.isStopped && agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                
                // Si l'agent ne bouge presque pas
                if (distanceMoved < stuckThreshold)
                {
                    stuckTimer += Time.deltaTime;
                    
                    // Si bloqué pendant plus d'une seconde
                    if (stuckTimer > 1.0f)
                    {
                        if (showDebugInfo) Debug.LogWarning("Enemy stuck detected! Resetting path...");
                        // Demander un reset du chemin
                        needsPathReset = true;
                        pathResetTimer = 0.2f;
                        stuckTimer = 0f;
                        
                        // Essayer de "pousser" légèrement l'agent
                        transform.position += Random.insideUnitSphere * 0.1f;
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
            
            // Attendre une frame pour s'assurer que l'agent est bien désactivé
            StartCoroutine(ReenableAgent());
        }
        
        private IEnumerator ReenableAgent()
        {
            yield return null; // Attendre une frame
            
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
            EnemyState previousState = currentState;
            
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
            
            // Si l'état a changé, forcer une mise à jour du comportement
            if (previousState != currentState && showDebugInfo)
            {
                Debug.Log($"State changed from {previousState} to {currentState}");
            }
        }
        
        private void Patrol()
        {
            // Vérifier si l'agent est activé
            if (!agent.enabled) return;
            
            // Si l'agent est arrivé à destination ou n'a pas de destination
            if (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance || !agent.hasPath))
            {
                SetRandomPatrolPoint();
            }
        }
        
        private void SetRandomPatrolPoint()
        {
            // Vérifier si l'agent est activé
            if (!agent.enabled) return;
            
            // Essayer plusieurs fois de trouver un point valide
            for (int i = 0; i < 5; i++)
            {
                // Choisir un point aléatoire pour patrouiller
                Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;
                randomPoint.y = startPosition.y; // Garder la même hauteur
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    agent.isStopped = false;
                    agent.SetDestination(hit.position);
                    if (showDebugInfo) Debug.Log($"New patrol point set at {hit.position}");
                    return;
                }
            }
            
            // Si aucun point n'a été trouvé, revenir au point de départ
            agent.SetDestination(startPosition);
        }
        
        private void FollowTarget()
        {
            // Vérifier si l'agent est activé
            if (!agent.enabled) return;
            
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
                
                // Vérifier périodiquement si on doit sortir de l'état d'attaque
                float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
                if (distanceToPlayer > attackRange * 1.1f) // Petite marge pour éviter les oscillations
                {
                    // Sortir de l'état d'attaque
                    currentState = EnemyState.Follow;
                    agent.isStopped = false;
                }
            }
        }
        
        public float DistanceToTarget()
        {
            if (playerTarget == null) return Mathf.Infinity;
            return Vector3.Distance(transform.position, playerTarget.position);
        }
        
        public void Stop()
        {
            if (agent != null && agent.enabled)
                agent.isStopped = true;
        }
        
        public void Resume()
        {
            if (agent != null && agent.enabled)
                agent.isStopped = false;
        }
        
        public void MoveToTarget()
        {
            if (playerTarget != null && agent != null && agent.enabled)
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
            if (showDebugInfo) Debug.Log("Force reset requested");
        }
        
        // Visualiser les rayons dans l'éditeur
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            if (Application.isPlaying && agent != null && agent.hasPath)
            {
                // Dessiner le chemin du NavMeshAgent
                Gizmos.color = Color.blue;
                Vector3[] corners = agent.path.corners;
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[i + 1]);
                }
            }
        }
    }
}