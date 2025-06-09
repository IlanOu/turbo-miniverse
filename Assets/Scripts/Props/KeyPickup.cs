using System.Collections;
using Props.Chest;
using UnityEngine;

namespace Props
{
    public enum KeyType
    {
        Red,
        Blue,
        Green,
        Gold,
        Silver
    }

    public class KeyPickup : MonoBehaviour
    {
        [Header("Paramètres d'animation")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private float floatFrequency = 1f;
        
        [Header("Paramètres de clé")]
        [SerializeField] private KeyType keyType = KeyType.Red;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private GameObject pickupEffectPrefab;

        private Vector3 startPosition;
        private bool isPickedUp = false;
        private float floatTimer = 0f;

        void Start()
        {
            startPosition = transform.position;
            floatTimer = Random.Range(0f, 2f * Mathf.PI);
        }

        void Update()
        {
            if (!isPickedUp)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                
                floatTimer += Time.deltaTime * floatFrequency;
                float newY = startPosition.y + Mathf.Sin(floatTimer) * floatAmplitude;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isPickedUp)
            {
                PlayerInventory playerInventory = other.GetComponentInParent<PlayerInventory>();
            
                if (playerInventory != null)
                {
                    isPickedUp = true;
                    
                    // Jouer un son si disponible
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }
                    
                    // Instancier un effet visuel si disponible
                    if (pickupEffectPrefab != null)
                    {
                        Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                    }
                    
                    // Désactiver les colliders immédiatement
                    DisableAllColliders();
                    
                    // Ajouter directement à l'inventaire (l'animation est gérée par PlayerInventory)
                    playerInventory.AddFloatingKey(gameObject, keyType);
                }
            }
        }
        
        private void DisableAllColliders()
        {
            // Désactiver tous les colliders sur l'objet et ses enfants
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }
    }
}
