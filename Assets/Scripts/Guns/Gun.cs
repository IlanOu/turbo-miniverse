using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Config")]
    public GunConfig config;
    [SerializeField] private Transform bulletSpawnPoint;

    private CarBattery battery;
    private float nextFireTime = 0f;
    private float energyCost;

    public float BulletSpeed => config != null ? config.bulletSpeed : 20f;
    public Transform GetBulletSpawnPoint() => bulletSpawnPoint;

    public void Initialize(CarBattery batteryRef, GunConfig newConfig = null)
    {
        if (newConfig != null)
            config = newConfig;

        battery = batteryRef;

        energyCost = config.energyCostPerShot < 0f
            ? (config.bulletSpeed * 0.01f) + (config.bulletSize * 0.1f)
            : config.energyCostPerShot;
    }

    public bool TryFire()
    {
        if (Time.time < nextFireTime || config == null)
            return false;

        if (battery != null && !battery.TryConsumeEnergy(energyCost))
            return false;

        GameObject bullet = Instantiate(config.bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation * Quaternion.Euler(90, 0, 0));
        bullet.transform.localScale = Vector3.one * config.bulletSize;
        
        Rigidbody parentRb = GetComponentInParent<Rigidbody>();
        Vector3 parentVelocity = parentRb != null ? parentRb.linearVelocity : Vector3.zero;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = bulletSpawnPoint.forward * config.bulletSpeed + parentVelocity;
        }

        Destroy(bullet, 5f);
        nextFireTime = Time.time + config.fireRate;

        return true;
    }
}