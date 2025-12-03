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
    public bool isGameOver;
    public ParticleSystem destructionParticle;

    //Sauvegarde
    private SaveSystem saveSystem;
    
    //R�f�rence sur la couroutine du d�compte
    private Coroutine countdownCoroutine;
    //R�f�rences sur le menu pause
    private MenuPause menuPauseScript;

    private void Start()
    {
        //R�f�rences sur le joueur et l'ennemi spawner
        playerControllerScript = player.GetComponent<PlayerController>();
        enemySpawnerScript = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();

        //Système de sauvegarde
        saveSystem = new SaveSystem();

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
        if (isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
                menuPauseScript.PauseGame();
            else
               menuPauseScript.ResumeGame();
        }
    }

    // ============ SYSTÈME DE SAUVEGARDE ============

    public void SaveGame()
    {
        if (playerControllerScript != null)
        {
            saveSystem.SaveGame(score, playerControllerScript.currentLives, currentWave);
        }
    }

    public void LoadGame()
    {
        GameState state = SaveSystem.LoadStateFromSave();
        if (state != null)
        {
            // Appliquer l'état chargé
            score = state.score;
            currentWave = state.currentWave;
            
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = state.lives;
            }

            // Mettre à jour l'UI
            UpdateScoreUI();
            UpdateLifeUI();
            
            Debug.Log($"Partie chargée - Vague: {currentWave} Score: {score} Vies: {state.lives}");
        }
        else
        {
            Debug.Log("Nouvelle partie démarrée");
            // Initialiser les vies du joueur si nouvelle partie
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = 3;
                UpdateLifeUI();
            }
        }
    }

    public void DeleteSave()
    {
        SaveSystem.DeleteSave();
        Debug.Log("Sauvegarde supprimée");
    }

    // ============ GESTION DU JEU ============

    //Mis � jour vie
    public void UpdateLifeUI(int currentLives = -1)
    {
        int livesToDisplay = currentLives;
        
        if (currentLives == -1 && playerControllerScript != null)
        {
            livesToDisplay = playerControllerScript.currentLives;
        }

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled = i < livesToDisplay;
            }
        }
    }

    //Mis � jour score
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }

    public void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);

        destructionParticle.Play();

        // Arrêter le décompte
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        // Supprimer la sauvegarde
        DeleteSave();

        // Détruire les ennemis
        DestroyAllEnemies();

    }

    public void RetryGame()
    {
        // Supprimer l'ancienne sauvegarde avant de recommencer
        DeleteSave();
        
        // Recharge la scène actuelle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    //Affiche un texte au d�but de chague vague
    public IEnumerator AfficherVague()
    {
        if (waveText != null)
            waveText.text = "VAGUE " + currentWave;
        if (warningText != null)
            warningText.text = "Détruisez les vaisseaux avant le temps imparti";

        if (indications != null)
            indications.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (indications != null)
            indications.gameObject.SetActive(false);
    }

    //D�buter une vague
    IEnumerator StartWave()
    {
        if (isGameOver)
            yield break;

        //VAgue suivante
        currentWave++;
        UpdateScoreUI();
        UpdateLifeUI();

        //On affiche le texte avant de commencer
        yield return StartCoroutine(AfficherVague());

        // Calcul de la difficulté
        int totalEnemies = initialEnemies + (currentWave - 1);
        float horizontalSpeed = 10f + (currentWave - 1) * 2f;

        int spawnedEnemies = 0;

        // Spawn des ennemis
        while (spawnedEnemies < totalEnemies)
        {
            if (enemiesAlive < 10)
            {
                enemySpawnerScript.SpawnEnemy(horizontalSpeed, spawnedEnemies, currentWave);
                enemiesAlive++;
                spawnedEnemies++;
            }
            else
            {
                yield return null;
            }
        }

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
            if (isGameOver)
                yield break;

            if (countdownText != null)
                countdownText.text = "Temps : " + Mathf.Ceil(countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        if (countdownText != null)
            countdownText.text = "Temps : 0";

        if (enemiesAlive > 0)
        {
            if (playerControllerScript != null)
            {
                playerControllerScript.LoseLife(1);
            }

            //Détruire les ennemis à la fin du temps 
            DestroyAllEnemies();

        }

        // Vérifier si le jeu n'est pas terminé avant de lancer une nouvelle vague
        if (!isGameOver && playerControllerScript != null && playerControllerScript.currentLives > 0)
        {
            StartCoroutine(StartWave());
        }
    }

    void DestroyAllEnemies()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
            
        }
        enemiesAlive = 0;
    }

    //Score augmente � la destruction de chaque ennemi
    public void OnEnemyDestroyed()
    {
        enemiesAlive--;
        score += 100;
        UpdateScoreUI();
       
    }
}