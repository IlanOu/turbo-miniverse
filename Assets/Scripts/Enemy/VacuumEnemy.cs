using UnityEngine;
using UnityEngine.AI;

public class VacuumEnemy : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent navAgent;
    public Transform player;

    [Header("Movement Settings")]
    public float speed = 5.5f;

    [Header("Suction Settings")]
    public float suctionRadius = 3f;
    public float suctionForce = 20f;
    public float antiGravityStrength = 9.81f;
    public LayerMask suctionMask; // layers des colliders aspirables

    private Rigidbody vacuumRigidbody;
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        // 1) Références
        navAgent       = navAgent ?? GetComponent<NavMeshAgent>();
        vacuumRigidbody = GetComponent<Rigidbody>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
            // on prend le Rigidbody sur le parent
            playerRigidbody = player.GetComponentInParent<Rigidbody>();

        navAgent.speed = speed;
    }

    private void Update()
    {
        if (player == null) return;

        // 2) Poursuite
        navAgent.SetDestination(player.position);

        // 3) Aspiration & anti-gravité
        ApplySuctionAndAntiGravity();
    }

    private void ApplySuctionAndAntiGravity()
    {
        // Récupère tous les colliders dans la zone
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            suctionRadius,
            suctionMask
        );

        foreach (Collider col in hits)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb == null) 
                continue;

            // *Prise en compte du joueur* (même si collider enfant)
            if (rb == playerRigidbody)
            {
                // Tu peux ajuster la force spéciale pour le joueur si besoin
                float specialMultiplier = 2f;
                Vector3 toVacuum = (transform.position - rb.position).normalized;
                float dist = Vector3.Distance(transform.position, rb.position);
                float factor = 1f - Mathf.Clamp01(dist / suctionRadius);

                rb.AddForce(toVacuum * suctionForce * factor * specialMultiplier, 
                            ForceMode.Acceleration);

                // anti-gravité aussi
                rb.AddForce(Vector3.up * antiGravityStrength * factor, 
                            ForceMode.Acceleration);

                continue;
            }

            // On ignore l’aspirateur lui-même
            if (rb == vacuumRigidbody)
                continue;

            // **Tous les autres** rigidbodies
            Vector3 dir = (transform.position - rb.position).normalized;
            float d    = Vector3.Distance(transform.position, rb.position);
            float m    = 1f - Mathf.Clamp01(d / suctionRadius);

            rb.AddForce(dir * suctionForce * m, ForceMode.Acceleration);
            rb.AddForce(Vector3.up * antiGravityStrength * m, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, suctionRadius);
    }
}
