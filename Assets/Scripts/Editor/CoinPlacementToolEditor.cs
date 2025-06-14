using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CoinPlacementTool))]
public class CoinPlacementToolEditor : Editor
{
    private bool isPlacingCoins = false;
    private bool showPatternSettings = true;
    private bool showPrefabSettings = true;
    
    // Textures pour les boutons de sélection de pièces
    private Texture2D goldCoinTexture;
    private Texture2D silverCoinTexture;
    private Texture2D bronzeCoinTexture;
    
    private void OnEnable()
    {
        // Charger les textures (vous devrez créer ces textures ou utiliser des icônes Unity)
        goldCoinTexture = EditorGUIUtility.Load("Icons/GoldCoin.png") as Texture2D;
        silverCoinTexture = EditorGUIUtility.Load("Icons/SilverCoin.png") as Texture2D;
        bronzeCoinTexture = EditorGUIUtility.Load("Icons/BronzeCoin.png") as Texture2D;
        
        // Si les textures ne sont pas trouvées, utiliser des icônes par défaut
        if (goldCoinTexture == null) goldCoinTexture = EditorGUIUtility.FindTexture("d_PreMatCube");
        if (silverCoinTexture == null) silverCoinTexture = EditorGUIUtility.FindTexture("d_PreMatSphere");
        if (bronzeCoinTexture == null) bronzeCoinTexture = EditorGUIUtility.FindTexture("d_PreMatCylinder");
    }
    
