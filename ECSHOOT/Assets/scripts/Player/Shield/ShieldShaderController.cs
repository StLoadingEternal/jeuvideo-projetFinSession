using UnityEngine;
using System.Collections;
using PowerUps;

public class ShieldShaderController : MonoBehaviour
{
    [Header("Shield Visual Reference")]
    public GameObject shieldVisual; // La sphère du shield
    
    [Header("Effect Objects - ASSIGN IN INSPECTOR")]
    public GameObject activationWave;
    public GameObject hitRipple;
    public GameObject breakEffect;
    
    [Header("Shield Settings")]
    public float maxShieldHealth = 100f;
    public float shieldRegenRate = 5f; // Régénération par seconde
    public float activationDuration = 1.5f;
    
    [Header("Break Settings")]
    public float breakEffectDuration = 1.5f;
    public float warningDuration = 1f; // Durée du warning avant expiration
    
    [Header("Shader Properties")]
    [Range(0, 1)] public float shieldIntensity = 1f;
    public Color healthyColor = new Color(0.2f, 0.6f, 1f, 0.3f);
    public Color lowHealthColor = new Color(1f, 0.2f, 0.2f, 0.3f);
    public Color warningColor = new Color(1f, 1f, 0f, 0.3f); // Jaune pour warning
    public Color criticalColor = new Color(1f, 0.5f, 0f, 0.3f); // Orange pour critical
    
    // État interne
    private float currentShieldHealth;
    private Material shieldMaterial;
    private bool isShieldActive = false;
    private float lastHitTime;
    private bool isBreakingByTimer = false;
    
    // Référence au PowerUpManager
    private PowerUpManager powerUpManager;
    
    void Start()
    {
        // Trouver le PowerUpManager sur le parent (Fighter)
        powerUpManager = GetComponentInParent<PowerUpManager>();
        
        // Initialiser le shield
        InitializeShield();
    }
    
