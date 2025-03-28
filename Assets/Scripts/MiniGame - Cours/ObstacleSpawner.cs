using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] Transform obstaclePrefab;

    [SerializeField] private float timeBetweenSpawns = 5f;
    
    float timeSinceLastSpawn = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // SpawnWave();
    }
    
    void Update()
    {
        // Loop for spawn wave every 5 seconds
        if (Time.time > timeSinceLastSpawn + timeBetweenSpawns)
        {
            SpawnWave();
            timeSinceLastSpawn = Time.time;
        }
    }

    void SpawnWave()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (i != randomIndex)
            {
                Instantiate(obstaclePrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }
    }
}
