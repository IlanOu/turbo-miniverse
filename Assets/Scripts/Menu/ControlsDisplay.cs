using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CommandInfo
{
    public string commandDescription;
    public Sprite buttonSprite;
}

public class ControlsDisplay : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject controlItemPrefab;
    [SerializeField] private Transform contentContainer;
    
    [Header("Configuration")]
    [SerializeField] private Vector2 spriteSize = new Vector2(40, 40);
    [SerializeField] private float spacing = 10f;
    
    [Header("Commandes")]
    [SerializeField] private CommandInfo[] carControls;
    
    void Start()
    {
        GenerateControlsList();
    }
    
    public void GenerateControlsList()
    {
        // Nettoyer le contenu existant
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Générer la liste des commandes
        foreach (CommandInfo command in carControls)
        {
            GameObject controlItem = Instantiate(controlItemPrefab, contentContainer);
            
            // Configurer l'image du bouton
            Image buttonImage = controlItem.transform.Find("ButtonImage").GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = command.buttonSprite;
                buttonImage.rectTransform.sizeDelta = spriteSize;
            }
            
            // Configurer le texte de description
            TextMeshProUGUI descriptionText = controlItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            if (descriptionText != null)
            {
                descriptionText.text = command.commandDescription;
            }
            
            // Configurer l'espacement (si nécessaire)
            HorizontalLayoutGroup layout = controlItem.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = spacing;
            }
        }
    }
    
    // Méthode pour mettre à jour la liste des commandes (si besoin)
    public void UpdateControlsList(CommandInfo[] newControls)
    {
        carControls = newControls;
        GenerateControlsList();
    }
}