    void InitializeShield()
    {
        currentShieldHealth = 0f;
        isBreakingByTimer = false;
        
        // Configurer le shield visual
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
            Renderer renderer = shieldVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                shieldMaterial = renderer.material;
                
                // Initialiser les propriétés shader
                if (shieldMaterial != null)
                {
                    shieldMaterial.SetFloat("_ShieldHealth", 0f);
                    shieldMaterial.SetColor("_MainColor", healthyColor);
                }
            }
        }
        
        // Désactiver tous les effets
        SetEffectActive(activationWave, false);
        SetEffectActive(hitRipple, false);
        SetEffectActive(breakEffect, false);
    }
    
    void Update()
    {
        if (isShieldActive && !isBreakingByTimer)
        {
            // Régénération du shield
            if (currentShieldHealth < maxShieldHealth && Time.time - lastHitTime > 2f)
            {
                currentShieldHealth += shieldRegenRate * Time.deltaTime;
                currentShieldHealth = Mathf.Min(currentShieldHealth, maxShieldHealth);
            }
            
            // Mettre à jour le shader
            UpdateShaderProperties();
            
            // Clignotement si santé faible
            if (currentShieldHealth < maxShieldHealth * 0.3f)
            {
                UpdateLowHealthEffect();
            }
        }
    }
    
    // ============ MÉTHODES PUBLIQUES ============
    
    public void ActivateShield(float healthPercent = 1f)
    {
        if (isShieldActive)
        {
            // Réinitialiser si déjà actif
            ResetShield(healthPercent);
            return;
        }
        
        isShieldActive = true;
        isBreakingByTimer = false;
        currentShieldHealth = maxShieldHealth * healthPercent;
        
        // Activer le visual
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
            StartCoroutine(ActivationSequence());
        }
        

    }
    
    public void DeactivateShield(bool withBreakEffect = false)
    {
        if (!isShieldActive || isBreakingByTimer) return;
        
        isShieldActive = false;
        
        if (withBreakEffect)
        {
            // Jouer l'effet de cassure
            StartCoroutine(BreakSequence(false));
        }
        else
        {
            // Désactivation sans effet
            StartCoroutine(DeactivationSequence());
        }

    }
    
    public void TriggerTimerBreakEffect()
    {
        if (!isShieldActive || isBreakingByTimer) return;
        
        isBreakingByTimer = true;
        StartCoroutine(TimerBreakSequence());
    }
    
    public bool TakeShieldHit(float damage, Vector3 hitPosition)
    {
        if (!isShieldActive || currentShieldHealth <= 0 || isBreakingByTimer) return false;
        
        currentShieldHealth -= damage;
        lastHitTime = Time.time;
        
        // Jouer l'effet de hit
        PlayHitEffect(hitPosition);
        
        // Mettre à jour le shader pour le feedback visuel
        StartCoroutine(HitFeedbackSequence());
        
        if (currentShieldHealth <= 0)
        {
            // Shield cassé par dégâts
            StartCoroutine(BreakSequence(false));
            return false;
        }
        
        return true;
    }
    
    public void AddShieldHealth(float amount)
    {
        if (isBreakingByTimer) return;
        currentShieldHealth = Mathf.Min(currentShieldHealth + amount, maxShieldHealth);
    }
    
    // ============ SÉQUENCES D'EFFETS ============
    
    IEnumerator ActivationSequence()
    {
        float elapsed = 0f;
        
        // Activer l'effet d'activation
        SetEffectActive(activationWave, true);
        
        // Animation de montée en puissance
        while (elapsed < activationDuration)
        {
            float progress = elapsed / activationDuration;
            
            // Mettre à jour le shader
            if (shieldMaterial != null)
            {
                shieldMaterial.SetFloat("_ActivationProgress", progress);
                shieldMaterial.SetFloat("_ShieldHealth", progress);
            }
            
            // Mettre à jour l'effet d'activation
            if (activationWave != null)
            {
                Material waveMat = activationWave.GetComponent<Renderer>().material;
                waveMat.SetFloat("_ActivationTime", Time.time);
                waveMat.SetVector("_ActivationCenter", transform.position);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Fin de l'activation
        if (shieldMaterial != null)
        {
            shieldMaterial.SetFloat("_ActivationProgress", 1f);
            shieldMaterial.SetFloat("_ShieldHealth", currentShieldHealth / maxShieldHealth);
        }
        
        // Désactiver l'effet d'activation
        yield return new WaitForSeconds(0.5f);
        SetEffectActive(activationWave, false);
    }
    
    IEnumerator TimerBreakSequence()
    {
        
        // 1. PHASE DE WARNING (clignotement et surchauffe)
        float warningElapsed = 0f;
        
        while (warningElapsed < warningDuration)
        {
            float progress = warningElapsed / warningDuration;
            float pulse = Mathf.Sin(Time.time * 20) * 0.5f + 0.5f;
            
            if (shieldMaterial != null)
            {
                // Transition de couleur: Bleu → Jaune → Orange → Rouge
                Color currentColor;
                if (progress < 0.33f)
                    currentColor = Color.Lerp(healthyColor, warningColor, progress * 3);
                else if (progress < 0.66f)
                    currentColor = Color.Lerp(warningColor, criticalColor, (progress - 0.33f) * 3);
                else
                    currentColor = Color.Lerp(criticalColor, lowHealthColor, (progress - 0.66f) * 3);
                
                shieldMaterial.SetColor("_MainColor", currentColor);
                shieldMaterial.SetFloat("_RimPower", 3 + pulse * 2);
                shieldMaterial.SetFloat("_NoiseScale", 1 + progress * 3);
            }
            
            warningElapsed += Time.deltaTime;
            yield return null;
        }
        
        // 2. PHASE D'EXPLOSION
        SetEffectActive(breakEffect, true);
        
        if (shieldMaterial != null)
        {
            shieldMaterial.SetFloat("_BreakProgress", 0f);
        }
        
        float breakElapsed = 0f;
        
        while (breakElapsed < breakEffectDuration)
        {
            float progress = breakElapsed / breakEffectDuration;
            
            // Mettre à jour le shader principal
            if (shieldMaterial != null)
            {
                shieldMaterial.SetFloat("_BreakProgress", progress);
                shieldMaterial.SetFloat("_NoiseScale", 4 + progress * 3);
                
                // Fade out de la couleur
                Color currentColor = shieldMaterial.GetColor("_MainColor");
                currentColor.a = Mathf.Lerp(0.3f, 0f, progress);
                shieldMaterial.SetColor("_MainColor", currentColor);
            }
            
            // Mettre à jour l'effet de break
            if (breakEffect != null)
            {
                Material breakMat = breakEffect.GetComponent<Renderer>().material;
                breakMat.SetFloat("_BreakTime", Time.time);
                breakMat.SetFloat("_BreakProgress", progress);
                
                // Effet d'explosion centrifuge
                breakMat.SetVector("_ExplosionCenter", transform.position);
                breakMat.SetFloat("_ExplosionForce", progress * 2);
            }
            
            breakElapsed += Time.deltaTime;
            yield return null;
        }
        
        // 3. FINALISATION
        isShieldActive = false;
        isBreakingByTimer = false;
        
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        SetEffectActive(breakEffect, false);
        currentShieldHealth = 0;
        

    }
    
    IEnumerator BreakSequence(bool fromDamage = true)
    {
        if (isBreakingByTimer) yield break;
        
        string cause = fromDamage ? "dégâts" : "manuellement";
        
        // Activer l'effet de cassure
        SetEffectActive(breakEffect, true);
        
        if (shieldMaterial != null)
        {
            shieldMaterial.SetFloat("_BreakProgress", 0f);
        }
        
        // Animation de cassure
        float breakElapsed = 0f;
        
        while (breakElapsed < breakEffectDuration)
        {
            float progress = breakElapsed / breakEffectDuration;
            
            if (shieldMaterial != null)
            {
                shieldMaterial.SetFloat("_BreakProgress", progress);
            }
            
            if (breakEffect != null)
            {
                Material breakMat = breakEffect.GetComponent<Renderer>().material;
                breakMat.SetFloat("_BreakTime", Time.time);
                breakMat.SetFloat("_BreakProgress", progress);
            }
            
            breakElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Fin de la cassure
        if (shieldMaterial != null)
        {
            shieldMaterial.SetFloat("_BreakProgress", 1f);
        }
        
        // Désactiver le shield
        isShieldActive = false;
        
        // Désactiver l'effet de cassure
        yield return new WaitForSeconds(0.5f);
        SetEffectActive(breakEffect, false);
        
        // Désactiver le visual
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        currentShieldHealth = 0;
    }
    
    IEnumerator HitFeedbackSequence()
    {
        if (shieldMaterial == null) yield break;
        
        // Flash blanc
        shieldMaterial.SetColor("_HitColor", Color.white);
        shieldMaterial.SetFloat("_HitIntensity", 1f);
        
        yield return new WaitForSeconds(0.1f);
        
        // Retour à la normale
        float healthPercent = currentShieldHealth / maxShieldHealth;
        Color hitColor = Color.Lerp(lowHealthColor, healthyColor, healthPercent);
        shieldMaterial.SetColor("_HitColor", hitColor);
        
        // Fade out de l'intensité
        float fadeTime = 0.3f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            shieldMaterial.SetFloat("_HitIntensity", 1 - (elapsed / fadeTime));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        shieldMaterial.SetFloat("_HitIntensity", 0f);
    }
    
    IEnumerator DeactivationSequence()
    {
        if (shieldMaterial == null) yield break;
        
        // Fade out progressif
        float fadeDuration = 0.5f;
        float elapsed = 0f;
        float startHealth = shieldMaterial.GetFloat("_ShieldHealth");
        
        while (elapsed < fadeDuration)
        {
            float progress = elapsed / fadeDuration;
            shieldMaterial.SetFloat("_ShieldHealth", Mathf.Lerp(startHealth, 0f, progress));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Désactiver le visual
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        
        currentShieldHealth = 0;
    }
    
    // ============ EFFETS VISUELS ============
    
    void PlayHitEffect(Vector3 hitPosition)
    {
        if (hitRipple == null) return;
        
        // Positionner et activer l'effet
        hitRipple.transform.position = hitPosition;
        SetEffectActive(hitRipple, true);
        
        // Configurer le shader
        Material rippleMat = hitRipple.GetComponent<Renderer>().material;
        rippleMat.SetVector("_HitPosition", hitPosition);
        rippleMat.SetFloat("_HitTime", Time.time);
        
        // Auto-désactivation
        StartCoroutine(DisableEffectAfterDelay(hitRipple, 0.5f));
    }
    
    IEnumerator DisableEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetEffectActive(effect, false);
    }
    
    void UpdateLowHealthEffect()
    {
        if (shieldMaterial == null) return;
        
        // Clignotement rapide quand santé faible
        float pulseSpeed = currentShieldHealth < maxShieldHealth * 0.15f ? 20f : 10f;
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
        
        // Changer la couleur
        Color pulseColor = Color.Lerp(lowHealthColor, healthyColor, pulse);
        shieldMaterial.SetColor("_MainColor", pulseColor);
        
        // Augmenter l'intensité du rim
        float rimIntensity = 2f + pulse;
        shieldMaterial.SetFloat("_RimPower", rimIntensity);
    }
    
    void ResetShield(float healthPercent = 1f)
    {
        currentShieldHealth = maxShieldHealth * healthPercent;
        lastHitTime = Time.time;
        isBreakingByTimer = false;
        
        // Effet visuel de reset
        StartCoroutine(HitFeedbackSequence());
    }
    
    // ============ MISE À JOUR SHADER ============
    
    void UpdateShaderProperties()
    {
        if (shieldMaterial == null) return;
        
        float healthPercent = currentShieldHealth / maxShieldHealth;
        
        // Santé
        shieldMaterial.SetFloat("_ShieldHealth", healthPercent);
        
        // Couleur selon la santé (sauf pendant le break par timer)
        if (!isBreakingByTimer)
        {
            Color currentColor = Color.Lerp(lowHealthColor, healthyColor, healthPercent);
            shieldMaterial.SetColor("_MainColor", currentColor);
        }
        
        // Intensité
        shieldMaterial.SetFloat("_ShieldIntensity", shieldIntensity);
        
        // Distorsion quand endommagé
        float distortion = (1 - healthPercent) * 0.3f;
        shieldMaterial.SetFloat("_NoiseScale", 1 + distortion);
    }
    
    // ============ MÉTHODES UTILITAIRES ============
    
    void SetEffectActive(GameObject effect, bool active)
    {
        if (effect != null)
        {
            effect.SetActive(active);
        }
    }
    
    // ============ GETTERS ============
    
    public bool IsShieldActive()
    {
        return isShieldActive && currentShieldHealth > 0 && !isBreakingByTimer;
    }
    
    public bool IsBreakingByTimer()
    {
        return isBreakingByTimer;
    }
    
    public float GetShieldHealth()
    {
        return currentShieldHealth;
    }
    
    public float GetShieldHealthPercent()
    {
        return currentShieldHealth / maxShieldHealth;
    }
    
    public float GetMaxShieldHealth()
    {
        return maxShieldHealth;
    }
    
    void OnDisable()
    {
        StopAllCoroutines();
    }
}