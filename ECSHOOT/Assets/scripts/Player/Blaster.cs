using UnityEngine;
using System.Collections;

public class Blaster : MonoBehaviour
{
    public Transform shootPoint;
    public GameObject projectilePrefab;

    private float fireRate = 0.5f;
    private float nextFire = 0f;

    private bool isMultiShot = true;

    [Header("Audio")]
    public AudioSource shotSound;

    // AJOUTS POUR LES POWER-UPS
    private bool isFireRateBoosted = false;
    private float fireRateMultiplier = 5f; // Tire 2x plus vite
    
    private bool isSpeedBoostActive = false;
    private float speedMultiplier = 1.5f; // 50% plus rapide
    
    private bool isDamageBoostActive = false;
    private int damageBonus = 1;
    
    // Timers (optionnel, si vous voulez durée limitée)
    private float fireRateBoostTimer = 0f;
    private float speedBoostTimer = 0f;
    private float damageBoostTimer = 0f;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFire)
        {
            // Calculer le fire rate (avec boost si actif)
            float currentFireRate = fireRate;
            if (isFireRateBoosted)
            {
                currentFireRate /= fireRateMultiplier; // Plus rapide = intervalle plus court
            }
            
            nextFire = Time.time + currentFireRate;

            if (isMultiShot)
            {
                if (GameSettings.FXEnabled) {
                    //shoot sound
                    shotSound.Play(); 
                }
                

                // Tirer 2 projectiles dans la même direction
                Shoot();
                Shoot(0.8f);
            }
            else
            {
                if (GameSettings.FXEnabled)
                {
                    //shoot sound
                    shotSound.Play();
                }

                Shoot();
            }
        }
        
        // Gérer les timers (optionnel)
        UpdatePowerUpTimers();
    }

    private void Shoot(float additionalPos = 0)
    {
        Vector3 worldPos = shootPoint.position + new Vector3(additionalPos, 0, 0);
        Quaternion rotation = Quaternion.Euler(-90, transform.eulerAngles.y, -90);

        GameObject p = Instantiate(projectilePrefab, worldPos, rotation);
        
        // AJOUT: Configurer les power-ups sur le projectile
        Projectile projectileScript = p.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Speed boost
            if (isSpeedBoostActive)
            {
                projectileScript.SetSpeedBoost(speedMultiplier);
            }
            
            
        }
        
        Rigidbody rb = p.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootPoint.forward * 20f;
        }
    }

    // ============ MÉTHODES POUR LES POWER-UPS (AJOUTS) ============
    
    // 1. MULTI-SHOT (déjà présent)
    public void ActivateMultiShot(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(MultiShotCoroutine(duration));
    }

    private IEnumerator MultiShotCoroutine(float duration)
    {
        isMultiShot = true;
        yield return new WaitForSeconds(duration);
        isMultiShot = false;
    }
    
    // 2. FIRE RATE BOOST (tirer plus vite)
    public void ActivateFireRateBoost(float duration)
    {
        isFireRateBoosted = true;
        fireRateBoostTimer = duration;
        
        // Option: timer automatique
        StartCoroutine(DeactivateAfterTime(() => isFireRateBoosted = false, duration));
    }
    
    // 3. SPEED BOOST (projectiles plus rapides)
    public void ActivateSpeedBoost(float duration)
    {
        isSpeedBoostActive = true;
        speedBoostTimer = duration;
        
        // Option: timer automatique
        StartCoroutine(DeactivateAfterTime(() => isSpeedBoostActive = false, duration));
    }
    
    // 4. DAMAGE BOOST (plus de dégâts)
    public void ActivateDamageBoost(float duration, int bonusDamage = 1)
    {
        isDamageBoostActive = true;
        damageBonus = bonusDamage;
        damageBoostTimer = duration;
        
        // Option: timer automatique
        StartCoroutine(DeactivateAfterTime(() => isDamageBoostActive = false, duration));
    }
    
    // ============ MÉTHODES UTILITAIRES (AJOUTS) ============
    
    // Coroutine générique pour désactiver après un temps
    private IEnumerator DeactivateAfterTime(System.Action deactivateAction, float duration)
    {
        yield return new WaitForSeconds(duration);
        deactivateAction.Invoke();
    }
    
    // Gestion des timers (optionnel)
    private void UpdatePowerUpTimers()
    {
        // À implémenter si vous voulez des timers précis
    }
    
    // Réinitialiser tous les power-ups
    public void ResetAllPowerUps()
    {
        isMultiShot = false;
        isFireRateBoosted = false;
        isSpeedBoostActive = false;
        isDamageBoostActive = false;
        
        // Arrêter toutes les coroutines
        StopAllCoroutines();
    }
    
    // GETTERS pour savoir quels power-ups sont actifs
    public bool IsMultiShotActive() { return isMultiShot; }
    public bool IsFireRateBoostActive() { return isFireRateBoosted; }
    public bool IsSpeedBoostActive() { return isSpeedBoostActive; }
    public bool IsDamageBoostActive() { return isDamageBoostActive; }
}