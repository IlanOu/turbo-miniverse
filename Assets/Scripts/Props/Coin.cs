using UnityEngine;

namespace Props
{
    public class Coin : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float amplitude = 0.5f;
        [SerializeField] private float frequency = 1f;
        [SerializeField] private float rotationSpeed = 90f;
    
        [Header("Collection")]
        [SerializeField] private int coinValue = 10;
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioClip collectSound;
    
        private Vector3 startPosition;
        private float startTime;
    
        private void Start()
        {
            startPosition = transform.position;
            startTime = Time.time;
        }
    
        private void Update()
        {
            // Animation de flottement
            float newY = startPosition.y + amplitude * Mathf.Sin(frequency * (Time.time - startTime));
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
            // Rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Ajouter la valeur au système de monnaie
                MoneyManager.Instance.AddMoney(coinValue);
            
                // Effets de collection
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }
            
                if (collectSound != null && Camera.main != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position);
                }
            
                // Détruire la pièce
                Destroy(gameObject);
            }
        }
    }
}