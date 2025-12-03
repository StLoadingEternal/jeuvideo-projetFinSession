namespace Player
{
    using UnityEngine;

public class PlayerShield : MonoBehaviour

{

    [Header("Shield Settings")]

    public float shieldDuration = 10f;

    public int maxShieldHealth = 3;

    private int currentShieldHealth;

    private float shieldTimer;

    private bool isShieldActive = false;

   

    [Header("Shield Visual")]

    public GameObject shieldVisual; // L'objet avec le shader

    public Material shieldMaterial;

    private Renderer shieldRenderer;

    [Header("Shader Properties")]

    public Color fullShieldColor = new Color(0.3f, 0.7f, 1f, 0.3f);

    public Color lowShieldColor = new Color(1f, 0.3f, 0.3f, 0.3f);

    [Header("Effects")]

    public ParticleSystem shieldActivateParticles;

    public ParticleSystem shieldHitParticles;

    public ParticleSystem shieldBreakParticles;

    public AudioClip shieldActivateSound;

    public AudioClip shieldHitSound;

    public AudioClip shieldBreakSound;

    void Start()

    {

        // Initialiser le bouclier visuel

        if (shieldVisual != null)

        {

            shieldVisual.SetActive(false);

            shieldRenderer = shieldVisual.GetComponent<Renderer>();

            if (shieldMaterial != null && shieldRenderer != null)

            {

                shieldRenderer.material = new Material(shieldMaterial);

            }

        }

    }

    void Update()

    {

        if (isShieldActive)

        {

            shieldTimer -= Time.deltaTime;

            UpdateShieldVisual();

            if (shieldTimer <= 0 || currentShieldHealth <= 0)

            {

                DeactivateShield();

            }

        }

    }

    public void ActivateShield()

    {

        if (isShieldActive)

        {

            // Réinitialiser si déjà actif

            shieldTimer = shieldDuration;

            currentShieldHealth = maxShieldHealth;

            return;

        }

        isShieldActive = true;

        shieldTimer = shieldDuration;

        currentShieldHealth = maxShieldHealth;

        // Activer visuel

        if (shieldVisual != null)

        {

            shieldVisual.SetActive(true);

            UpdateShieldMaterial();

        }

        // Effets

        PlayActivationEffects();

        Debug.Log(" Bouclier activé!");

    }

    void UpdateShieldVisual()

    {

        if (!isShieldActive || shieldRenderer == null) return;

        Material mat = shieldRenderer.material;

        float healthPercent = (float)currentShieldHealth / maxShieldHealth;

        // Couleur selon la santé

        Color shieldColor = Color.Lerp(lowShieldColor, fullShieldColor, healthPercent);

        mat.SetColor("_Color", shieldColor);

        // Intensité du rim

        mat.SetFloat("_RimPower", 2 + healthPercent * 2);

        // Vitesse de pulsation (plus rapide quand faible)

        mat.SetFloat("_PulseSpeed", 1 + (1 - healthPercent) * 3);

        // Flash rouge quand touché récemment

        if (Time.time - lastHitTime < 0.3f)

        {

            float flash = Mathf.PingPong(Time.time * 10, 1);

            mat.SetColor("_RimColor", Color.Lerp(shieldColor, Color.white, flash));

        }

    }

    void UpdateShieldMaterial()

    {

        if (shieldRenderer == null) return;

        Material mat = shieldRenderer.material;

        mat.SetColor("_Color", fullShieldColor);

        mat.SetColor("_RimColor", Color.white);

        mat.SetFloat("_RimPower", 3);

        mat.SetFloat("_PulseSpeed", 1);

        mat.SetFloat("_SpinSpeed", 2);

    }

    private float lastHitTime = 0;

    public bool TakeShieldHit(int damage = 1)

    {

        if (!isShieldActive) return false;

        currentShieldHealth -= damage;

        lastHitTime = Time.time;

        // Effets de hit

        PlayHitEffects();

        // Feedback shader

        StartCoroutine(ShieldHitFlash());

        if (currentShieldHealth <= 0)

        {

            PlayBreakEffects();

            return false;

        }

        return true;

    }

    System.Collections.IEnumerator ShieldHitFlash()

    {

        if (shieldRenderer != null)

        {

            Material mat = shieldRenderer.material;

            Color originalRim = mat.GetColor("_RimColor");

            mat.SetColor("_RimColor", Color.white);

            mat.SetFloat("_RimPower", 1);

            yield return new WaitForSeconds(0.1f);

            mat.SetColor("_RimColor", originalRim);

            mat.SetFloat("_RimPower", 3);

        }

    }

    void DeactivateShield()

    {

        isShieldActive = false;

        if (shieldVisual != null)

            shieldVisual.SetActive(false);

        Debug.Log("Bouclier désactivé");

    }

    void PlayActivationEffects()
    {

        if (shieldActivateParticles != null)

            shieldActivateParticles.Play();

        if (shieldActivateSound != null)

            AudioSource.PlayClipAtPoint(shieldActivateSound, transform.position);

    }

    void PlayHitEffects()
    {
        if (shieldHitParticles != null)

            shieldHitParticles.Play();

        if (shieldHitSound != null)

            AudioSource.PlayClipAtPoint(shieldHitSound, transform.position, 0.5f);

    }

    void PlayBreakEffects()
    {
        if (shieldBreakParticles != null)

            shieldBreakParticles.Play();

        if (shieldBreakSound != null)

            AudioSource.PlayClipAtPoint(shieldBreakSound, transform.position);

    }

    // GETTERS pour l'UI

    public bool IsShieldActive() => isShieldActive;

    public float GetShieldTimeLeft() => shieldTimer;

    public int GetShieldHealth() => currentShieldHealth;

    public float GetShieldHealthPercent() => (float)currentShieldHealth / maxShieldHealth;

}
 
}