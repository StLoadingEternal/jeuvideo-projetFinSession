using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI scoreText;
    public GameObject indications;

    [Header("Vagues")]
    public GameObject enemyPrefab;
    public Transform player;
    public int initialEnemies = 1;

    EnemySpawner enemySpawnerScript;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private int score = 0;
    private float waveDuration = 10f;

    private Coroutine countdownCoroutine;

    void Start()
    {
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
        int enemyCount = initialEnemies + (currentWave - 1);
        float horizontalSpeed = 10f + (currentWave - 1) * 5f;

        // Spawn des ennemis
        for (int i = 0; i < enemyCount; i++)
        {
            enemySpawnerScript.SpawnEnemy(horizontalSpeed, i);
            enemiesAlive++;
        }

        // Décompte (COROUTINE !!!)
        StartCountdown();

        // Attendre destruction des ennemis
        while (enemiesAlive > 0)
            yield return null;

        // Vague suivante
        StartCoroutine(StartWave());
    }

    void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(LancerDecompte());
    }

    private IEnumerator LancerDecompte()
    {
        float countdown = waveDuration;

        while (countdown > 0)
        {
            countdownText.text = "Temps : " + Mathf.Ceil(countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.text = "Temps : 0";
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

    void UpdateScoreUI()
    {
        scoreText.text = "Score : " + score;
    }
}
