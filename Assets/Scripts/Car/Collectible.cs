using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [Header("Comportement")]
    [SerializeField] private bool canFloat = true;
    [SerializeField] private bool canRotate = true;
    
    [Header("Paramètres de flottement")]
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatSpeed = 1.5f;
    
    [Header("Paramètres de rotation")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 90f, 0);
    
    [Header("Collecte")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private UnityEvent onCollect;
    
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (canFloat)
        {
            // Effet de flottement avec un sinus
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        if (canRotate)
        {
            // Rotation continue
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.transform.parent!.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }
    
    private void Collect(GameObject player)
    {
        // Jouer l'effet sonore
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Créer l'effet visuel
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        onCollect?.Invoke();
        
        // Détruire l'objet
        Destroy(gameObject);
    }
}