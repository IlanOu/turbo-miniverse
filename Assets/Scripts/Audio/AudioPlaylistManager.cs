using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AudioTrack
{
    public string title;
    public string artist;
    public AudioClip clip;
    [Tooltip("Battements par minute pour cette piste")]
    public float bpm = 120f;
}


public class AudioPlaylistManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private List<AudioTrack> playlist = new List<AudioTrack>();
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool shuffle = false;
    [SerializeField] private bool loop = true;
    [SerializeField] private float crossfadeDuration = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;
    
    [Header("Shuffle Settings")]
    [SerializeField] private int preventRecentTracksCount = 3;
    [Tooltip("Si activé, les pistes récemment jouées ne seront pas rejouées avant d'avoir parcouru un certain nombre d'autres pistes")]
    [SerializeField] private bool avoidRecentTracks = true;
    
    [Header("Events")]
    public UnityEvent<AudioTrack> OnTrackChanged;
    
    private AudioSource audioSource;
    private AudioSource crossfadeSource;
    private int currentTrackIndex = -1;
    private List<int> shuffledIndices = new List<int>();
    private int shuffleIndex = 0;
    private Queue<int> recentlyPlayedTracks = new Queue<int>();
    
    public AudioTrack CurrentTrack => currentTrackIndex >= 0 && currentTrackIndex < playlist.Count ? 
                                     playlist[currentTrackIndex] : null;
    
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
    
    private void Awake()
    {
        // Créer les AudioSources
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        
        crossfadeSource = gameObject.AddComponent<AudioSource>();
        crossfadeSource.playOnAwake = false;
        crossfadeSource.loop = false;
        crossfadeSource.volume = 0f;
        
        // Initialiser la playlist
        if (shuffle)
        {
            GenerateShuffledPlaylist();
        }
    }
    
    private void Start()
    {
        if (playOnAwake && playlist.Count > 0)
        {
            PlayTrack(0);
        }
    }
    
    private void Update()
    {
        // Vérifier si la piste actuelle est terminée
        if (audioSource != null && !audioSource.isPlaying && currentTrackIndex >= 0)
        {
            PlayNextTrack();
        }
    }
    
    private void GenerateShuffledPlaylist()
    {
        shuffledIndices.Clear();
        
        // Créer une liste d'indices disponibles (excluant les pistes récemment jouées)
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < playlist.Count; i++)
        {
            if (!avoidRecentTracks || !recentlyPlayedTracks.Contains(i))
            {
                availableIndices.Add(i);
            }
        }
        
        // Si tous les indices sont dans la liste des récemment joués, utiliser tous les indices
        if (availableIndices.Count == 0)
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                availableIndices.Add(i);
            }
        }
        
        // Mélanger les indices disponibles
        while (availableIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            shuffledIndices.Add(availableIndices[randomIndex]);
            availableIndices.RemoveAt(randomIndex);
        }
        
        shuffleIndex = 0;
    }
    
    public void PlayTrack(int index)
    {
        if (playlist.Count == 0) return;
        
        // Valider l'index
        index = Mathf.Clamp(index, 0, playlist.Count - 1);
        
        // Si la même piste est déjà en cours de lecture, ne rien faire
        if (index == currentTrackIndex && audioSource.isPlaying)
            return;
            
        // Ajouter la piste à la liste des récemment jouées
        if (avoidRecentTracks)
        {
            // Éviter les doublons
            if (!recentlyPlayedTracks.Contains(index))
            {
                recentlyPlayedTracks.Enqueue(index);
                
                // Limiter la taille de la file
                while (recentlyPlayedTracks.Count > preventRecentTracksCount)
                {
                    recentlyPlayedTracks.Dequeue();
                }
            }
        }
        
        currentTrackIndex = index;
        
        // Arrêter la lecture en cours
        StopAllCoroutines();
        
        // Démarrer la nouvelle piste avec crossfade
        StartCoroutine(CrossfadeToNewTrack(playlist[currentTrackIndex].clip));
        
        // Déclencher l'événement
        OnTrackChanged?.Invoke(playlist[currentTrackIndex]);
    }
    
    private IEnumerator CrossfadeToNewTrack(AudioClip newClip)
    {
        // Échanger les sources pour le crossfade
        AudioSource tempSource = audioSource;
        audioSource = crossfadeSource;
        crossfadeSource = tempSource;
        
        // Configurer la nouvelle source
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();
        
        // Effectuer le crossfade
        float timer = 0f;
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;
            
            audioSource.volume = Mathf.Lerp(0f, volume, t);
            
            if (crossfadeSource.isPlaying)
                crossfadeSource.volume = Mathf.Lerp(volume, 0f, t);
                
            yield return null;
        }
        
        // Finaliser le crossfade
        audioSource.volume = volume;
        
        if (crossfadeSource.isPlaying)
            crossfadeSource.Stop();
    }
    
    public void PlayNextTrack()
    {
        if (playlist.Count == 0) return;
        
        if (shuffle)
        {
            shuffleIndex++;
            
            // Si on a atteint la fin de la liste mélangée
            if (shuffleIndex >= shuffledIndices.Count)
            {
                if (loop)
                {
                    // Regénérer une nouvelle liste mélangée
                    GenerateShuffledPlaylist();
                }
                else
                {
                    // Arrêter la lecture
                    StopPlayback();
                    return;
                }
            }
            
            PlayTrack(shuffledIndices[shuffleIndex]);
        }
        else
        {
            int nextIndex = currentTrackIndex + 1;
            
            // Si on a atteint la fin de la playlist
            if (nextIndex >= playlist.Count)
            {
                if (loop)
                {
                    nextIndex = 0;
                }
                else
                {
                    // Arrêter la lecture
                    StopPlayback();
                    return;
                }
            }
            
            PlayTrack(nextIndex);
        }
    }
    
    public void PlayPreviousTrack()
    {
        if (playlist.Count == 0) return;
        
        if (shuffle)
        {
            shuffleIndex--;
            
            // Si on est au début de la liste mélangée
            if (shuffleIndex < 0)
            {
                if (loop)
                {
                    shuffleIndex = shuffledIndices.Count - 1;
                }
                else
                {
                    shuffleIndex = 0;
                }
            }
            
            PlayTrack(shuffledIndices[shuffleIndex]);
        }
        else
        {
            int prevIndex = currentTrackIndex - 1;
            
            // Si on est au début de la playlist
            if (prevIndex < 0)
            {
                if (loop)
                {
                    prevIndex = playlist.Count - 1;
                }
                else
                {
                    prevIndex = 0;
                }
            }
            
            PlayTrack(prevIndex);
        }
    }
    
    public void TogglePlayPause()
    {
        if (audioSource == null) return;
        
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            if (currentTrackIndex < 0 && playlist.Count > 0)
            {
                PlayTrack(0);
            }
            else if (audioSource.clip != null)
            {
                audioSource.UnPause();
            }
            else if (playlist.Count > 0)
            {
                PlayTrack(0);
            }
        }
    }
    
    public void StopPlayback()
    {
        audioSource.Stop();
        crossfadeSource.Stop();
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
    
    public void ToggleShuffle()
    {
        shuffle = !shuffle;
        
        if (shuffle)
        {
            GenerateShuffledPlaylist();
            
            // Trouver l'index actuel dans la liste mélangée
            for (int i = 0; i < shuffledIndices.Count; i++)
            {
                if (shuffledIndices[i] == currentTrackIndex)
                {
                    shuffleIndex = i;
                    break;
                }
            }
        }
    }
    
    public void ToggleLoop()
    {
        loop = !loop;
    }
    
    // Méthode pour vider la liste des pistes récemment jouées
    public void ClearRecentlyPlayedTracks()
    {
        recentlyPlayedTracks.Clear();
    }
    
    // Méthode pour activer/désactiver l'évitement des pistes récentes
    public void SetAvoidRecentTracks(bool avoid)
    {
        avoidRecentTracks = avoid;
    }
    
    // Méthode pour définir le nombre de pistes à éviter
    public void SetPreventRecentTracksCount(int count)
    {
        preventRecentTracksCount = Mathf.Max(1, count);
        
        // Ajuster la file si nécessaire
        while (recentlyPlayedTracks.Count > preventRecentTracksCount)
        {
            recentlyPlayedTracks.Dequeue();
        }
    }
}
