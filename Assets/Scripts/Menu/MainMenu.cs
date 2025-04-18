using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string playScene;
    
    [Space(10)]
    [SerializeField] private Button quitButton;
    
    void Start()
    {
        
        playButton.onClick.AddListener(Play);
        quitButton.onClick.AddListener(Quit);
    }
    
    private void Play()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(playScene);
    }
    
    private void Quit()
    {
# if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
# else
        Application.Quit();
# endif
    }
    
    
    
    
}
