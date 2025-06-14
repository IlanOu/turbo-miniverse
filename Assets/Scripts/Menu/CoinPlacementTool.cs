using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CoinPlacementTool : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject goldCoinPrefab;
    public GameObject silverCoinPrefab;
    public GameObject bronzeCoinPrefab;
    public Transform parentObject;
    
    [Header("Placement Settings")]
    public float heightOffset = 0.5f;
    public LayerMask placementLayerMask = -1;
    
    [Header("Pattern Settings")]
    public PatternType currentPattern = PatternType.Single;
    public float patternRadius = 2f;
    public int itemsCount = 5;
    public float spacing = 1f;
    
    // Type de préfab actuellement sélectionné
    [HideInInspector]
    public CoinType selectedCoinType = CoinType.Gold;
    
    public enum PatternType
    {
        Single,
        Line,
        Circle,
        Triangle,
        Square,
        Grid,
        Spiral
    }
    
    public enum CoinType
    {
        Gold,
        Silver,
        Bronze
    }
    
    public GameObject GetSelectedPrefab()
    {
        switch (selectedCoinType)
        {
            case CoinType.Gold:
                return goldCoinPrefab;
            case CoinType.Silver:
                return silverCoinPrefab;
            case CoinType.Bronze:
                return bronzeCoinPrefab;
            default:
                return goldCoinPrefab;
        }
    }
    
    #if UNITY_EDITOR
    public void PlaceCoinAtPosition(Vector3 position)
    {
        GameObject prefab = GetSelectedPrefab();
        
        if (prefab == null)
        {
            Debug.LogError("Selected coin prefab not assigned!");
            return;
        }
        
        if (parentObject == null)
        {
            parentObject = transform;
        }
        
        // Utiliser PrefabUtility pour instancier correctement le prefab
        GameObject coin = PrefabUtility.InstantiatePrefab(prefab, parentObject) as GameObject;
        
        if (coin != null)
        {
            // Positionner et orienter l'instance
            coin.transform.position = position;
            coin.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            coin.name = selectedCoinType.ToString() + "_Coin_" + parentObject.childCount;
            
            // Enregistrer l'action pour l'annulation
            Undo.RegisterCreatedObjectUndo(coin, "Place Coin");
            
            // Marquer la scène comme modifiée
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
    
    public void PlacePattern(Vector3 centerPosition, Vector3 normal)
    {
        // Créer un groupe pour ce pattern
        GameObject patternGroup = new GameObject(currentPattern.ToString() + "_Pattern");
        patternGroup.transform.parent = parentObject;
        patternGroup.transform.position = centerPosition;
        
        // Orienter le groupe en fonction de la normale de la surface
        if (normal != Vector3.zero)
        {
            patternGroup.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
        }
        
        // Enregistrer la création du groupe pour l'annulation
        Undo.RegisterCreatedObjectUndo(patternGroup, "Place Pattern");
        
        List<Vector3> positions = new List<Vector3>();
        
        switch (currentPattern)
        {
            case PatternType.Single:
                positions.Add(Vector3.zero);
                break;
                
            case PatternType.Line:
                for (int i = 0; i < itemsCount; i++)
                {
                    float offset = (i - (itemsCount - 1) / 2f) * spacing;
                    positions.Add(new Vector3(offset, 0, 0));
                }
                break;
                
            case PatternType.Circle:
                float angleStep = 360f / itemsCount;
                for (int i = 0; i < itemsCount; i++)
                {
                    float angle = i * angleStep;
                    float x = Mathf.Sin(angle * Mathf.Deg2Rad) * patternRadius;
                    float z = Mathf.Cos(angle * Mathf.Deg2Rad) * patternRadius;
                    positions.Add(new Vector3(x, 0, z));
                }
                break;
                
            case PatternType.Triangle:
                for (int i = 0; i < 3; i++)
                {
                    float angle = i * 120f;
                    float x = Mathf.Sin(angle * Mathf.Deg2Rad) * patternRadius;
                    float z = Mathf.Cos(angle * Mathf.Deg2Rad) * patternRadius;
                    positions.Add(new Vector3(x, 0, z));
                }
                break;
                
            case PatternType.Square:
                positions.Add(new Vector3(-patternRadius, 0, -patternRadius));
                positions.Add(new Vector3(patternRadius, 0, -patternRadius));
                positions.Add(new Vector3(patternRadius, 0, patternRadius));
                positions.Add(new Vector3(-patternRadius, 0, patternRadius));
                break;
                
            case PatternType.Grid:
                int sideLength = Mathf.CeilToInt(Mathf.Sqrt(itemsCount));
                float gridSpacing = patternRadius / (sideLength - 1);
                
                for (int x = 0; x < sideLength; x++)
                {
                    for (int z = 0; z < sideLength; z++)
                    {
                        if (positions.Count < itemsCount)
                        {
                            float xPos = (x - (sideLength - 1) / 2f) * gridSpacing;
                            float zPos = (z - (sideLength - 1) / 2f) * gridSpacing;
                            positions.Add(new Vector3(xPos, 0, zPos));
                        }
                    }
                }
                break;
                
            case PatternType.Spiral:
                float spiralSpacing = patternRadius / itemsCount;
                float spiralAngleStep = 30f;
                float currentRadius = 0f;
                float currentAngle = 0f;
                
                for (int i = 0; i < itemsCount; i++)
                {
                    float x = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * currentRadius;
                    float z = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * currentRadius;
                    positions.Add(new Vector3(x, 0, z));
                    
                    currentAngle += spiralAngleStep;
                    currentRadius += spiralSpacing;
                }
                break;
        }
        
        // Placer les pièces aux positions calculées
        GameObject prefab = GetSelectedPrefab();
        
        // Créer un groupe d'annulation pour toutes les pièces
        Undo.IncrementCurrentGroup();
        int undoGroupIndex = Undo.GetCurrentGroup();
        
        foreach (Vector3 localPos in positions)
        {
            // Utiliser PrefabUtility pour instancier correctement le prefab
            GameObject coin = PrefabUtility.InstantiatePrefab(prefab, patternGroup.transform) as GameObject;
            
            if (coin != null)
            {
                // Positionner et orienter l'instance
                coin.transform.localPosition = localPos + Vector3.up * heightOffset;
                coin.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                coin.name = selectedCoinType.ToString() + "_Coin";
                
                // Enregistrer l'action pour l'annulation
                Undo.RegisterCreatedObjectUndo(coin, "Place Pattern Coin");
            }
        }
        
        // Grouper toutes les actions d'annulation
        Undo.CollapseUndoOperations(undoGroupIndex);
        
        // Marquer la scène comme modifiée
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
    
    public void ClearAllCoins()
    {
        if (parentObject == null) return;
        
        // Créer un groupe d'annulation
        Undo.IncrementCurrentGroup();
        int undoGroupIndex = Undo.GetCurrentGroup();
        
        // Enregistrer l'état actuel pour l'annulation
        Undo.RecordObject(parentObject, "Clear All Coins");
        
        // Supprimer tous les enfants
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in parentObject)
        {
            childrenToDestroy.Add(child.gameObject);
        }
        
        foreach (GameObject child in childrenToDestroy)
        {
            Undo.DestroyObjectImmediate(child);
        }
        
        // Grouper toutes les actions d'annulation
        Undo.CollapseUndoOperations(undoGroupIndex);
        
        // Marquer la scène comme modifiée
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
    #endif
}
