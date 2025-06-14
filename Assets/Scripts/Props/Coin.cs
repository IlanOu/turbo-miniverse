using UnityEngine;
using DG.Tweening;

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
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private float collectDuration = 0.4f;
        [SerializeField] private float collectHeight = 2f;
    
        private Vector3 startPosition;
        private bool isCollected = false;
    
        private void Start()
        {
            startPosition = transform.position;
        }
    
        private void Update()
        {
            if (isCollected) return;
            
            // Animation de flottement
            float newY = startPosition.y + amplitude * Mathf.Sin(frequency * Time.time);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // Rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isCollected)
            {
                isCollected = true;
                
                // Désactiver le collider
                Collider coinCollider = GetComponent<Collider>();
                if (coinCollider) coinCollider.enabled = false;
                
                // Jouer le son
                if (collectSound)
                {
                    AudioSource.PlayClipAtPoint(collectSound, Camera.main ? Camera.main.transform.position : transform.position);
                }
                
                // Ajouter la valeur immédiatement
                MoneyManager.Instance.AddMoney(coinValue);
                
                // Animation style Mario Kart
                transform.DOLocalMoveY(transform.position.y + collectHeight, collectDuration)
                    .SetEase(Ease.OutQuad);
                
                transform.DOScale(Vector3.zero, collectDuration)
                    .SetEase(Ease.InQuad)
                    .SetDelay(collectDuration * 0.5f)
                    .OnComplete(() => Destroy(gameObject));
                
                // Rotation rapide
                transform.DORotate(new Vector3(0, 360, 0), collectDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad)
                    .SetRelative(true);
            }
        }
    }
}
