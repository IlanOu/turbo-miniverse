using UnityEngine;
using System;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private int startingMoney = 0;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private string moneyFormat = "{0}";
    [SerializeField] private GameObject moneyAddedEffect;
    
    private int currentMoney;
    
    // Événement pour notifier les changements de monnaie
    public event Action<int> OnMoneyChanged;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialiser avec la valeur de départ
        currentMoney = startingMoney;
        UpdateMoneyUI();
    }
    
    public int GetMoney()
    {
        return currentMoney;
    }
    
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        
        currentMoney += amount;
        UpdateMoneyUI();
        
        // Déclencher l'événement
        OnMoneyChanged?.Invoke(currentMoney);
        
        // Effet visuel (optionnel)
        if (moneyAddedEffect != null && moneyText != null)
        {
            GameObject effect = Instantiate(moneyAddedEffect, moneyText.transform);
            effect.GetComponent<TextMeshProUGUI>().text = "+" + amount;
            Destroy(effect, 1.5f);
        }
    }
    
    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true;
        
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            
            // Déclencher l'événement
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        
        return false;
    }
    
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = string.Format(moneyFormat, currentMoney);
        }
    }
}
