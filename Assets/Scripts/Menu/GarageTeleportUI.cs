using UnityEngine;

public class TeleportToGarage : MonoBehaviour
{
    [Header("Garage Teleport Settings")]
    [Tooltip("Point de spawn dans le garage")] public Transform garageSpawnPoint;
    [Tooltip("Touche pour téléporter")] public KeyCode teleportKey = KeyCode.T;

    private Rigidbody rb;
    private SmoothCamera cam;

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
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

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
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
