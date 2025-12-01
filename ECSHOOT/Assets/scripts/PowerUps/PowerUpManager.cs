namespace PowerUps
{
    using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    [Header("Références")]
    private Blaster blaster;
    private PlayerController playerController;
    
    // États
    private bool isMultiShotActive = false;
    private float multiShotTimer = 0f;
    
    [Header("Durée des power-ups")]
    public float powerUpDuration = 10f;
    
    void Start()
    {
        blaster = GetComponent<Blaster>();
        playerController = GetComponent<PlayerController>();
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
                Debug.Log("Multi-Shot désactivé");
            }
        }
    }
    
    // ============ MÉTHODES D'ACTIVATION ============
    
    public void CollectFireRatePowerUp()
    {
        if (blaster != null)
        {
            // blaster.ActivateFireRateBoost(powerUpDuration);

        }
    }
    
    public void CollectMultiShotPowerUp()
    {
        isMultiShotActive = true;
        multiShotTimer = powerUpDuration;
        
        if (blaster != null)
        {
            // blaster.ActivateMultiShot(powerUpDuration);
        }
        

    }
    
    public void CollectSpeedPowerUp()
    {
        if (blaster != null)
        {
            // blaster.ActivateSpeedBoost(powerUpDuration);
        }
        

    }
    
    public void CollectShieldPowerUp()
    {
        if (playerController != null)
        {
            // playerController.ActivateShield(powerUpDuration);

        }
    }
    
    // ============ GETTERS ============
    
    public bool IsMultiShotActive() { return isMultiShotActive; }
    
    // ============ UI ============
    
    
}
}