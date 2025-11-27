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

    //Références
    private EnemyController enemyController;
    private GameManager gameManager;

    //Destruction de l'ennemi
    private bool  isDead = false;

    //PArticules explosion
    public ParticleSystem hitEffectPrefab;

    void Start()
    {
        //Mis à jour de la vie de l'ennemi
        currentHealth = maxHealth;
        UpdateHealthUI();

        enemyController = GetComponent<EnemyController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }


    //l'ennemi prend des dégâts on met à jour l'ui
    public void TakeDamage(int damage)
    {
        //dégâts
        currentHealth -= damage;
        if (currentHealth < 0) 
            currentHealth = 0;
        //UI màj
        UpdateHealthUI();

        //Il meurt s'il n'a plus de vie
        if (currentHealth <= 0)
            Die();
    }

    //màj Barre de vie
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
