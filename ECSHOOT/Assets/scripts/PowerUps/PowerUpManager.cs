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
        
        [Header("UI Timer")]
        public PowerUpTimerUI powerUpTimerUI;
        
        // États
        private bool isMultiShotActive = false;
        private float multiShotTimer = 0f;
        
        // Shield timer
        private bool isShieldPowerUpActive = false;
        private float shieldTimer = 0f;
        private Coroutine shieldExpireCoroutine;
        
        
        [Header("Speed Boost Settings")]
        public float speedMultiplier = 10f; // 50% plus rapide
        public float originalMoveSpeed; // Pour stocker la vitesse originale
        public float originalForwardSpeed; // Pour stocker la vitesse avant
        
        // Speed boost timer
        private bool isSpeedBoostActive = false;
        private float speedBoostTimer = 0f;
        private Coroutine speedBoostCoroutine;
        
        [Header("UI Link")]
        public PowerUpUILinker powerUpUILinker;
        
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
            //  le timer du multi-shot
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
        
       
            if (isSpeedBoostActive)
            {
                speedBoostTimer -= Time.deltaTime;
            }
        }
        
        // ============ MÉTHODES D'ACTIVATION ============
        
        
        public void CollectMultiShotPowerUp()
        {
            isMultiShotActive = true;
            multiShotTimer = powerUpDuration;
    
            if (blasterLeft != null && blasterRight != null)
            {
                blasterLeft.ActivateMultiShot(powerUpDuration);
                blasterRight.ActivateMultiShot(powerUpDuration);
            }
            
            if (powerUpUILinker != null)
                powerUpUILinker.OnPowerUpCollected("MultiShot", powerUpDuration);
        }


        public void CollectSpeedPowerUp()
        {
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine);
            }

            isSpeedBoostActive = true;
            speedBoostTimer = speedBoostDuration;
            ApplySpeedBoostToPlayer(true);
            speedBoostCoroutine = StartCoroutine(SpeedBoostExpireCountdown());
            
            if (powerUpUILinker != null)
                powerUpUILinker.OnPowerUpCollected("Speed", speedBoostDuration);
        }

        public void CollectShieldPowerUp()
        {
            if (shieldController == null) return;
    
            if (shieldExpireCoroutine != null)
            {
                StopCoroutine(shieldExpireCoroutine);
            }
    
            shieldController.ActivateShield(1f);
            shieldTimer = shieldPowerUpDuration;
            isShieldPowerUpActive = true;
            shieldExpireCoroutine = StartCoroutine(ShieldExpireCountdown());
            
            if (powerUpUILinker != null)
                powerUpUILinker.OnPowerUpCollected("Shield", shieldPowerUpDuration);
    
            ShowPowerUpMessage("Shield Activated!");
        }

        public void CollectFireRatePowerUp()
        {
            if (blasterLeft != null && blasterRight != null)
            {
                blasterLeft.ActivateFireRateBoost(powerUpDuration);
                blasterRight.ActivateFireRateBoost(powerUpDuration);
                
                if (powerUpUILinker != null)
                    powerUpUILinker.OnPowerUpCollected("FireRate", powerUpDuration);
            }
        }
        
        void DeactivateSpeedBoost()
        {
            if (!isSpeedBoostActive) return;

            isSpeedBoostActive = false;
            speedBoostTimer = 0f;
            ApplySpeedBoostToPlayer(false);
            
            if (powerUpUILinker != null)
                powerUpUILinker.OnPowerUpEnded("Speed");
        }

        
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
    
            if (shieldController != null)
            {
                shieldController.TriggerTimerBreakEffect();
            }
            
            if (powerUpUILinker != null)
                powerUpUILinker.OnPowerUpEnded("Shield");
        }
        
        // ============ UI ET FEEDBACK ============
        
        void ShowPowerUpMessage(string message)
        {
        
      
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
            
            if (powerUpTimerUI != null) powerUpTimerUI.StopAllTimers();
        }
        
        public float GetMultiShotTimeLeft() { return multiShotTimer; }
    
        public bool IsSpeedBoostActive() { return isSpeedBoostActive; }
        public float GetSpeedBoostTimeLeft() { return speedBoostTimer; }
    }
    
    
}