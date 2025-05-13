using DefaultNamespace;
using UnityEngine;

namespace Car
{
    public class CarManager : ITypeManager
    {
        public CarConfig CurrentConfig { get; private set; }

        [SerializeField] private CarController carController;
        
        public override void ApplyConfiguration(ScriptableObject config)
        {
            carController.config = config as CarConfig;    
        }
    }
}