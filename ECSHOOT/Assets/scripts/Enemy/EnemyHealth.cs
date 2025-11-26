using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{

    [Header("Vies")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("UI")]
    public Image healthBar;

    private EnemyController enemyController;
    private GameManager gameManager;

    private bool  isDead = false;

    public ParticleSystem hitEffectPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        enemyController = GetComponent<EnemyController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
           
            Die();
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        // Informe le GameManager
        if (gameManager != null)
            gameManager.OnEnemyDestroyed();

        // Détruit l’ennemi
        Destroy(gameObject);

        // Effet visuel
        if (hitEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }
    }

}
