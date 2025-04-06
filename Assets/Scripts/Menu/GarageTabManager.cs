using UnityEngine;
using UnityEngine.UI;

public class GarageTabManager : MonoBehaviour
{
    [Header("Tab Buttons")]
    public Button carsTabButton;
    public Button gunsTabButton;
    public Button stationTabButton;

    [Header("Tab Panels")]
    public GameObject carsPanel;
    public GameObject gunsPanel;
    public GameObject stationPanel;

    private void Start()
    {
        carsTabButton.onClick.AddListener(() => ShowTab("Cars"));
        gunsTabButton.onClick.AddListener(() => ShowTab("Guns"));
        stationTabButton.onClick.AddListener(() => ShowTab("Station"));

        ShowTab("Cars"); // Onglet par défaut
    }

    public void ShowTab(string tabName)
    {
        carsPanel.SetActive(tabName == "Cars");
        gunsPanel.SetActive(tabName == "Guns");
        stationPanel.SetActive(tabName == "Station");
    }
}