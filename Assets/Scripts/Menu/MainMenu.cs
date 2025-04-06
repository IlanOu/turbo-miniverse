using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button garageButton;
    [SerializeField] private GameObject garageMenuPanel;
    [SerializeField] private Button menuButton;
    
    [SerializeField] private CameraPresets cameraPresets;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloseGarageMenu();
        garageButton.onClick.AddListener(DisplayGarageMenu);
        menuButton.onClick.AddListener(CloseGarageMenu);
        
        if (cameraPresets != null)
            cameraPresets.SetCameraPosition(0);
    }
    
    void DisplayGarageMenu()
    {
        mainMenuPanel.SetActive(false);
        garageMenuPanel.SetActive(true);
        
        if (cameraPresets != null)
            cameraPresets.SmoothToCameraPosition(1);
    }
    
    void CloseGarageMenu()
    {
        mainMenuPanel.SetActive(true);
        garageMenuPanel.SetActive(false);
        
        if (cameraPresets != null)
            cameraPresets.SmoothToCameraPosition(0);
    }

}
