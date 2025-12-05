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

    // ============ SYSTÈME DE SAUVEGARDE ============

    [System.Serializable]
    public class GameState
    {
        public int score;
        public int lives;
        public int currentWave;
        public string saveDate;
        public int saveSlot;

        public GameState(int score, int lives, int currentWave, int slot = 1)
        {
            this.score = score;
            this.lives = lives;
            this.currentWave = currentWave;
            this.saveSlot = slot;
            this.saveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }
    }

    public static class SaveSystem
    {
        private static readonly string saveFolder = Application.persistentDataPath;
        private static string lastSaveSlotKey = "LastSaveSlot";

        public static void SaveGame(GameState state)
        {
            try
            {
                string savePath = Path.Combine(saveFolder, $"save_slot_{state.saveSlot}.json");
                string json = JsonConvert.SerializeObject(state, Formatting.Indented);
                File.WriteAllText(savePath, json);

                PlayerPrefs.SetInt(lastSaveSlotKey, state.saveSlot);
                PlayerPrefs.Save();

                Debug.Log($"Jeu sauvegardé dans le slot {state.saveSlot}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erreur sauvegarde : " + e.Message);
            }
        }

        public static GameState LoadFromSlot(int slot)
        {
            string savePath = Path.Combine(saveFolder, $"save_slot_{slot}.json");

            if (!File.Exists(savePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                GameState state = JsonConvert.DeserializeObject<GameState>(json);
                return state;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erreur chargement : " + e.Message);
                return null;
            }
        }

        public static GameState LoadLastSave()
        {
            int lastSlot = PlayerPrefs.GetInt(lastSaveSlotKey, 0);
            if (lastSlot > 0)
            {
                return LoadFromSlot(lastSlot);
            }
            return null;
        }

        public static bool HasSaveInSlot(int slot)
        {
            string savePath = Path.Combine(saveFolder, $"save_slot_{slot}.json");
            return File.Exists(savePath);
        }

        public static bool CheckHasSave()
        {
            for (int i = 1; i <= 3; i++)
            {
                if (HasSaveInSlot(i))
                {
                    return true;
                }
            }
            return false;
        }

        public static void DeleteSave(int slot)
        {
            string savePath = Path.Combine(saveFolder, $"save_slot_{slot}.json");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"Sauvegarde slot {slot} supprimée");
            }
        }

        public static void DeleteAllSaves()
        {
            for (int i = 1; i <= 3; i++)
            {
                DeleteSave(i);
            }
        }

        public static int GetLastSaveSlot()
        {
            return PlayerPrefs.GetInt(lastSaveSlotKey, 0);
        }

        public static int GetBestSlotForNewGame()
        {
            for (int i = 1; i <= 3; i++)
            {
                if (!HasSaveInSlot(i))
                {
                    return i;
                }
            }
            
            return FindOldestSaveSlot();
        }

        private static int FindOldestSaveSlot()
        {
            DateTime oldestDate = DateTime.MaxValue;
            int oldestSlot = 1;
            
            for (int i = 1; i <= 3; i++)
            {
                GameState save = LoadFromSlot(i);
                if (save != null && !string.IsNullOrEmpty(save.saveDate))
                {
                    if (DateTime.TryParse(save.saveDate, out DateTime saveDate))
                    {
                        if (saveDate < oldestDate)
                        {
                            oldestDate = saveDate;
                            oldestSlot = i;
                        }
                    }
                }
            }
            
            return oldestSlot;
        }
    }

    private void Start()
    {
        playerControllerScript = player.GetComponent<PlayerController>();
        enemySpawnerScript = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();

        
        
        // Appliquer la préférences de son et d'écran
        AudioListener.volume = GameSettings.MusicVolume;
        Screen.fullScreen = GameSettings.Fullscreen;

        // Initialiser correctement l'UI
        UpdateScoreUI();
        UpdateLifeUI();
        gameOverPanel.SetActive(false);

        menuPauseScript = MenuPause.GetComponent<MenuPause>();

        LoadGame();
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
            
            Debug.Log($"Partie sauvegardée dans le slot {slot}");
        }
    }

    public void LoadGame()
    {
        if (PlayerPrefs.GetInt("NewGame", 0) == 1)
        {
            Debug.Log("Nouvelle partie démarrée");
            isNewGame = true;
            
            score = 0;
            currentWave = 0;
            currentSaveSlot = PlayerPrefs.GetInt("NewGameSlot", 1);
            
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = 3;
            }

            PlayerPrefs.DeleteKey("NewGame");
            PlayerPrefs.DeleteKey("NewGameSlot");
            PlayerPrefs.DeleteKey("LoadSlot");
            PlayerPrefs.Save();
            
            UpdateScoreUI();
            UpdateLifeUI();
            return;
        }

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
            UpdateLifeUI();
            
            Debug.Log($"Partie chargée depuis le slot {state.saveSlot}");
            
            PlayerPrefs.DeleteKey("LoadSlot");
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("Nouvelle partie démarrée (pas de sauvegarde)");
            isNewGame = true;
            
            if (playerControllerScript != null)
            {
                playerControllerScript.currentLives = 3;
                UpdateLifeUI();
            }
            currentSaveSlot = 1;
        }
    }

    public int GetCurrentSaveSlot() { return currentSaveSlot; }

    public void SetCurrentSaveSlot(int slot)
    {
        currentSaveSlot = slot;
        PlayerPrefs.SetInt("CurrentSaveSlot", slot);
        PlayerPrefs.Save();
    }

    public void DeleteCurrentSave()
    {
        if (currentSaveSlot > 0)
        {
            SaveSystem.DeleteSave(currentSaveSlot);
        }
    }

    public int GetCurrentScore() { return score; }
    public int GetCurrentWave() { return currentWave; }
    public int GetCurrentLives() 
    { 
        if (playerControllerScript != null) 
            return playerControllerScript.currentLives; 
        return 3; 
    }

    public void StartNewGameInSlot(int slot)
    {
        SaveSystem.DeleteSave(slot);
        
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
        
        Debug.Log($"Nouvelle partie dans le slot {slot}");
    }

    public void StartNewGame()
    {
        StartNewGameInSlot(1);
    }

    // ============ GESTION DU JEU ============

    // Mis à jour vie (Attention problème d'affichages au début du jeu avec le playscriptcontroller)
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

    public void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);

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

        currentWave++;
        UpdateScoreUI();
        UpdateLifeUI();

        if (!isNewGame || currentWave > 1)
        {
            SaveGame();
        }
        
        if (isNewGame && currentWave > 1)
        {
            isNewGame = false;
        }

        isBossWave = (currentWave % bossSpawnWaveInterval == 0 && currentWave > 0);
        
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

    void StartNormalWave()
    {
        int totalEnemies = initialEnemies + (currentWave - 1);
        float horizontalSpeed = 10f + (currentWave - 1) * 2f;

        StartCoroutine(SpawnNormalEnemies(totalEnemies, horizontalSpeed));
    }

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
                
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return null;
            }
        }

        StartCountdown();
    }

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
                bossHealth.maxHealth = 20 + (bossAppearanceCount * 10);
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

    private IEnumerator LancerDecompte()
    {
        float countdown = waveDuration + (currentWave - 1) * 5;

        while (countdown > 0 && enemiesAlive > 0)
        {
            // Bloquer les vagues en cas de gameOver
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

            DestroyAllEnemies();
        }

        if (!isGameOver && playerControllerScript != null && playerControllerScript.currentLives > 0)
        {
            // Lancer une nouvelle vague
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