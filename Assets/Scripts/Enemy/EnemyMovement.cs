using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        public Transform target;
        public float stoppingDistance = 5f;

        private NavMeshAgent agent;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.stoppingDistance = stoppingDistance;
        }

        public void MoveToTarget()
        {
            if (target != null)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);

                // Regarder horizontalement
                Vector3 lookPos = target.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
            }
        }

        public void Stop()
        {
            if (agent != null)
                agent.isStopped = true;
        }

        public float DistanceToTarget()
        {
            if (target == null) return Mathf.Infinity;
            return Vector3.Distance(transform.position, target.position);
        }
    }
}