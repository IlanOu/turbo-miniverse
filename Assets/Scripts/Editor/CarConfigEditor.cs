using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(CarConfig))]
public class CarConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Appliquer configuration rapide", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Sport"))
        {
            CarConfig config = (CarConfig)target;
            ApplyPreset(CarConfig.CreateSportConfig(), config);
        }
        
        if (GUILayout.Button("Drift"))
        {
            CarConfig config = (CarConfig)target;
            ApplyPreset(CarConfig.CreateDriftConfig(), config);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Monster Truck"))
        {
            CarConfig config = (CarConfig)target;
            ApplyPreset(CarConfig.CreateOffRoadConfig(), config);
        }
        
        if (GUILayout.Button("Rally"))
        {
            CarConfig config = (CarConfig)target;
            ApplyPreset(CarConfig.CreateRallyConfig(), config);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void ApplyPreset(CarConfig source, CarConfig destination)
    {
        Undo.RecordObject(destination, "Apply Car Configuration");
        
        // Copie des paramètres
        destination.motorSettings = source.motorSettings;
        destination.steeringSettings = source.steeringSettings;
        destination.wheelSettings = source.wheelSettings;
        destination.suspensionSettings = source.suspensionSettings;
        destination.driftSettings = source.driftSettings;
        destination.rigidbodySettings = source.rigidbodySettings;
        
        EditorUtility.SetDirty(destination);
    }
}
#endif