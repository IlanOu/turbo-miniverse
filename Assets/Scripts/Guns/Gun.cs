using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint; // Déjà dans le prefab de l'arme

    [Header("Parameters")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletSize = 1f;
    
    private float nextFireTime = 0f;
    
    public bool TryFire()
    {
        if (Time.time < nextFireTime)
            return false;
            
        // Instanciation de la balle
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation * Quaternion.Euler(90, 0, 0));
        bullet.transform.localScale = Vector3.one * bulletSize;
        
        // Configuration de la balle
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent == null)
            bulletComponent = bullet.AddComponent<Bullet>();
        bulletComponent.SetParameters(damage, bulletSpeed);
        
        // Récupérer la vélocité du véhicule parent pour compenser son mouvement
        Rigidbody parentRb = GetComponentInParent<Rigidbody>();
        Vector3 parentVelocity = parentRb != null ? parentRb.linearVelocity : Vector3.zero;
        
        // Appliquer la vélocité calculée
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = bulletSpawnPoint.forward * bulletSpeed + parentVelocity;
        }
        
        // Détruire la balle après 5 secondes
        Destroy(bullet, 5f);
        
        nextFireTime = Time.time + fireRate;
        return true;
    }
    
    // Ajouts pour permettre l'accès à bulletSpeed et bulletSpawnPoint
    public float BulletSpeed => bulletSpeed;
    public Transform GetBulletSpawnPoint() => bulletSpawnPoint;
}