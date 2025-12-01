namespace PowerUps
{
    using UnityEngine;
    using System.Collections;

    public class PowerUpSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] powerUpPrefabs;
    
        [Header("Spawn Settings")]
        public float spawnInterval = 30f;
        public float initialDelay = 60f; // Délai avant le premier spawn
        public Transform[] spawnPoints;
    
        [Header("Chances")]
        [Range(0, 100)] public int spawnChance = 30; // 30% de chance
    
        void Start()
        {
            StartCoroutine(SpawnRoutine());
        }
    
        IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(initialDelay);
        
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
            
                // Chance de spawn
                if (Random.Range(0, 100) < spawnChance && spawnPoints.Length > 0)
                {
                    SpawnPowerUp();
                }
            }
        }
    
        void SpawnPowerUp()
        {
            // Choisir un point de spawn aléatoire
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
            // Choisir un power-up aléatoire
            GameObject powerUpPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        
            // Spawn
            Instantiate(powerUpPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}