    public override void OnInspectorGUI()
    {
        CoinPlacementTool tool = (CoinPlacementTool)target;
        
        // Vérifier que les prefabs sont bien des prefabs et pas des instances
        CheckPrefabStatus(tool);
        
        // Section des préfabriqués
        showPrefabSettings = EditorGUILayout.Foldout(showPrefabSettings, "Prefab Settings", true, EditorStyles.foldoutHeader);
        if (showPrefabSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("silverCoinPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bronzeCoinPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("parentObject"));
            
            EditorGUI.indentLevel--;
        }
        
        // Section des paramètres de placement
        EditorGUILayout.PropertyField(serializedObject.FindProperty("heightOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("placementLayerMask"));
        
        // Section des paramètres de motif
        showPatternSettings = EditorGUILayout.Foldout(showPatternSettings, "Pattern Settings", true, EditorStyles.foldoutHeader);
        if (showPatternSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentPattern"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patternRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemsCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spacing"));
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(10);
        
        // Sélection du type de pièce avec des boutons visuels
        EditorGUILayout.LabelField("Coin Type Selection", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.padding = new RectOffset(5, 5, 5, 5);
        buttonStyle.fixedWidth = 80;
        buttonStyle.fixedHeight = 80;
        
        // Bouton Or
        GUI.backgroundColor = tool.selectedCoinType == CoinPlacementTool.CoinType.Gold ? Color.yellow : Color.white;
        if (GUILayout.Button(new GUIContent(goldCoinTexture, "Gold Coin"), buttonStyle))
        {
            tool.selectedCoinType = CoinPlacementTool.CoinType.Gold;
        }
        
        // Bouton Argent
        GUI.backgroundColor = tool.selectedCoinType == CoinPlacementTool.CoinType.Silver ? Color.gray : Color.white;
        if (GUILayout.Button(new GUIContent(silverCoinTexture, "Silver Coin"), buttonStyle))
        {
            tool.selectedCoinType = CoinPlacementTool.CoinType.Silver;
        }
        
        // Bouton Bronze
        GUI.backgroundColor = tool.selectedCoinType == CoinPlacementTool.CoinType.Bronze ? new Color(0.8f, 0.4f, 0.1f) : Color.white;
        if (GUILayout.Button(new GUIContent(bronzeCoinTexture, "Bronze Coin"), buttonStyle))
        {
            tool.selectedCoinType = CoinPlacementTool.CoinType.Bronze;
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
                // Boutons de placement
        EditorGUILayout.LabelField("Placement Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button(isPlacingCoins ? "Stop Placing Coins" : "Start Placing Coins", GUILayout.Height(30)))
        {
            isPlacingCoins = !isPlacingCoins;
            SceneView.RepaintAll();
        }
        
        if (isPlacingCoins)
        {
            EditorGUILayout.HelpBox(
                "Click in the scene view to place coins using the selected pattern.\n" +
                "Current Pattern: " + tool.currentPattern + "\n" +
                "Current Coin: " + tool.selectedCoinType, 
                MessageType.Info);
        }
        
        if (GUILayout.Button("Clear All Coins", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All Coins", 
                "Are you sure you want to remove all placed coins?", 
                "Yes", "Cancel"))
            {
                tool.ClearAllCoins();
            }
        }
        
        // Ajouter un bouton pour appliquer les modifications aux prefabs
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Prefab Management", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Apply All Prefab Changes", GUILayout.Height(30)))
        {
            ApplyAllPrefabChanges(tool);
        }
    }
    
    private void CheckPrefabStatus(CoinPlacementTool tool)
    {
        // Vérifier que les références sont bien des prefabs et pas des instances
        if (tool.goldCoinPrefab != null && PrefabUtility.GetPrefabAssetType(tool.goldCoinPrefab) == PrefabAssetType.NotAPrefab)
        {
            EditorGUILayout.HelpBox("Gold Coin should be a prefab asset, not a scene instance!", MessageType.Warning);
        }
        
        if (tool.silverCoinPrefab != null && PrefabUtility.GetPrefabAssetType(tool.silverCoinPrefab) == PrefabAssetType.NotAPrefab)
        {
            EditorGUILayout.HelpBox("Silver Coin should be a prefab asset, not a scene instance!", MessageType.Warning);
        }
        
        if (tool.bronzeCoinPrefab != null && PrefabUtility.GetPrefabAssetType(tool.bronzeCoinPrefab) == PrefabAssetType.NotAPrefab)
        {
            EditorGUILayout.HelpBox("Bronze Coin should be a prefab asset, not a scene instance!", MessageType.Warning);
        }
    }
    
    private void ApplyAllPrefabChanges(CoinPlacementTool tool)
    {
        if (tool.parentObject == null) return;
        
        int appliedCount = 0;
        
        // Parcourir tous les enfants et appliquer les modifications aux prefabs
        foreach (Transform child in tool.parentObject)
        {
            // Vérifier si c'est un groupe de pattern
            if (child.childCount > 0)
            {
                foreach (Transform coinTransform in child)
                {
                    if (ApplyPrefabChange(coinTransform.gameObject))
                    {
                        appliedCount++;
                    }
                }
            }
            else
            {
                // C'est peut-être une pièce individuelle
                if (ApplyPrefabChange(child.gameObject))
                {
                    appliedCount++;
                }
            }
        }
        
        if (appliedCount > 0)
        {
            Debug.Log($"Applied changes to {appliedCount} prefab instances.");
        }
        else
        {
            Debug.Log("No prefab changes to apply.");
        }
    }
    
    private bool ApplyPrefabChange(GameObject instance)
    {
        // Vérifier si c'est une instance de prefab
        PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(instance);
        
        if (status == PrefabInstanceStatus.Connected || status == PrefabInstanceStatus.Disconnected)
        {
            // Appliquer les modifications à l'asset prefab
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);
            return true;
        }
        
        return false;
    }
    
    private void OnSceneGUI()
    {
        if (!isPlacingCoins) return;
        
        CoinPlacementTool tool = (CoinPlacementTool)target;
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 1000f, tool.placementLayerMask))
            {
                if (tool.currentPattern == CoinPlacementTool.PatternType.Single)
                {
                    Vector3 position = hit.point + hit.normal * tool.heightOffset;
                    tool.PlaceCoinAtPosition(position);
                }
                else
                {
                    tool.PlacePattern(hit.point, hit.normal);
                }
                
                e.Use();
            }
        }
        
        // Dessiner un aperçu du pattern sous le curseur
        Ray previewRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit previewHit;
        
        if (Physics.Raycast(previewRay, out previewHit, 1000f, tool.placementLayerMask))
        {
            Vector3 centerPos = previewHit.point + previewHit.normal * tool.heightOffset;
            
            // Créer une rotation basée sur la normale de la surface
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, previewHit.normal);
            
            // Dessiner l'aperçu en fonction du pattern sélectionné
            Handles.color = new Color(1, 1, 0, 0.5f);
            
            switch (tool.currentPattern)
            {
                case CoinPlacementTool.PatternType.Single:
                    Handles.DrawWireDisc(centerPos, previewHit.normal, 0.5f);
                    Handles.Label(centerPos + previewHit.normal * 0.5f, "Click to place coin");
                    break;
                    
                case CoinPlacementTool.PatternType.Line:
                    DrawLinePreview(centerPos, surfaceRotation, tool.itemsCount, tool.spacing);
                    break;
                    
                case CoinPlacementTool.PatternType.Circle:
                    Handles.DrawWireDisc(centerPos, previewHit.normal, tool.patternRadius);
                    DrawCirclePointsPreview(centerPos, surfaceRotation, tool.itemsCount, tool.patternRadius);
                    break;
                    
                case CoinPlacementTool.PatternType.Triangle:
                    DrawPolygonPreview(centerPos, surfaceRotation, 3, tool.patternRadius);
                    break;
                    
                case CoinPlacementTool.PatternType.Square:
                    DrawPolygonPreview(centerPos, surfaceRotation, 4, tool.patternRadius);
                    break;
                    
                case CoinPlacementTool.PatternType.Grid:
                    DrawGridPreview(centerPos, surfaceRotation, tool.itemsCount, tool.patternRadius);
                    break;
                    
                case CoinPlacementTool.PatternType.Spiral:
                    DrawSpiralPreview(centerPos, surfaceRotation, tool.itemsCount, tool.patternRadius);
                    break;
            }
            
            Handles.Label(centerPos + previewHit.normal * 1.5f, 
                $"Pattern: {tool.currentPattern} - Coin: {tool.selectedCoinType}");
            
            SceneView.RepaintAll();
        }
    }
    
    private void DrawLinePreview(Vector3 center, Quaternion rotation, int count, float spacing)
    {
        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) / 2f) * spacing;
            Vector3 localPos = new Vector3(offset, 0, 0);
            Vector3 worldPos = center + rotation * localPos;
            
            Handles.DrawWireDisc(worldPos, rotation * Vector3.up, 0.25f);
        }
        
        // Dessiner une ligne reliant tous les points
        Vector3[] linePoints = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) / 2f) * spacing;
            Vector3 localPos = new Vector3(offset, 0, 0);
            linePoints[i] = center + rotation * localPos;
        }
        
        Handles.DrawAAPolyLine(3f, linePoints);
    }
    
    private void DrawCirclePointsPreview(Vector3 center, Quaternion rotation, int count, float radius)
    {
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            
            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 worldPos = center + rotation * localPos;
            
            Handles.DrawWireDisc(worldPos, rotation * Vector3.up, 0.25f);
        }
    }
    
    private void DrawPolygonPreview(Vector3 center, Quaternion rotation, int sides, float radius)
    {
        float angleStep = 360f / sides;
        Vector3[] points = new Vector3[sides + 1];
        
        for (int i = 0; i <= sides; i++)
        {
            float angle = i % sides * angleStep;
            float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            
            Vector3 localPos = new Vector3(x, 0, z);
            points[i] = center + rotation * localPos;
            
            if (i < sides)
            {
                Handles.DrawWireDisc(points[i], rotation * Vector3.up, 0.25f);
            }
        }
        
        Handles.DrawAAPolyLine(3f, points);
    }
    
    private void DrawGridPreview(Vector3 center, Quaternion rotation, int count, float radius)
    {
        int sideLength = Mathf.CeilToInt(Mathf.Sqrt(count));
        float gridSpacing = radius / (sideLength - 1);
        
        int placedCount = 0;
        
        for (int x = 0; x < sideLength; x++)
        {
            for (int z = 0; z < sideLength; z++)
            {
                if (placedCount < count)
                {
                    float xPos = (x - (sideLength - 1) / 2f) * gridSpacing;
                    float zPos = (z - (sideLength - 1) / 2f) * gridSpacing;
                    
                    Vector3 localPos = new Vector3(xPos, 0, zPos);
                    Vector3 worldPos = center + rotation * localPos;
                    
                    Handles.DrawWireDisc(worldPos, rotation * Vector3.up, 0.25f);
                    placedCount++;
                }
            }
        }
        
        // Dessiner le cadre de la grille
        float halfSize = (sideLength - 1) * gridSpacing / 2;
        Vector3[] gridFrame = new Vector3[5];
        gridFrame[0] = center + rotation * new Vector3(-halfSize, 0, -halfSize);
        gridFrame[1] = center + rotation * new Vector3(halfSize, 0, -halfSize);
        gridFrame[2] = center + rotation * new Vector3(halfSize, 0, halfSize);
        gridFrame[3] = center + rotation * new Vector3(-halfSize, 0, halfSize);
        gridFrame[4] = gridFrame[0];
        
        Handles.DrawAAPolyLine(2f, gridFrame);
    }
    
    private void DrawSpiralPreview(Vector3 center, Quaternion rotation, int count, float maxRadius)
    {
        float spiralSpacing = maxRadius / count;
        float spiralAngleStep = 30f;
        float currentRadius = 0f;
        float currentAngle = 0f;
        
        Vector3[] spiralPoints = new Vector3[count];
        
        for (int i = 0; i < count; i++)
        {
            float x = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * currentRadius;
            float z = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * currentRadius;
            
            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 worldPos = center + rotation * localPos;
            
            spiralPoints[i] = worldPos;
            Handles.DrawWireDisc(worldPos, rotation * Vector3.up, 0.25f);
            
            currentAngle += spiralAngleStep;
            currentRadius += spiralSpacing;
        }
        
        Handles.DrawAAPolyLine(2f, spiralPoints);
    }
}

