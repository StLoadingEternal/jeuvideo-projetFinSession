using UnityEngine;

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject enemyPrefab;

    public float minSpawnDistance = 30f;
    public float maxSpawnDistance = 80f;
    public float spread = 25f;

    void Start()
    {
        SpawnEnemy(); // Spawn une seule fois au démarrage
    }

    void SpawnEnemy()
    {
        Vector3 direction = player.forward;
        direction.y = 0; // Reste sur un plan horizontal
        
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 spawnPos = player.position + direction * distance;
        spawnPos += player.right * Random.Range(-spread, spread);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
