using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AudioPlayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioPlaylistManager playlistManager;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private Image playPauseIcon;
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;
    
    [Header("Scrolling Title")]
    [SerializeField] private RectTransform titleContainer;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float scrollDelay = 1.5f;
    [SerializeField] private float scrollPadding = 50f;
    
    [Header("Animations")]
    [SerializeField] private float buttonPressScale = 0.9f;
    [SerializeField] private float buttonPressDuration = 0.1f;
    [SerializeField] private float trackChangeFadeDuration = 0.3f;
    [SerializeField] private Ease buttonEase = Ease.OutBack;
    [SerializeField] private Ease textFadeEase = Ease.OutQuad;
    
    private Sequence titleScrollSequence;
    private Tween artistFadeTween;
    private float titleWidth;
    private float containerWidth;
    private bool isScrollingNeeded = false;
    private bool isChangingTrack = false;
    
    private void Awake()
    {
        // Vérifier les références
        if (playlistManager == null)
        {
            playlistManager = FindObjectOfType<AudioPlaylistManager>();
            
            if (playlistManager == null)
            {
                Debug.LogError("AudioPlayerUI: No AudioPlaylistManager found!");
                enabled = false;
                return;
            }
        }
        
        // Configurer les boutons
        if (previousButton != null)
            previousButton.onClick.AddListener(OnPreviousButtonClicked);
            
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(OnPlayPauseButtonClicked);
            
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }
    
    private void OnEnable()
    {
        // S'abonner à l'événement de changement de piste
        if (playlistManager != null)
        {
            playlistManager.OnTrackChanged.AddListener(OnTrackChanged);
            
            // Mettre à jour l'UI avec la piste actuelle
            if (playlistManager.CurrentTrack != null)
            {
                UpdateTrackInfo(playlistManager.CurrentTrack, false);
            }
        }
        
        // Mettre à jour l'icône play/pause
        UpdatePlayPauseIcon();
    }
    
    private void OnDisable()
    {
        // Se désabonner de l'événement
        if (playlistManager != null)
        {
            playlistManager.OnTrackChanged.RemoveListener(OnTrackChanged);
        }
        
        // Arrêter les animations en cours
        StopAllAnimations();
    }
    
    private void StopAllAnimations()
    {
        if (titleScrollSequence != null && titleScrollSequence.IsActive())
        {
            titleScrollSequence.Kill();
            titleScrollSequence = null;
        }
        
        if (artistFadeTween != null && artistFadeTween.IsActive())
        {
            artistFadeTween.Kill();
            artistFadeTween = null;
        }
        
        // Réinitialiser les positions et opacités
        if (titleText != null)
        {
            titleText.rectTransform.anchoredPosition = Vector2.zero;
            titleText.alpha = 1f;
        }
        
        if (artistText != null)
        {
            artistText.alpha = 1f;
        }
    }
    
    private void Update()
    {
        // Mettre à jour l'icône play/pause
        UpdatePlayPauseIcon();
    }
    
    private void UpdatePlayPauseIcon()
    {
        if (playPauseIcon != null && playSprite != null && pauseSprite != null)
        {
            playPauseIcon.sprite = playlistManager.IsPlaying ? pauseSprite : playSprite;
        }
    }
    
    private void OnTrackChanged(AudioTrack track)
    {
        UpdateTrackInfo(track, true);
    }
    
    private void UpdateTrackInfo(AudioTrack track, bool animate)
    {
        // Éviter les mises à jour multiples
        if (isChangingTrack) return;
        isChangingTrack = true;
        
        // Arrêter les animations en cours
        StopAllAnimations();
        
        if (animate)
        {
            // Animer la transition
            if (titleText != null)
            {
                // Faire disparaître le texte actuel
                titleText.DOFade(0f, trackChangeFadeDuration / 2f).SetEase(textFadeEase).OnComplete(() => {
                    // Mettre à jour le texte
                    titleText.text = track.title;
                    
                    // Faire réapparaître le texte
                    titleText.DOFade(1f, trackChangeFadeDuration / 2f).SetEase(textFadeEase).OnComplete(() => {
                        // Configurer le défilement si nécessaire
                        SetupTitleScrolling();
                        isChangingTrack = false;
                    });
                });
            }
            
            if (artistText != null)
            {
                // Animer le texte de l'artiste
                artistFadeTween = artistText.DOFade(0f, trackChangeFadeDuration / 2f).SetEase(textFadeEase).OnComplete(() => {
                    artistText.text = track.artist;
                    artistFadeTween = artistText.DOFade(1f, trackChangeFadeDuration / 2f).SetEase(textFadeEase);
                });
            }
        }
        else
        {
            // Mise à jour sans animation
            if (titleText != null)
            {
                titleText.text = track.title;
                titleText.alpha = 1f;
            }
            
            if (artistText != null)
            {
                artistText.text = track.artist;
                artistText.alpha = 1f;
            }
            
            // Configurer le défilement si nécessaire
            SetupTitleScrolling();
            isChangingTrack = false;
        }
    }
    
    private void SetupTitleScrolling()
    {
        if (titleText == null || titleContainer == null) return;
        
        // Attendre une frame pour que le texte soit correctement mis à jour
        StartCoroutine(DelayedTitleScrolling());
    }
    
    private IEnumerator DelayedTitleScrolling()
    {
        // Attendre une frame pour que le texte soit correctement mis à jour
        yield return null;
        
        // Mesurer la largeur du texte et du conteneur
        titleWidth = titleText.preferredWidth;
        containerWidth = titleContainer.rect.width;
        
        // Réinitialiser la position du texte
        titleText.rectTransform.anchoredPosition = Vector2.zero;
        
        // Vérifier si le défilement est nécessaire
        isScrollingNeeded = titleWidth > containerWidth - 20; // Marge de 20 pixels
        
        if (isScrollingNeeded)
        {
            // Créer une séquence de défilement
            titleScrollSequence = DOTween.Sequence();
            
            // Attendre avant de commencer le défilement
            titleScrollSequence.AppendInterval(scrollDelay);
            
            // Calculer la distance de défilement
            float scrollDistance = titleWidth - containerWidth + scrollPadding;
            
            // Animer le défilement
            titleScrollSequence.Append(titleText.rectTransform.DOAnchorPosX(-scrollDistance, scrollDistance / scrollSpeed).SetEase(Ease.Linear));
            
            // Attendre à la fin
            titleScrollSequence.AppendInterval(scrollDelay);
            
            // Revenir à la position initiale
            titleScrollSequence.Append(titleText.rectTransform.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuad));
            
            // Répéter indéfiniment
            titleScrollSequence.SetLoops(-1);
        }
    }
    
    private void OnPreviousButtonClicked()
    {
        // Animer le bouton
        AnimateButtonPress(previousButton.transform);
        
        // Jouer la piste précédente
        playlistManager.PlayPreviousTrack();
    }
    
    private void OnPlayPauseButtonClicked()
    {
        // Animer le bouton
        AnimateButtonPress(playPauseButton.transform);
        
        // Basculer lecture/pause
        playlistManager.TogglePlayPause();
    }
    
    private void OnNextButtonClicked()
    {
        // Animer le bouton
        AnimateButtonPress(nextButton.transform);
        
        // Jouer la piste suivante
        playlistManager.PlayNextTrack();
    }
    
    private void AnimateButtonPress(Transform buttonTransform)
    {
        // Arrêter toute animation en cours sur ce bouton
        buttonTransform.DOKill();
        
        // Réinitialiser l'échelle
        buttonTransform.localScale = Vector3.one;
        
        // Séquence d'animation pour le bouton
        Sequence buttonSequence = DOTween.Sequence();
        
        // Réduire le bouton
        buttonSequence.Append(buttonTransform.DOScale(buttonPressScale, buttonPressDuration).SetEase(buttonEase));
        
        // Revenir à la taille normale
        buttonSequence.Append(buttonTransform.DOScale(1f, buttonPressDuration).SetEase(buttonEase));
    }
}
