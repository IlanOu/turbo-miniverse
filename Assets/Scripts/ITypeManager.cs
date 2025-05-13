using UnityEngine;

namespace DefaultNamespace
{
    public abstract class ITypeManager : MonoBehaviour
    {
        public abstract void ApplyConfiguration(ScriptableObject config);
    }
}