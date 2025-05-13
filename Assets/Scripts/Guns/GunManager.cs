using DefaultNamespace;
using UnityEngine;

namespace Guns
{
    public class GunManager : ITypeManager
    {
        public GunConfig CurrentConfig { get; private set; }
        
        [SerializeField] private WeaponManager weaponManager;
        public override void ApplyConfiguration(ScriptableObject config)
        {
            weaponManager.ChangeGunConfig(config as GunConfig);
        }
    }
}