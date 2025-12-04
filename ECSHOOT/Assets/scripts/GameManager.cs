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
    private float waveDuration = 30f;

    [Header("Système de Boss")]
    public GameObject bossPrefab;
    public int bossSpawnWaveInterval = 4; // Apparaît toutes les 4 vagues
    private int bossAppearanceCount = 0;
    private bool isBossWave = false;
    
    [Header("Paramètres de spawn du Boss")]
    public float bossSpawnDistance = 120f; // Distance de spawn (augmentez cette valeur)
    public float bossSpacing = 20f;        // Espace entre plusieurs bosses

    [Header("Audio")]
    public AudioSource gameOverSound;
    public AudioSource themeSound;
    public AudioSource destructionPlayerSound;

    public bool IsBossWave => isBossWave;

    public int bossAlive = 0;

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
    
    //Référence sur la couroutine du décompte
    private Coroutine countdownCoroutine;
    //Références sur le menu pause
    private MenuPause menuPauseScript;

    private void Start()
    {
        //Références sur le joueur et l'ennemi spawner
        playerControllerScript = player.GetComponent<PlayerController>();
        enemySpawnerScript = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();

        //Système de sauvegarde (Static à la place)
        saveSystem = new SaveSystem();

        //Appliquer la préférences de son et d'écran
        AudioListener.volume = GameSettings.MusicVolume;
        Screen.fullScreen = GameSettings.Fullscreen;

        //Initialiser correctement l'UI
        UpdateScoreUI();
        UpdateLifeUI();
        gameOverPanel.SetActive(false);

        //Référence sur le menu pause
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

    //Mis à jour vie
    public void UpdateLifeUI(int currentLives = 3)
    {
        int livesToDisplay = playerControllerScript.currentLives;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled = i < livesToDisplay;
            }
        }
    }

    //Mis à jour score
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

        //Jouer le son GameOver
        themeSound.Stop();
        destructionPlayerSound.Play();
        gameOverSound.Play();
    }

    public void RetryGame()
    {
        // Supprimer l'ancienne sauvegarde avant de recommencer
        DeleteSave();
        
        // Recharge la scène actuelle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    //Affiche un texte au début de chaque vague
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

    //Débuter une vague
    IEnumerator StartWave()
    {
        if (isGameOver)
            yield break;

        //Vague suivante
        currentWave++;
        UpdateScoreUI();
        UpdateLifeUI();

        // Vérifier si c'est une vague de boss
        isBossWave = (currentWave % bossSpawnWaveInterval == 0 && currentWave > 0);
        
        if (isBossWave)
        {
            yield return StartCoroutine(SpawnBossWave());
        }
        else
        {
            yield return StartCoroutine(AfficherVague());
            StartNormalWave();
        }
    }

    // NOUVELLE MÉTHODE : Démarrer une vague normale
    void StartNormalWave()
    {
        // Calcul de la difficulté
        int totalEnemies = initialEnemies + (currentWave - 1);
        float horizontalSpeed = 10f + (currentWave - 1) * 2f;

        int spawnedEnemies = 0;

        // Spawn des ennemis
        StartCoroutine(SpawnNormalEnemies(totalEnemies, horizontalSpeed));
    }

    // NOUVELLE MÉTHODE : Spawn progressif des ennemis normaux
    IEnumerator SpawnNormalEnemies(int totalEnemies, float horizontalSpeed)
    {
        int spawnedEnemies = 0;

        while (spawnedEnemies < totalEnemies)
        {
            if (enemiesAlive < 10)
            {
                enemySpawnerScript.SpawnEnemy(horizontalSpeed, spawnedEnemies, currentWave);
                enemiesAlive++;
                spawnedEnemies++;
                
                // Petite pause entre chaque spawn
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return null;
            }
        }

        StartCountdown();
    }

    // Vague de boss
    IEnumerator SpawnBossWave()
    {
        // Afficher un message spécial pour le boss
        if (waveText != null)
            waveText.text = "BOSS VAGUE " + bossAppearanceCount;
        if (warningText != null)
            warningText.text = "Attention! Boss en approche!";
        
        if (indications != null)
            indications.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(3f);
        
        if (indications != null)
            indications.gameObject.SetActive(false);
        
        // Spawn le(s) boss
        SpawnBoss();
        
        // Lancer le décompte pour le boss (plus long)
        StartBossCountdown();
    }

    void SpawnBoss()
    {
        if (bossPrefab == null) return;
    
        int numberOfBosses = (bossAppearanceCount >= 3) ? 2 : 1;
    
        for (int i = 0; i < numberOfBosses; i++)
        {
            float spawnDistance = 150f;
        
            Vector3 direction = player.transform.forward;
            direction.y = 0;
            direction.Normalize();
        
            Vector3 spawnPos = player.transform.position + direction * spawnDistance;
        
            // Espacement pour plusieurs bosses
            if (numberOfBosses > 1)
            {
                float spacing = 25f; // Augmenté l'espacement
                float startX = -(spacing * (numberOfBosses - 1)) / 2f;
                spawnPos += player.transform.right * (startX + i * spacing);
            }
        
            spawnPos.y = player.transform.position.y;
        
            // Orientation vers le joueur
            Quaternion spawnRot = Quaternion.LookRotation(player.transform.position - spawnPos);
        
            GameObject boss = Instantiate(bossPrefab, spawnPos, spawnRot);
        
            // Configurer la santé
            EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.maxHealth = 20 + (bossAppearanceCount * 10);
            }
        
            bossAlive++;
            enemiesAlive++;
        }
    }

    Vector3 CalculateBossSpawnPosition(int index, int totalBosses)
    {
        Vector3 direction = player.transform.forward;
        direction.y = 0;
        direction.Normalize();
    
        // Utilisez la variable bossSpawnDistance au lieu d'une valeur fixe
        Vector3 spawnPos = player.transform.position + direction * bossSpawnDistance;
    
        if (totalBosses > 1)
        {
            float startX = -(bossSpacing * (totalBosses - 1)) / 2f;
            spawnPos += player.transform.right * (startX + index * bossSpacing);
        }
    
        spawnPos.y = player.transform.position.y;
        return spawnPos;
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

    void StartBossCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        
        countdownCoroutine = StartCoroutine(BossDecompte());
    }

    private IEnumerator BossDecompte()
    {
        float countdown = 30f; // Temps plus long pour le boss
        
        while (countdown > 0 && bossAlive > 0)
        {
            if (isGameOver)
                yield break;
            
            if (countdownText != null)
                countdownText.text = "BOSS TEMPS: " + Mathf.Ceil(countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        
        if (bossAlive > 0)
        {
            // Pénalité si le boss n'est pas vaincu
            if (playerControllerScript != null)
            {
                playerControllerScript.LoseLife(2); // Pénalité plus sévère
            }
            
            DestroyAllEnemies();
        }
        
        // Passer à la vague suivante
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
        bossAlive = 0;
    }

    //Score augmente à la destruction de chaque ennemi
    public void OnEnemyDestroyed(bool isBoss = false)
    {
        enemiesAlive--;
        
        if (isBoss)
        {
            bossAlive--;
            score += 500; // Plus de points pour un boss
        }
        else
        {
            score += 100;
        }
        
        UpdateScoreUI();
       
        // Sauvegarder après un kill important
        if (isBoss)
        {
            SaveGame();
        }
    }
}