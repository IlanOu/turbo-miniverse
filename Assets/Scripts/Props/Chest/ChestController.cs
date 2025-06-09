using UnityEngine;
using System.Collections;
using Props.Chest;

namespace Props
{
    public class ChestController : MonoBehaviour
    {
        [Header("Paramètres d'animation")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float openDuration = 1f;
        [SerializeField] private float openAngle = 120f;

        [Header("Références")]
        [SerializeField] private Transform chestLid;
        [SerializeField] private ChestRewardManager rewardManager;
        
        [Header("Paramètres de coffre")]
        [SerializeField] private KeyType requiredKeyType = KeyType.Red;
        [SerializeField] private string chestName = "Coffre rouge";
        
        private bool isOpened = false;
        private bool isAnimating = false;
        private Vector3 originalPosition;
        
        void Start()
        {
            originalPosition = transform.position;
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isOpened && !isAnimating)
            {
                PlayerInventory playerInventory = other.GetComponentInParent<PlayerInventory>();
                
                if (playerInventory != null && playerInventory.HasKey(requiredKeyType))
                {
                    StartCoroutine(OpenChest());
                    playerInventory.UseKey(requiredKeyType);
                }
                else
                {
                    StartCoroutine(ShakeChest());
                    Debug.Log($"Il vous faut une clé {requiredKeyType} pour ouvrir ce {chestName}!");
                }
            }
        }
        
        IEnumerator ShakeChest()
        {
            isAnimating = true;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-shakeIntensity, shakeIntensity);
                float z = Random.Range(-shakeIntensity, shakeIntensity);
                
                transform.position = originalPosition + new Vector3(x, 0, z);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = originalPosition;
            isAnimating = false;
        }
        
        IEnumerator OpenChest()
        {
            isAnimating = true;
            isOpened = true;
            
            float elapsed = 0f;
            Quaternion startRotation = chestLid ? chestLid.rotation : transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(-openAngle, 0, 0);
            
            while (elapsed < openDuration)
            {
                float t = elapsed / openDuration;
                t = Mathf.SmoothStep(0, 1, t);
                
                if (chestLid)
                {
                    chestLid.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                }
                else
                {
                    transform.position = originalPosition + Vector3.up * (t * 0.5f);
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            GiveReward();
            isAnimating = false;
        }
        
        void GiveReward()
        {
            if (rewardManager == null)
            {
                rewardManager = GetComponent<ChestRewardManager>();
            }
            if (rewardManager != null)
            {
                rewardManager.OnChestOpened();
            }
        }

    }
}
