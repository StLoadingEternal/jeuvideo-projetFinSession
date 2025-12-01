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

    //R�f�rences
    private EnemyController enemyController;
    private GameManager gameManager;

    //Destruction de l'ennemi
    private bool  isDead = false;

    public bool IsDead => isDead;

    //PArticules explosion
    public ParticleSystem hitEffectPrefab;

    void Start()
    {
        //Mis � jour de la vie de l'ennemi
        currentHealth = maxHealth;
        UpdateHealthUI();

        enemyController = GetComponent<EnemyController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }


    //l'ennemi prend des d�g�ts on met � jour l'ui
    public void TakeDamage(int damage)
    {
        //d�g�ts
        currentHealth -= damage;
        if (currentHealth < 0) 
            currentHealth = 0;
        //UI m�j
        UpdateHealthUI();

        //Il meurt s'il n'a plus de vie
        if (currentHealth <= 0)
            Die();
    }

    //m�j Barre de vie
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    //L'ennemi meurt
    void Die()
    {
        if (isDead) return;

        isDead = true;

        // Informe le GameManager
        if (gameManager != null)
            gameManager.OnEnemyDestroyed();

        // D�truit l�ennemi
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

