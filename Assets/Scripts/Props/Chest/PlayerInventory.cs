using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Props.Chest
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Paramètres des clés flottantes")]
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float orbitHeight = 1.5f;
        [SerializeField] private float orbitSpeed = 50f;
        
        [Header("Paramètres d'animation")]
        [SerializeField] private float pickupDuration = 1.2f;
        [SerializeField] private float consumeDuration = 0.8f;
        
        // Structure pour stocker les informations de clé
        private class KeyInfo
        {
            public GameObject keyObject;
            public KeyType keyType;
            public float angle;
            
            public KeyInfo(GameObject obj, KeyType type, float initialAngle)
            {
                keyObject = obj;
                keyType = type;
                angle = initialAngle;
            }
        }
        
        private List<KeyInfo> floatingKeys = new List<KeyInfo>();
        private Transform keysContainer;
        
        void Start()
        {
            // Créer un conteneur pour les clés
            keysContainer = new GameObject("FloatingKeys").transform;
            keysContainer.SetParent(transform);
            keysContainer.localPosition = new Vector3(0, orbitHeight, 0);
        }
        
        void Update()
        {
            // Faire tourner les clés autour du conteneur
            UpdateKeyRotations();
        }
        
        void UpdateKeyRotations()
        {
            for (int i = 0; i < floatingKeys.Count; i++)
            {
                if (floatingKeys[i].keyObject == null)
                {
                    floatingKeys.RemoveAt(i);
                    i--;
                    continue;
                }
                
                // Mettre à jour l'angle de la clé
                floatingKeys[i].angle += orbitSpeed * Time.deltaTime;
                if (floatingKeys[i].angle >= 360f) floatingKeys[i].angle -= 360f;
                
                // Calculer la position en orbite
                float x = Mathf.Sin(floatingKeys[i].angle * Mathf.Deg2Rad) * orbitRadius;
                float z = Mathf.Cos(floatingKeys[i].angle * Mathf.Deg2Rad) * orbitRadius;
                float y = Mathf.Sin(Time.time * 2f + i) * 0.1f; // Petit mouvement vertical
                
                // Appliquer la position locale
                floatingKeys[i].keyObject.transform.localPosition = new Vector3(x, y, z);
                
                // Rotation continue de la clé sur elle-même
                floatingKeys[i].keyObject.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
            }
        }
        
        // Méthode pour redistribuer uniformément les angles des clés
        private void RedistributeKeyAngles()
        {
            int keyCount = floatingKeys.Count;
            if (keyCount <= 1) return; // Pas besoin de redistribuer s'il n'y a qu'une seule clé
            
            float angleStep = 360f / keyCount;
            
            for (int i = 0; i < keyCount; i++)
            {
                // Distribuer uniformément les angles
                floatingKeys[i].angle = i * angleStep;
            }
        }
        
        public void AddFloatingKey(GameObject key, KeyType keyType)
        {
            // Désactiver tous les colliders
            DisableAllColliders(key);
            
            // Désactiver le script KeyPickup
            KeyPickup keyPickup = key.GetComponent<KeyPickup>();
            if (keyPickup != null)
            {
                keyPickup.enabled = false;
            }
            
            // Calculer un angle initial pour la nouvelle clé
            float initialAngle = 0f;
            if (floatingKeys.Count > 0)
            {
                // Placer la nouvelle clé à l'opposé de la dernière clé ajoutée
                initialAngle = (floatingKeys[floatingKeys.Count - 1].angle + 180f) % 360f;
            }
            
            // Créer l'info de clé et l'ajouter
            KeyInfo keyInfo = new KeyInfo(key, keyType, initialAngle);
            floatingKeys.Add(keyInfo);
            
            // Redistribuer les angles pour que toutes les clés soient équidistantes
            RedistributeKeyAngles();
            
            // Important: Rendre la clé enfant du conteneur AVANT de commencer l'animation
            // Sauvegarder la position et rotation mondiales
            Vector3 worldPos = key.transform.position;
            Quaternion worldRot = key.transform.rotation;
            Vector3 worldScale = key.transform.localScale;
            
            // Rendre enfant du conteneur
            key.transform.SetParent(keysContainer);
            
            // Restaurer la position mondiale pour éviter le saut
            key.transform.position = worldPos;
            key.transform.rotation = worldRot;
            key.transform.localScale = worldScale;
            
            // Lancer l'animation de récupération
            StartCoroutine(SmoothPickupAnimation(key, keyInfo));
            
            Debug.Log($"Clé de type {keyType} ajoutée ! Total : {floatingKeys.Count}");
        }
        
        private void DisableAllColliders(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }
        
        IEnumerator SmoothPickupAnimation(GameObject key, KeyInfo keyInfo)
        {
            // Calculer la position cible finale dans l'espace local
            float targetAngle = keyInfo.angle;
            float targetX = Mathf.Sin(targetAngle * Mathf.Deg2Rad) * orbitRadius;
            float targetZ = Mathf.Cos(targetAngle * Mathf.Deg2Rad) * orbitRadius;
            Vector3 targetLocalPos = new Vector3(targetX, 0, targetZ);
            
            // Obtenir la position locale actuelle (après avoir défini le parent)
            Vector3 startLocalPos = key.transform.localPosition;
            Vector3 startLocalScale = key.transform.localScale;
            
            // Animation complète en une seule phase fluide
            float elapsed = 0f;
            
            while (elapsed < pickupDuration)
            {
                float t = elapsed / pickupDuration;
                
                // Utiliser une courbe de lissage pour un mouvement plus naturel
                float smoothT = Mathf.SmoothStep(0, 1, t);
                
                // Interpoler la position locale
                Vector3 newLocalPos = Vector3.Lerp(startLocalPos, targetLocalPos, smoothT);
                
                // Ajouter un mouvement vertical supplémentaire au milieu de l'animation
                float extraHeight = Mathf.Sin(t * Mathf.PI) * 0.5f;
                newLocalPos.y += extraHeight;
                
                // Appliquer la position
                key.transform.localPosition = newLocalPos;
                
                // Rotation plus rapide pendant l'animation
                float rotationSpeed = Mathf.Lerp(360f, 90f, smoothT);
                key.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                
                // Effet de mise à l'échelle subtil
                float scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI * 2) * 0.15f;
                key.transform.localScale = startLocalScale * scaleMultiplier;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Finaliser la position
            key.transform.localPosition = targetLocalPos;
            key.transform.localScale = startLocalScale;
        }
        
        public bool HasKey(KeyType requiredKeyType)
        {
            return floatingKeys.Exists(k => k.keyType == requiredKeyType);
        }
        
        public void UseKey(KeyType keyType)
        {
            int keyIndex = floatingKeys.FindIndex(k => k.keyType == keyType);
            
            if (keyIndex >= 0)
            {
                GameObject keyToUse = floatingKeys[keyIndex].keyObject;
                
                // Sauvegarder la position mondiale avant de détacher
                Vector3 worldPos = keyToUse.transform.position;
                Quaternion worldRot = keyToUse.transform.rotation;
                Vector3 worldScale = keyToUse.transform.localScale;
                
                // Retirer de la liste et détacher du parent
                floatingKeys.RemoveAt(keyIndex);
                keyToUse.transform.SetParent(null);
                
                // Restaurer la position mondiale pour éviter le saut
                keyToUse.transform.position = worldPos;
                keyToUse.transform.rotation = worldRot;
                keyToUse.transform.localScale = worldScale;
                
                // Redistribuer les angles des clés restantes
                RedistributeKeyAngles();
                
                StartCoroutine(EnhancedConsumeAnimation(keyToUse));
                
                Debug.Log($"Clé de type {keyType} utilisée. Restantes : {floatingKeys.Count}");
            }
        }
        
        IEnumerator EnhancedConsumeAnimation(GameObject key)
        {
            Vector3 startPos = key.transform.position;
            Vector3 startScale = key.transform.localScale;
            Vector3 targetPos = startPos + Vector3.up * 2f;
            
            float elapsed = 0f;
            
            while (elapsed < consumeDuration)
            {
                if (!key) yield break;
                
                float t = elapsed / consumeDuration;
                float smoothT = Mathf.SmoothStep(0, 1, t);
                
                // Mouvement vers le haut avec une trajectoire en arc
                Vector3 newPos = Vector3.Lerp(startPos, targetPos, smoothT);
                // Ajouter un petit mouvement latéral pour plus de dynamisme
                newPos += transform.right * (Mathf.Sin(t * Mathf.PI * 2) * 0.3f);
                key.transform.position = newPos;
                
                // Rotation qui s'accélère
                float rotationSpeed = Mathf.Lerp(180f, 720f, t);
                key.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                
                // Réduction de taille avec un effet de rebond
                float scaleMultiplier = 1f;
                if (t < 0.7f)
                {
                    // Légère augmentation puis diminution
                    scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
                }
                else
                {
                    // Diminution rapide à la fin
                    scaleMultiplier = Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
                }
                key.transform.localScale = startScale * scaleMultiplier;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Destroy(key);
        }
    }
}
