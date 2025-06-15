using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private List<RectTransform> menuButtons = new List<RectTransform>();
    
    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float delayBetweenButtons = 0.1f;
    [SerializeField] private float slideDistance = 50f;
    [SerializeField] private Ease easeType = Ease.OutBack;
    
    
    [Header("Boutons")]
    [SerializeField] private KeyCode togglePauseKey = KeyCode.Escape;
    private bool isPaused = false;
    private bool isAnimating = false;
    private Vector3[] originalPositions;
    
    void Start()
    {
        // Cacher le menu au démarrage
        pauseMenuPanel.SetActive(false);
        
        // Sauvegarder les positions originales des boutons
        originalPositions = new Vector3[menuButtons.Count];
        for (int i = 0; i < menuButtons.Count; i++)
        {
            originalPositions[i] = menuButtons[i].localPosition;
        }
    }
    
    void Update()
    {
        // Vérifier si la touche configurée est pressée et qu'aucune animation n'est en cours
        if (Input.GetKeyDown(togglePauseKey) && !isAnimating)
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        // Ne rien faire si une animation est en cours
        if (isAnimating)
            return;
            
        isPaused = !isPaused;
        
        if (isPaused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }
    
    private void ShowPauseMenu()
    {
        // Marquer le début de l'animation
        isAnimating = true;
        
        // Mettre le jeu en pause
        Time.timeScale = 0f;
        
        // Afficher le panneau
        pauseMenuPanel.SetActive(true);
        
        // Animer les boutons un par un
        for (int i = 0; i < menuButtons.Count; i++)
        {
            // Position initiale (depuis le haut)
            Vector3 startPos = originalPositions[i] + new Vector3(0, slideDistance, 0);
            menuButtons[i].localPosition = startPos;
            
            // Animer vers la position originale
            menuButtons[i].DOLocalMove(originalPositions[i], animationDuration)
                .SetDelay(i * delayBetweenButtons)
                .SetEase(easeType)
                .SetUpdate(true); // Important pour que l'animation fonctionne quand le jeu est en pause
        }
        
        // Calculer la durée totale de l'animation
        float totalAnimationTime = animationDuration + ((menuButtons.Count - 1) * delayBetweenButtons);
        
        // Marquer la fin de l'animation après que toutes les animations soient terminées
        DOVirtual.DelayedCall(totalAnimationTime, () => {
            isAnimating = false;
        }, true);
    }
    
    private void HidePauseMenu()
    {
        // Marquer le début de l'animation
        isAnimating = true;
        
        // Animer les boutons en séquence inverse
        for (int i = 0; i < menuButtons.Count; i++)
        {
            // Position finale (vers le haut)
            Vector3 endPos = originalPositions[i] + new Vector3(0, slideDistance, 0);
            
            // Index inversé pour l'animation
            int reverseIndex = menuButtons.Count - 1 - i;
            
            // Animer vers la position hors écran
            menuButtons[reverseIndex].DOLocalMove(endPos, animationDuration)
                .SetDelay(i * delayBetweenButtons)
                .SetEase(Ease.InBack)
                .SetUpdate(true);
        }
        
        // Calculer la durée totale de l'animation
        float totalAnimationTime = animationDuration + ((menuButtons.Count - 1) * delayBetweenButtons);
        
        // Désactiver le panneau après la dernière animation
        DOVirtual.DelayedCall(totalAnimationTime, () => {
            pauseMenuPanel.SetActive(false);
            // Reprendre le jeu
            Time.timeScale = 1f;
            // Marquer la fin de l'animation
            isAnimating = false;
        }, true);
    }
    
    // Fonctions pour les boutons
    public void ResumeGame()
    {
        // Ne rien faire si une animation est en cours
        if (isAnimating)
            return;
            
        TogglePause();
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Rétablir le temps normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitToMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f; // Rétablir le temps normal
        SceneManager.LoadScene(mainMenuSceneName); // Remplacer par le nom de votre scène de menu principal
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
