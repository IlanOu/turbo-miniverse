using Car;
using UnityEngine;

public class TeleportToGarage : MonoBehaviour
{
    [Header("Garage Teleport Settings")]
    [Tooltip("Point de spawn dans le garage")] public Transform garageSpawnPoint;
    [Tooltip("Touche pour téléporter")] public KeyCode teleportKey = KeyCode.T;

    private SmoothCamera cam;
    private CarController car;

    void Awake()
    {
        car = GetComponentInChildren<CarController>();
        cam = GetComponentInChildren<SmoothCamera>();
        if (garageSpawnPoint == null)
            Debug.LogError("GarageSpawnPoint non assigné dans TeleportToGarage", this);
    }

    void Update()
    {
        if (Input.GetKeyDown(teleportKey))
            Teleport();
    }
    
    public void Teleport()
    {
        if (garageSpawnPoint == null) return;
        
        car.CompletelyStopCar();
        cam.ResetCameraPosition();
        
        transform.position = garageSpawnPoint.position;
        transform.rotation = garageSpawnPoint.rotation;
                
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }
    }
}
