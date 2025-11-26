using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject enemyPrefab;
    private float spawnDistance = 100f; // distance fixe devant le joueur
    private float spread = 25f;        // dispersion latérale

    void Start()
    {
       
    }

    public void SpawnEnemy(float horizontalSpeed, int index)
    {
        // Direction devant le joueur sur le plan XZ
        Vector3 direction = player.forward;
        direction.y = 0;
        direction.Normalize();

        // Position initiale devant le joueur
        Vector3 spawnPos = player.position + direction * spawnDistance;

        // Ajouter une dispersion horizontale
        spawnPos += player.right * Random.Range(-spread, spread);

        // Décalage en Z pour éviter le chevauchement
        spawnPos.z += index * 30f;   // 10 unités derrière le précédent

        // Garder la même hauteur que le joueur
        spawnPos.y = player.position.y;

        Quaternion spawnRot = Quaternion.Euler(0f, 180f, 0f);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.moveSpeed = horizontalSpeed;
        }
    }

}

