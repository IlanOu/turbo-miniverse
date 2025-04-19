using UnityEngine;

namespace Guns
{
    public interface IShooterConfig
    {
        GameObject BulletPrefab { get; }
        float BulletSpeed { get; }
        float BulletSize { get; }
        float BulletMass { get; }
        float FireRate { get; }
    }

}