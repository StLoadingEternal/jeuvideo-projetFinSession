using UnityEngine;


public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject enemyPrefab;
    private float spawnDistance = 100f; // distance fixe devant le joueur
    private float spread = 20f;        // dispersion latérale


    void Start()
    {
       
    }

    private void Update()
    {
        
    }

    public void SpawnEnemy(float horizontalSpeed, int index, int currentWave)
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
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        if (ec != null)
        {
            ec.moveSpeed = horizontalSpeed;
            
        }

        // vie des ennemis 
        if (enemyHealth != null)
        {
            if (currentWave < 3)
                enemyHealth.maxHealth = 2;
            else if (currentWave < 5)
                enemyHealth.maxHealth = 3;
            else if (currentWave < 8)
                enemyHealth.maxHealth = 4;
            else
                enemyHealth.maxHealth = 5;

            
        }
    }

}

