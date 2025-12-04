using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 50f;  // vitesse du projectile
    private float lifeTime = 2f; // durée avant destruction automatique
    
    // Variables pour les power-ups (ajouts seulement)
    private bool hasSpeedBoost = false;
    private float speedMultiplier = 1f;
    
    private bool hasPenetration = false;
    private int penetrationCount = 1;
    private int currentPenetration = 0;
    
    private int damage = 1; // Dégâts de base

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Va vers l'avant (Axe z) quand il est shooté
        float currentSpeed = speed;
        
        // Appliquer le speed boost si actif
        if (hasSpeedBoost)
        {
            currentSpeed *= speedMultiplier;
        }
        
        transform.position += transform.right * currentSpeed * Time.deltaTime;
    }
    
    // ============ MÉTHODES POUR LES POWER-UPS (AJOUTS SEULEMENT) ============
    
    // 1. SPEED BOOST - rend le projectile plus rapide
    public void SetSpeedBoost(float multiplier)
    {
        hasSpeedBoost = true;
        speedMultiplier = multiplier;
    }
    
    // 2. PENETRATION - permet de traverser plusieurs ennemis
    public void SetPenetration(int maxPenetrations)
    {
        hasPenetration = true;
        penetrationCount = maxPenetrations;
        currentPenetration = 0;
    }
    
    // 3. DAMAGE BOOST - augmente les dégâts
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // 4. Réinitialiser tous les power-ups
    public void ResetPowerUps()
    {
        hasSpeedBoost = false;
        speedMultiplier = 1f;
        hasPenetration = false;
        penetrationCount = 1;
        currentPenetration = 0;
        damage = 1;
    }
    
    // ============ COLLISION (MODIFICATION MINIMALE) ============
    
    private void OnTriggerEnter(Collider other)
    {
        // Vérifier si c'est un ennemi
        if (other.CompareTag("Enemy"))
        {
            // Appliquer les dégâts
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            
            // Gérer la pénétration
            if (hasPenetration)
            {
                currentPenetration++;
                
                // Vérifier si on a dépassé le nombre max de pénétration
                if (currentPenetration >= penetrationCount)
                {
                    Destroy(gameObject);
                }
                // Sinon, le projectile continue (ne pas le détruire)
            }
            else
            {
                // Pas de pénétration, détruire immédiatement
                Destroy(gameObject);
            }
        }
        
    }
}