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
    public float bpm = 120f;
}

public class AudioPlaylistManager : MonoBehaviour
{
    [SerializeField] private List<AudioTrack> playlist = new List<AudioTrack>();
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool shuffle = false;
    [SerializeField] private bool loop = true;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.7f;
    
    [SerializeField] private int preventRecentTracksCount = 3;
    [SerializeField] private bool avoidRecentTracks = true;
    
    public UnityEvent<AudioTrack> OnTrackChanged;

    private AudioSource audioSource;
    private int currentTrackIndex = -1;
    private List<int> shuffledIndices = new List<int>();
    private int shuffleIndex = 0;
    private Queue<int> recentlyPlayedTracks = new Queue<int>();
    private bool wasPausedManually = false;

    public AudioTrack CurrentTrack => currentTrackIndex >= 0 && currentTrackIndex < playlist.Count
        ? playlist[currentTrackIndex]
        : null;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private void Awake()
    {
        // Initialiser le générateur de nombres aléatoires avec un seed basé sur le temps
        Random.InitState((int)System.DateTime.Now.Ticks);
    
        // Une seule source audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;

        if (shuffle)
        {
            GenerateShuffledPlaylist();
        }
    }


    private void Start()
    {
        if (playOnAwake && playlist.Count > 0)
        {
            if (shuffle)
            {
                // Jouer la première piste de la liste mélangée
                PlayTrack(shuffledIndices[0]);
            }
            else
            {
                // Jouer la première piste normalement
                PlayTrack(0);
            }
        }
    }


    private void Update()
    {
        // Vérifier si la piste actuelle est terminée
        if (audioSource != null && !audioSource.isPlaying && audioSource.clip != null && currentTrackIndex >= 0)
        {
            // Vérifier si la piste est réellement terminée (et pas juste en pause)
            if (!wasPausedManually && audioSource.time >= audioSource.clip.length - 0.1f)
            {
                PlayNextTrack();
            }
        }
    }

    private void GenerateShuffledPlaylist()
    {
        shuffledIndices.Clear();
        List<int> availableIndices = new List<int>();
        
        for (int i = 0; i < playlist.Count; i++)
        {
            if (!avoidRecentTracks || !recentlyPlayedTracks.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count == 0)
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                availableIndices.Add(i);
            }
        }

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

        index = Mathf.Clamp(index, 0, playlist.Count - 1);

        // Si la même piste est déjà en cours de lecture, ne rien faire
        if (index == currentTrackIndex && audioSource.isPlaying)
            return;

        if (avoidRecentTracks && !recentlyPlayedTracks.Contains(index))
        {
            recentlyPlayedTracks.Enqueue(index);
            while (recentlyPlayedTracks.Count > preventRecentTracksCount)
            {
                recentlyPlayedTracks.Dequeue();
            }
        }

        currentTrackIndex = index;
        
        // Simplement changer le clip et jouer
        audioSource.Stop();
        audioSource.clip = playlist[currentTrackIndex].clip;
        audioSource.volume = volume;
        audioSource.Play();
        wasPausedManually = false;
        
        OnTrackChanged?.Invoke(playlist[currentTrackIndex]);
    }

    public void PlayNextTrack()
    {
        if (playlist.Count == 0) return;

        if (shuffle)
        {
            shuffleIndex++;
            if (shuffleIndex >= shuffledIndices.Count)
            {
                if (loop)
                {
                    GenerateShuffledPlaylist();
                }
                else
                {
                    StopPlayback();
                    return;
                }
            }
            PlayTrack(shuffledIndices[shuffleIndex]);
        }
        else
        {
            int nextIndex = currentTrackIndex + 1;
            if (nextIndex >= playlist.Count)
            {
                if (loop)
                {
                    nextIndex = 0;
                }
                else
                {
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
            // Mettre en pause
            audioSource.Pause();
            wasPausedManually = true;
            Debug.Log("Audio paused manually");
        }
        else
        {
            // Reprendre la lecture
            if (audioSource.clip != null)
            {
                audioSource.UnPause();
                wasPausedManually = false;
                Debug.Log("Audio resumed from pause");
            }
            else if (playlist.Count > 0)
            {
                PlayTrack(0);
                Debug.Log("Started playing first track");
            }
        }
    }

    public void StopPlayback()
    {
        audioSource.Stop();
        wasPausedManually = false;
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

    public void ClearRecentlyPlayedTracks()
    {
        recentlyPlayedTracks.Clear();
    }

    public void SetAvoidRecentTracks(bool avoid)
    {
        avoidRecentTracks = avoid;
    }

    public void SetPreventRecentTracksCount(int count)
    {
        preventRecentTracksCount = Mathf.Max(1, count);
        while (recentlyPlayedTracks.Count > preventRecentTracksCount)
        {
            recentlyPlayedTracks.Dequeue();
        }
    }
    
    // Gestion des événements d'application
    private void OnApplicationPause(bool pauseStatus)
    {
        // Ne rien faire ici pour éviter les problèmes
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Ne rien faire ici pour éviter les problèmes
    }
}
