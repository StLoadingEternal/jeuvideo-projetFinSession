using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using System;
using UnityEngine.Serialization;

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
    
    
    [Header("Système de Boss")]
    public GameObject bossPrefab;
    public int bossSpawnWaveInterval = 4;
    private int bossAppearanceCount = 0;
    private bool isBossWave = false;
    
    [Header("Paramètres de spawn du Boss")]
    public float bossSpawnDistance = 120f;
    public float bossSpacing = 20f;

    [Header("Audio")]
    public AudioSource gameOverSound;
    public AudioSource themeSound;
    public AudioSource destructionPlayerSound;
    public AudioSource destructionSoundBoss;

    public bool IsBossWave => isBossWave;
    public int bossAlive = 0;

    public GameObject player;
    private PlayerController playerControllerScript;
    private EnemySpawner enemySpawnerScript;
    public bool isGameOver;
    public ParticleSystem destructionParticle;
    
    private Coroutine countdownCoroutine;
    private MenuPause menuPauseScript;
    private int currentSaveSlot = 0;
    private bool isNewGame = false;

  
    private void Start()
    {
        //mis en place des references
        playerControllerScript = player.GetComponent<PlayerController>();
        enemySpawnerScript = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();

        
        
        // Appliquer la préférences de son et d'écran
        AudioListener.volume = GameSettings.MusicVolume;
        Screen.fullScreen = GameSettings.Fullscreen;

        // Initialiser correctement l'UI
        gameOverPanel.SetActive(false);

        menuPauseScript = MenuPause.GetComponent<MenuPause>();

        LoadGame();
        StartCoroutine(StartWave());
    }

    private void Update()
    {
        if (isGameOver)
            return;
        // Trigger le menu pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1)
                menuPauseScript.PauseGame();
            else
               menuPauseScript.ResumeGame();
        }
    }

    // ============ SAUVEGARDE ============

    public void SaveGame(int slot = 0)
    {
        if (playerControllerScript != null)
        {
            if (slot == 0)
            {
                if (currentSaveSlot > 0)
                {
                    slot = currentSaveSlot;
                }
                else
                {
                    // on tient le slot loade en memoire
                    slot = PlayerPrefs.GetInt("LoadSlot", 0);
                    if (slot == 0) slot = 1;
                }
            }

            GameState state = new GameState(
                score, 
                playerControllerScript.currentLives, 
                currentWave,
                slot
            );

            SaveSystem.SaveGame(state);
            currentSaveSlot = slot;
            
        }
    }
    
    // cette methode permet de lancer une nouvelle partie ou une partie existante
    public void LoadGame()
    {
        if (PlayerPrefs.GetInt("NewGame", 0) == 1)
        {
            isNewGame = true;
            
            score = 0;
            currentWave = 0;
            currentSaveSlot = PlayerPrefs.GetInt("NewGameSlot", 1);
            
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = playerControllerScript.maxLives;
            }

            PlayerPrefs.DeleteKey("NewGame");
            PlayerPrefs.DeleteKey("NewGameSlot");
            PlayerPrefs.DeleteKey("LoadSlot");
            PlayerPrefs.Save();
            
            UpdateScoreUI();
            UpdateLifeUI();
            return;
        }
        
        // On load une partie existante
        int loadSlot = PlayerPrefs.GetInt("LoadSlot", 0);
        GameState state = null;

        if (loadSlot > 0)
        {
            state = SaveSystem.LoadFromSlot(loadSlot);
            currentSaveSlot = loadSlot;
        }
        else
        {
            state = SaveSystem.LoadLastSave();
            if (state != null)
            {
                currentSaveSlot = state.saveSlot;
            }
        }

        if (state != null)
        {
            score = state.score;
            currentWave = state.currentWave;
            
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = state.lives;
            }

            UpdateScoreUI();
            UpdateLifeUI(playerControllerScript.currentLives);
            
            PlayerPrefs.DeleteKey("LoadSlot");
            PlayerPrefs.Save();
        }
        else
        {
            isNewGame = true;
            
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = playerControllerScript.maxLives;
                UpdateLifeUI();
            }
            currentSaveSlot = 1;
        }
    }

    public int GetCurrentSaveSlot() { return currentSaveSlot; }
    
    
    // On tient en memoire le slot sur lequel la partie a ete lance
    // Ca va etre le meme sur lequel les futures sauvegarde du meme load iront
    public void SetCurrentSaveSlot(int slot)
    {
        currentSaveSlot = slot;
        PlayerPrefs.SetInt("CurrentSaveSlot", slot);
        PlayerPrefs.Save();
    }
    
    // Methode d'ecrasement
    public void DeleteCurrentSave()
    {
        if (currentSaveSlot > 0)
        {
            SaveSystem.DeleteSave(currentSaveSlot);
        }
    }

    
    // Commencer une nouvelle partie (en sauvegardant sur un slot)
    public void StartNewGameInSlot(int slot)
    {
        SaveSystem.DeleteSave(slot);
        
        // On reitialise les valeurs avant de commencer
        score = 0;
        currentWave = 0;
        bossAppearanceCount = 0;
        isNewGame = true;
        currentSaveSlot = slot;
        
        if (playerControllerScript != null)
        {
            playerControllerScript.currentLives = 3;
        }
        
        UpdateScoreUI();
        UpdateLifeUI();
        
        if (!isGameOver)
        {
            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);
            
            DestroyAllEnemies();
            StartCoroutine(StartWave());
        }
    }

    public void StartNewGame()
    {
        StartNewGameInSlot(1);
    }

    // ============ GESTION DU JEU ============

    // Mis à jour vie 
    public void UpdateLifeUI(int currentLives = 3)
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled = i < currentLives;
            }
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }
    
    // Fin de partie ( perte )
    public void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        
        // On joue des particules de destruction sur le joueur
        destructionParticle.Play();

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        DeleteCurrentSave();
        DestroyAllEnemies();

        // Jouer le son GameOver
        themeSound.Stop();
        destructionPlayerSound.Play();
        gameOverSound.Play();
    }
    
    public void RetryGame()
    {
        DeleteCurrentSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Mise a jour Ui montrant les informant de la vague qui commence
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
    
    
    IEnumerator StartWave()
    {
        // Bloquer les vagues en cas de gameOver
        if (isGameOver)
            yield break;
        
        // sauvegarder la partie a chague vague
        if (!isNewGame || currentWave > 1)
        {
            SaveGame();
        }
        
        if (isNewGame && currentWave > 1)
        {
            isNewGame = false;
        }

        isBossWave = (currentWave % bossSpawnWaveInterval == 0 && currentWave > 0);
        
        // vague de boss. Son apparence change avec l'apparition
        if (isBossWave)
        {
            bossAppearanceCount++;
            yield return StartCoroutine(SpawnBossWave());
        }
        else
        {
            yield return StartCoroutine(AfficherVague());
            StartNormalWave();
        }
    }

    
    // Une vague normal avec des enemis de base qui ne tirent pas 
    void StartNormalWave()
    {
        // Le nombre d'enemis est calcule en fonction de la vague
        int totalEnemies = initialEnemies + (currentWave - 1);
        float horizontalSpeed = 10f + (currentWave - 1) * 2f;

        StartCoroutine(SpawnNormalEnemies(totalEnemies, horizontalSpeed));
    }

    IEnumerator SpawnNormalEnemies(int totalEnemies, float horizontalSpeed)
    {
        int spawnedEnemies = 0;
        
        // Faire apparaitre la quantite d'enemi qu'il faut
        while (spawnedEnemies < totalEnemies)
        {
            if (enemiesAlive < 10)
            {
                enemySpawnerScript.SpawnEnemy(horizontalSpeed, spawnedEnemies, currentWave);
                enemiesAlive++;
                spawnedEnemies++;
                
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return null;
            }
        }

        StartCountdown();
    }
    
    
    // Afficher les messages qu'il faut pour la vague de Boss
    IEnumerator SpawnBossWave()
    {
        if (waveText != null)
            waveText.text = "BOSS VAGUE " + bossAppearanceCount;
        if (warningText != null)
            warningText.text = "Attention! Boss en approche!";
        
        if (indications != null)
            indications.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(3f);
        
        if (indications != null)
            indications.gameObject.SetActive(false);
        
        SpawnBoss();
        StartBossCountdown();
    }
    
    
    // Faire apparaitre le Boss
    void SpawnBoss()
    {
        if (bossPrefab == null) return;
        
        // Le nombre de Boss augmente avec les vagues
        int numberOfBosses = (bossAppearanceCount >= 3) ? 2 : 1;
    
        for (int i = 0; i < numberOfBosses; i++)
        {
            float spawnDistance = 150f;
        
            Vector3 direction = player.transform.forward;
            direction.y = 0;
            direction.Normalize();
        
            Vector3 spawnPos = player.transform.position + direction * spawnDistance;
        
            if (numberOfBosses > 1)
            {
                float spacing = 25f;
                float startX = -(spacing * (numberOfBosses - 1)) / 2f;
                spawnPos += player.transform.right * (startX + i * spacing);
            }
        
            spawnPos.y = player.transform.position.y;
        
            Quaternion spawnRot = Quaternion.LookRotation(player.transform.position - spawnPos);
        
            GameObject boss = Instantiate(bossPrefab, spawnPos, spawnRot);
        
            EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.maxHealth = 30 + (bossAppearanceCount * 10);
            }
        
            bossAlive++;
            enemiesAlive++;
        }
    }

    void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(LancerDecompte());
    }
    
    // Les enemis doivent etre tuer dans un delai
    // Cette methode lance le decompte
    private IEnumerator LancerDecompte()
    {
        float countdown = waveDuration + (currentWave - 1) * 5;

        while (countdown > 0 && enemiesAlive > 0)
        {
            // Bloquer les vagues en cas de gameOver
            if (isGameOver)
                yield break;
            
            // Afficher le temps
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

            DestroyAllEnemies();
        }

        if (!isGameOver && playerControllerScript != null && playerControllerScript.currentLives > 0)
        {
            // Lancer une nouvelle vague
            currentWave++;
            StartCoroutine(StartWave());
        }
    }
    
    // Le minuteur pour la vague du boss
    void StartBossCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        
        countdownCoroutine = StartCoroutine(BossDecompte());
    }
    
    private IEnumerator BossDecompte()
    {
        float countdown = 30f;
        
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
            if (playerControllerScript != null)
            {
                playerControllerScript.LoseLife(2);
            }
            
            DestroyAllEnemies();
        }
        
        if (!isGameOver && playerControllerScript != null && playerControllerScript.currentLives > 0)
        {
            currentWave++;
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

    public void OnEnemyDestroyed(bool isBoss = false)
    {
        enemiesAlive--;
        
        if (isBoss)
        {
            if (GameSettings.FXEnabled)
            {
                //Musique de destruction
                destructionSoundBoss.Play();
            }
            
            bossAlive--;
            score += 500;
        }
        else
        {
            score += 100;
        }
        
        UpdateScoreUI(); // Peut-être ne pas donner de point en cas de collision
       
        if (isBoss || currentWave % 3 == 0)
        {
            SaveGame();
        }
    }
}