using System.Collections.Generic;
using UnityEngine;

namespace Props
{
    public class ChestRewardManager : MonoBehaviour
    {
        [Header("Track Reveal")]
        [SerializeField] private TrackRevealManager trackManager;
        [SerializeField] private List<int> trackPieceIndices = new List<int>(); // Liste des indices de pièces à révéler
        
        [Header("Reward Effects")]
        [SerializeField] private ParticleSystem chestOpenEffect;
        [SerializeField] private AudioClip rewardSound;
        
        private bool hasRevealed = false;
        private AudioSource audioSource;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Cette méthode doit être appelée par ChestController lorsque le coffre est ouvert
        public void OnChestOpened()
        {
            if (hasRevealed)
                return;
                
            hasRevealed = true;
            
            // Jouer les effets
            if (chestOpenEffect != null)
            {
                chestOpenEffect.Play();
            }
            
            if (rewardSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(rewardSound);
            }
            
            // Déclencher la révélation des pièces de circuit
            if (trackManager != null && trackPieceIndices.Count > 0)
            {
                trackManager.RevealTrackPieces(trackPieceIndices);
            }
        }
    }
}