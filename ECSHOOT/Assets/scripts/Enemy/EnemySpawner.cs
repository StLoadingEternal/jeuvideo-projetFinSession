using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //références
    public Transform player;
    public GameObject enemyPrefab;

    // distance fixe d'apparition devant le joueur
    private float spawnDistance = 100f; 

    // dispersion latérale d'apparition
    private float spread = 15f;        

    public void SpawnEnemy(float horizontalSpeed, int index, int currentWave)
    {
        // Vérifier si c'est une vague de boss via le GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && gameManager.IsBossWave)
        {
            return; // Ne pas spawn d'ennemis normaux pendant la vague de boss
        }

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

        //ils font face au joueur
        Quaternion spawnRot = Quaternion.Euler(0f, 180f, 0f);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        // vie et vitesse des ennemis  augment à chaque vague
        EnemyController ec = enemy.GetComponent<EnemyController>();
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        if (ec != null)
        {
            ec.moveSpeed = horizontalSpeed;
            
        }

        //J'ai augmenté les vies pour rendre plus challenge
        if (enemyHealth != null)
        {
            if (currentWave < 3)
                enemyHealth.maxHealth = 6;
            else if (currentWave < 5)
                enemyHealth.maxHealth = 8;
            else if (currentWave < 8)
                enemyHealth.maxHealth = 12;
            else
                enemyHealth.maxHealth = 16;
        }
    }
}