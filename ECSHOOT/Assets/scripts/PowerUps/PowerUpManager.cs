using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace PowerUps
{
    public class PowerUpManager : MonoBehaviour
    {
        [FormerlySerializedAs("blaster")] [Header("Références")]
        public Blaster blasterLeft;
        public Blaster blasterRight;
        public ShieldShaderController shieldController;
        public PlayerController playerController; 
        
        [Header("Durée des power-ups")]
        public float powerUpDuration = 10f;
        public float shieldPowerUpDuration = 10f;
        public float speedBoostDuration = 10f; 
        
        // États
        private bool isMultiShotActive = false;
        private float multiShotTimer = 0f;
        
        // Shield timer
        private bool isShieldPowerUpActive = false;
        private float shieldTimer = 0f;
        private Coroutine shieldExpireCoroutine;
        
        
        [Header("Speed Boost Settings")]
        public float speedMultiplier = 2f; // 50% plus rapide
        public float originalMoveSpeed; // Pour stocker la vitesse originale
        public float originalForwardSpeed; // Pour stocker la vitesse avant
        
        // Speed boost timer
        private bool isSpeedBoostActive = false;
        private float speedBoostTimer = 0f;
        private Coroutine speedBoostCoroutine;
        
        
        void Start()
        {
            if (shieldController == null)
            {
                shieldController = GetComponentInChildren<ShieldShaderController>();
            }
            
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
                if (playerController == null)
                {
                    playerController = GetComponentInParent<PlayerController>();
                }
            }
            
            if (playerController != null)
            {
                originalForwardSpeed = playerController.ForwardSpeed;
                originalMoveSpeed = playerController.MoveSpeed;
            }
        }
        
        void Update()
        {
            // Gérer le timer du multi-shot
            if (isMultiShotActive)
            {
                multiShotTimer -= Time.deltaTime;
                if (multiShotTimer <= 0)
                {
                    isMultiShotActive = false;
                }
            }
        
            // Timer du shield (affichage seulement)
            if (isShieldPowerUpActive)
            {
                shieldTimer -= Time.deltaTime;
            }
        
            // AJOUT: Timer du speed boost
            if (isSpeedBoostActive)
            {
                speedBoostTimer -= Time.deltaTime;
            }
        }
        
        // ============ MÉTHODES D'ACTIVATION ============
        
        public void CollectFireRatePowerUp()
        {
            if (blasterLeft != null && blasterRight != null)
            {
                Debug.Log("Collecting fire rate powerup");
                blasterLeft.ActivateFireRateBoost(powerUpDuration);
                blasterRight.ActivateFireRateBoost(powerUpDuration);
            }
        }
        
        public void CollectMultiShotPowerUp()
        {
            isMultiShotActive = true;
            multiShotTimer = powerUpDuration;
            
            if (blasterLeft != null && blasterRight != null)
            {
                Debug.Log("Collecting multi shot powerup");
                blasterLeft.ActivateMultiShot(powerUpDuration);
                blasterRight.ActivateMultiShot(powerUpDuration);
            }
            

        }

        public void CollectSpeedPowerUp()
        {
            // Arrêter le boost précédent s'il y en a un
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine);
            }

            // Activer le speed boost
            isSpeedBoostActive = true;
            speedBoostTimer = speedBoostDuration;

            // Appliquer le boost au joueur
            ApplySpeedBoostToPlayer(true);

            // Démarrer la coroutine d'expiration
            speedBoostCoroutine = StartCoroutine(SpeedBoostExpireCountdown());

        }
        
        void DeactivateSpeedBoost()
        {
            if (!isSpeedBoostActive) return;
        
            isSpeedBoostActive = false;
            speedBoostTimer = 0f;
        
            // Retirer le boost du joueur
            ApplySpeedBoostToPlayer(false);
        
            
        }
        
        // AJOUT: Coroutine pour l'expiration du speed boost
        IEnumerator SpeedBoostExpireCountdown()
        {
            yield return new WaitForSeconds(speedBoostDuration);
        
            // Désactiver le speed boost
            DeactivateSpeedBoost();
        }
        
        private void ApplySpeedBoostToPlayer(bool activate)
        {
            if (playerController == null) return;
        
            // OPTION 1: Si vous avez des getters/setters publics dans PlayerController
            playerController.MoveSpeed = activate ? originalMoveSpeed * speedMultiplier : originalMoveSpeed;
            playerController.ForwardSpeed = activate ? originalForwardSpeed * speedMultiplier : originalForwardSpeed;
        
            // OPTION 2: Si vous préférez modifier directement via réflexion (moins recommandé)
            // System.Reflection.FieldInfo moveSpeedField = typeof(PlayerController).GetField("moveSpeed", 
            //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // if (moveSpeedField != null)
            // {
            //     float currentSpeed = (float)moveSpeedField.GetValue(playerController);
            //     if (activate)
            //     {
            //         // Stocker la vitesse originale avant de la modifier
            //         originalMoveSpeed = currentSpeed;
            //         moveSpeedField.SetValue(playerController, currentSpeed * speedMultiplier);
            //     }
            //     else
            //     {
            //         moveSpeedField.SetValue(playerController, originalMoveSpeed);
            //     }
            // }
        
            // OPTION 3: La meilleure approche - ajouter des méthodes dans PlayerController
            // Voir ci-dessous pour les modifications à apporter à PlayerController
        }
        

        public void CollectShieldPowerUp()
        {
            if (shieldController == null)
            {
                return;
            }
            
            // Arrêter la coroutine existante si elle tourne
            if (shieldExpireCoroutine != null)
            {
                StopCoroutine(shieldExpireCoroutine);
            }
            
            // Activer ou réinitialiser le shield
            shieldController.ActivateShield(1f);
            
            // Démarrer le timer avec effet de fin
            shieldTimer = shieldPowerUpDuration;
            isShieldPowerUpActive = true;
            
            // Démarrer la coroutine d'expiration
            shieldExpireCoroutine = StartCoroutine(ShieldExpireCountdown());
            
            ShowPowerUpMessage("Shield Activated!");
        }
        
        IEnumerator ShieldExpireCountdown()
        {
            // Attendre la durée du shield
            yield return new WaitForSeconds(shieldPowerUpDuration);
            
            // Désactiver le shield avec effet de breakage
            DeactivateShieldWithBreakEffect();
        }
        
        void DeactivateShieldWithBreakEffect()
        {
            if (!isShieldPowerUpActive) return;
            
            isShieldPowerUpActive = false;
            shieldTimer = 0f;
            
            // Jouer l'effet de breakage avant de désactiver
            if (shieldController != null)
            {
                shieldController.TriggerTimerBreakEffect();
            }
            
        }
        
        // ============ UI ET FEEDBACK ============
        
        void ShowPowerUpMessage(string message)
        {
        
            // Ici vous pouvez ajouter votre système UI
            // Par exemple: UIManager.Instance.ShowPowerUpMessage(message);
        }
        
        // ============ GETTERS ============
        
        public bool IsMultiShotActive() { return isMultiShotActive; }
        
        public bool IsShieldPowerUpActive() 
        { 
            return isShieldPowerUpActive && shieldController != null && shieldController.IsShieldActive(); 
        }
        
        public float GetShieldTimeLeft() { return shieldTimer; }
        
        public float GetShieldHealthPercent() 
        { 
            return shieldController != null ? shieldController.GetShieldHealthPercent() : 0f; 
        }
        
        // ============ MÉTHODES DE NETTOYAGE ============
        
        void OnDisable()
        {
            if (shieldExpireCoroutine != null)
            {
                StopCoroutine(shieldExpireCoroutine);
                shieldExpireCoroutine = null;
            }
        
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine);
                speedBoostCoroutine = null;
            }
        
            // S'assurer que le speed boost est désactivé
            if (isSpeedBoostActive)
            {
                DeactivateSpeedBoost();
            }
        }
        
        
    }
}