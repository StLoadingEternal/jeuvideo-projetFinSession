using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI scoreText;
    public GameObject indications;
    public GameObject gameOverPanel;
    public Image[] lifeImages;
    public Button retryButton;

    [Header("Vagues")]
    public GameObject enemyPrefab;
    public Transform playerPosition;
    public int initialEnemies = 1;

    private int currentWave = 0;
    public int enemiesAlive = 0;
    private int score = 0;
    private float waveDuration = 10f;

    public GameObject player;
    private PlayerController playerControllerScript;

    EnemySpawner enemySpawnerScript;

    public bool isGameOver;
    private Coroutine countdownCoroutine;

    private void Start()
    {
        //Désactiver les ui pas nécessaires

        playerControllerScript = player.GetComponent<PlayerController>();
        enemySpawnerScript = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        currentWave++;
        UpdateScoreUI();

        // Affichage vague (COROUTINE !!!)
        yield return StartCoroutine(AfficherVague());

        // Calcul de la difficulté
        int totalEnemies = initialEnemies + (currentWave - 1);
        float horizontalSpeed = 10f + (currentWave - 1) * 2f;

        int spawnedEnemies = 0;

        // Spawn des ennemis
        while (spawnedEnemies < totalEnemies)
        {
            // Si moins de 10 ennemis à l’écran, spawn
            if (enemiesAlive < 10)
            {
                enemySpawnerScript.SpawnEnemy(horizontalSpeed, spawnedEnemies, currentWave);
                enemiesAlive++;
                spawnedEnemies++;
            }
            else
            {
                // Sinon, attendre que des ennemis soient détruits
                yield return null;
            }
        }

        // Décompte (COROUTINE !!!)
        StartCountdown();

    }

    void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(LancerDecompte());
    }

    private IEnumerator LancerDecompte()
    {
        float countdown = waveDuration + (currentWave - 1) * 5;

        while (countdown > 0)
        {
            countdownText.text = "Temps : " + Mathf.Ceil(countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.text = "Temps : 0";

        // Temps fini --> perte de vie ET on passe à la vague suivante
        if (enemiesAlive > 0)
        {
            playerControllerScript.LoseLife(1);

            // Détruire tous les ennemis restants
            DestroyAllEnemies();

            // Important : remettre le compteur à 0
            enemiesAlive = 0;
        }

        // Lancer une nouvelle vague
        StartCoroutine(StartWave());
    }

    void DestroyAllEnemies()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }

    public IEnumerator AfficherVague()
    {
        waveText.text = "VAGUE " + currentWave;
        warningText.text = "Détruisez les vaisseaux avant le temps imparti";

        indications.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        indications.gameObject.SetActive(false);
        
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive--;
        score += 100;
        UpdateScoreUI();
    }

    public void UpdateLifeUI(int currentLives)
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].enabled = i < currentLives;
        }
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score : " + score;
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }


    public void RetryGame()
    {
        // Recharge la scène actuelle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
}
