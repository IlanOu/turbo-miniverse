using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageTabManager : MonoBehaviour
{
    [Header("Toggle Group Parent")]
    [SerializeField] private ToggleGroup tabToggleGroup;

    [Header("Panels associés à chaque tab")]
    [SerializeField] private List<GameObject> tabPanels = new List<GameObject>();

    private List<Toggle> tabToggles = new List<Toggle>();

    private void Awake()
    {
        // Récupérer dynamiquement tous les Toggles enfants du groupe
        tabToggles.Clear();
        foreach (Transform child in tabToggleGroup.transform)
        {
            Toggle toggle = child.GetComponent<Toggle>();
            if (toggle != null)
            {
                tabToggles.Add(toggle);
                toggle.group = tabToggleGroup;

                int index = tabToggles.Count - 1;
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                        ShowTab(index);
                });
            }
        }

        // Afficher le premier onglet par défaut
        ShowTab(0);
    }

    private void ShowTab(int index)
    {
        for (int i = 0; i < tabPanels.Count; i++)
        {
            tabPanels[i].SetActive(i == index);
        }
    }
}