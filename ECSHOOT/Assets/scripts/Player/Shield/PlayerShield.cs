namespace Player
{
    using UnityEngine;

    public class PlayerShield : MonoBehaviour
    {
        [Header("Shield Settings")]
        public float shieldDuration = 10f;       // Durée du bouclier
        public int maxShieldHealth = 3;          // Santé maximale du bouclier
        private int currentShieldHealth;         // Santé actuelle
        private float shieldTimer;               // Timer de durée
        private bool isShieldActive = false;     // État d'activation

        [Header("Shield Visual")]
        public GameObject shieldVisual;          // Objet avec shader
        public Material shieldMaterial;          // Matériau de base
        private Renderer shieldRenderer;         // Renderer du bouclier

        [Header("Shader Properties")]
        public Color fullShieldColor = new Color(0.3f, 0.7f, 1f, 0.3f);  // Couleur pleine santé
        public Color lowShieldColor = new Color(1f, 0.3f, 0.3f, 0.3f);    // Couleur basse santé

        [Header("Effects")]
        public ParticleSystem shieldActivateParticles;  // Particules d'activation
        public ParticleSystem shieldHitParticles;       // Particules de dégâts
        public ParticleSystem shieldBreakParticles;     // Particules de rupture
        public AudioClip shieldActivateSound;           // Son d'activation
        public AudioClip shieldHitSound;                // Son de dégâts
        public AudioClip shieldBreakSound;              // Son de rupture

        void Start()
        {
            // Initialise le visuel du bouclier
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
                
                // Désactive si timer écoulé ou santé épuisée
                if (shieldTimer <= 0 || currentShieldHealth <= 0)
                {
                    DeactivateShield();
                }
            }
        }

        // Active ou réinitialise le bouclier
        public void ActivateShield()
        {
            if (isShieldActive)
            {
                // Réinitialise si déjà actif
                shieldTimer = shieldDuration;
                currentShieldHealth = maxShieldHealth;
                return;
            }

            isShieldActive = true;
            shieldTimer = shieldDuration;
            currentShieldHealth = maxShieldHealth;

            // Active le visuel
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(true);
                UpdateShieldMaterial();
            }

            // Joue les effets
            PlayActivationEffects();
            Debug.Log("Bouclier activé!");
        }

        // Met à jour l'apparence du bouclier selon sa santé
        void UpdateShieldVisual()
        {
            if (!isShieldActive || shieldRenderer == null) return;

            Material mat = shieldRenderer.material;
            float healthPercent = (float)currentShieldHealth / maxShieldHealth;

            // Ajuste la couleur selon la santé
            Color shieldColor = Color.Lerp(lowShieldColor, fullShieldColor, healthPercent);
            mat.SetColor("_Color", shieldColor);

            // Ajuste les propriétés du shader
            mat.SetFloat("_RimPower", 2 + healthPercent * 2);
            mat.SetFloat("_PulseSpeed", 1 + (1 - healthPercent) * 3);

            // Effet flash si touché récemment
            if (Time.time - lastHitTime < 0.3f)
            {
                float flash = Mathf.PingPong(Time.time * 10, 1);
                mat.SetColor("_RimColor", Color.Lerp(shieldColor, Color.white, flash));
            }
        }

        // Initialise les propriétés du matériau
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

        private float lastHitTime = 0;  // Dernier moment d'impact

        // Applique des dégâts au bouclier
        public bool TakeShieldHit(int damage = 1)
        {
            if (!isShieldActive) return false;

            currentShieldHealth -= damage;
            lastHitTime = Time.time;

            // Effets de dégâts
            PlayHitEffects();
            StartCoroutine(ShieldHitFlash());

            // Vérifie si le bouclier est détruit
            if (currentShieldHealth <= 0)
            {
                PlayBreakEffects();
                return false;
            }

            return true;
        }

        // Effet flash sur le bouclier quand touché
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

        // Désactive le bouclier
        void DeactivateShield()
        {
            isShieldActive = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
            Debug.Log("Bouclier désactivé");
        }

        // Joue les effets d'activation
        void PlayActivationEffects()
        {
            if (shieldActivateParticles != null)
                shieldActivateParticles.Play();

            if (shieldActivateSound != null)
                AudioSource.PlayClipAtPoint(shieldActivateSound, transform.position);
        }

        // Joue les effets de dégâts
        void PlayHitEffects()
        {
            if (shieldHitParticles != null)
                shieldHitParticles.Play();

            if (shieldHitSound != null)
                AudioSource.PlayClipAtPoint(shieldHitSound, transform.position, 0.5f);
        }

        // Joue les effets de rupture
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