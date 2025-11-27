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
    public GameObject MenuPause;

    [Header("Vagues")]
    public int initialEnemies = 1;
    private int currentWave = 0;
    public int enemiesAlive = 0;
    private int score = 0;
    private float waveDuration = 10f;

    //Joueur
    public GameObject player;
    private PlayerController playerControllerScript;

    //Spawner
    private EnemySpawner enemySpawnerScript;

    //Game over
    private bool isGameOver;
    
    //Référence sur la couroutine du décompte
    private Coroutine countdownCoroutine;
    //Références sur le menu pause
    private MenuPause menuPauseScript;

    private void Start()
    {
        //Références sur le joueur et l'ennemi spawner
        playerControllerScript = player.GetComponent<PlayerController>();
        enemySpawnerScript = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();

        //Initialiser correctement l'UI
        UpdateScoreUI();
        UpdateLifeUI();
        gameOverPanel.SetActive(false);

        //Réference sur le menu pause
        menuPauseScript = MenuPause.GetComponent<MenuPause>();

        StartCoroutine(StartWave());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
                menuPauseScript.PauseGame();
            else
               menuPauseScript.ResumeGame();
        }
    }

    //Mis à jour vie
    public void UpdateLifeUI(int currentLives = 0)
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].enabled = i < currentLives;
        }
    }

    //Mis à jour score
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    //Affiche un texte au début de chague vague
    public IEnumerator AfficherVague()
    {
        waveText.text = "VAGUE " + currentWave;
        warningText.text = "Détruisez les vaisseaux avant le temps imparti";

        indications.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        indications.gameObject.SetActive(false);

    }

    //Débuter une vague
    IEnumerator StartWave()
    {
        //VAgue suivante
        currentWave++;
        //UpdateScoreUI();

        //On affiche le texte avant de commencer
        yield return StartCoroutine(AfficherVague());

        // Calcul de la difficulté ( La vitesse et le nombre d'ennemis augmentent)
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

        //On start le décompte à chaque vague
        //A  voir si on le garde
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
        //Chaque vague le temps augmente
        float countdown = waveDuration + (currentWave - 1) * 5;

        //Décompte
        while (countdown > 0)
        {
            countdownText.text = "Temps : " + Mathf.Ceil(countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.text = "Temps : 0";

        // Temps finit && ennemi restant > 0 = on passe à la vague suivante. Le joueur perd une vie
        if (enemiesAlive > 0)
        {
            playerControllerScript.LoseLife(1);

            // Détruire tous les ennemis restants
            DestroyAllEnemies();
            enemiesAlive = 0;
        }

        // Lancer une nouvelle vague
        StartCoroutine(StartWave());
    }

    //Détruire tous les ennemis quand le temps imparti est terminé
    void DestroyAllEnemies()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }

    //Score augmente à la destruction de chaque ennemi
    public void OnEnemyDestroyed()
    {
        enemiesAlive--;
        score += 100;
        UpdateScoreUI();
    }
    
}
