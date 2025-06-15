using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MasterVolumeController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string volumeParameter = "MasterVolume";
    
    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;
    
    [Header("Settings")]
    [SerializeField] private float defaultVolume = 0.75f;
    [SerializeField] private bool saveVolume = true;
    [SerializeField] private string saveKey = "MasterVolume";
    
    private float lastVolume;
    private bool isMuted = false;
    
    private void Start()
    {
        // Charger le volume sauvegardé
        if (saveVolume && PlayerPrefs.HasKey(saveKey))
        {
            lastVolume = PlayerPrefs.GetFloat(saveKey);
            isMuted = PlayerPrefs.GetInt(saveKey + "_muted", 0) == 1;
        }
        else
        {
            lastVolume = defaultVolume;
            isMuted = false;
        }
        
        // Configurer le slider
        if (volumeSlider != null)
        {
            volumeSlider.value = lastVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        
        // Configurer le toggle de mute
        if (muteToggle != null)
        {
            muteToggle.isOn = isMuted;
            muteToggle.onValueChanged.AddListener(SetMute);
        }
        
        // Appliquer le volume initial
        ApplyVolume();
    }
    
    public void SetVolume(float volume)
    {
        lastVolume = volume;
        ApplyVolume();
        SaveSettings();
    }
    
    public void SetMute(bool muted)
    {
        isMuted = muted;
        ApplyVolume();
        SaveSettings();
    }
    
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        if (muteToggle != null)
            muteToggle.isOn = isMuted;
        else
            ApplyVolume();
            
        SaveSettings();
    }
    
    private void ApplyVolume()
    {
        float effectiveVolume = isMuted ? 0f : lastVolume;
        
        // Convertir la valeur linéaire (0-1) en valeur logarithmique (-80dB à 0dB)
        float dbValue = effectiveVolume > 0.001f ? Mathf.Log10(effectiveVolume) * 20f : -80f;
        
        // Appliquer au mixer
        if (audioMixer != null)
        {
            audioMixer.SetFloat(volumeParameter, dbValue);
        }
        
        // Appliquer à toutes les sources audio si pas de mixer
        if (audioMixer == null)
        {
            AudioSource[] allSources = FindObjectsOfType<AudioSource>();
            foreach (AudioSource source in allSources)
            {
                source.volume = effectiveVolume;
            }
        }
    }
    
    private void SaveSettings()
    {
        if (saveVolume)
        {
            PlayerPrefs.SetFloat(saveKey, lastVolume);
            PlayerPrefs.SetInt(saveKey + "_muted", isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    
    // Méthodes publiques pour contrôler le volume depuis d'autres scripts
    public float GetVolume()
    {
        return lastVolume;
    }
    
    public bool IsMuted()
    {
        return isMuted;
    }
    
    public void SetVolumeAndMute(float volume, bool muted)
    {
        lastVolume = volume;
        isMuted = muted;
        
        if (volumeSlider != null)
            volumeSlider.value = volume;
            
        if (muteToggle != null)
            muteToggle.isOn = muted;
            
        ApplyVolume();
        SaveSettings();
    }
}
