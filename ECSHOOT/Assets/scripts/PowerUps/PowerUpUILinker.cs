using PowerUps;
using UnityEngine;

public class PowerUpUILinker : MonoBehaviour
{
    public PowerUpTimerUI powerUpTimerUI;
    public PowerUpManager powerUpManager;
    
    [Header("Slot IDs Configuration")]
    public string multiShotSlotId = "multishot";
    public string shieldSlotId = "shield";
    public string speedSlotId = "speed";
    public string fireRateSlotId = "firerate";

    void Start()
    {
        // Trouver les composants si non assignés
        if (powerUpTimerUI == null)
        {
            GameObject timerObj = GameObject.Find("PowerUpTimerUI");
            if (timerObj != null)
                powerUpTimerUI = timerObj.GetComponent<PowerUpTimerUI>();
        }

        if (powerUpManager == null)
        {
            GameObject managerObj = GameObject.Find("PowerUpManager");
            if (managerObj != null)
                powerUpManager = managerObj.GetComponent<PowerUpManager>();
        }
    }

    void Update()
    {
        SyncWithPowerUpManager();
    }

    void SyncWithPowerUpManager()
    {
        if (powerUpManager == null || powerUpTimerUI == null) return;

        // Shield - Mettre à jour le timer
        if (powerUpManager.IsShieldPowerUpActive())
        {
            float timeLeft = powerUpManager.GetShieldTimeLeft();
            powerUpTimerUI.UpdateTimerOnSlot(shieldSlotId, timeLeft);
        }
    }

    // Méthodes à appeler depuis PowerUpManager
    public void OnPowerUpCollected(string powerUpType, float duration)
    {
        if (powerUpTimerUI != null)
        {
            Debug.Log($"Power-up collecté: {powerUpType} ({duration}s)");
            
            // Utiliser StartTimerOnSlot avec l'ID du slot approprié
            switch (powerUpType.ToLower())
            {
                case "multishot":
                    powerUpTimerUI.StartTimerOnSlot(multiShotSlotId, powerUpType, duration);
                    break;
                    
                case "shield":
                    powerUpTimerUI.StartTimerOnSlot(shieldSlotId, powerUpType, duration);
                    break;
                    
                case "speed":
                    powerUpTimerUI.StartTimerOnSlot(speedSlotId, powerUpType, duration);
                    break;
                    
                case "firerate":
                    powerUpTimerUI.StartTimerOnSlot(fireRateSlotId, powerUpType, duration);
                    break;
                    
                default:
                    // Fallback: utiliser l'ancienne méthode
                    powerUpTimerUI.StartTimer(powerUpType, duration);
                    Debug.LogWarning($"Slot ID non configuré pour {powerUpType}");
                    break;
            }
        }
    }

    public void OnPowerUpEnded(string powerUpType)
    {
        if (powerUpTimerUI != null)
        {
            Debug.Log($"Power-up terminé: {powerUpType}");
            
            // Utiliser StopTimerOnSlot avec l'ID du slot approprié
            switch (powerUpType.ToLower())
            {
                case "multishot":
                    powerUpTimerUI.StopTimerOnSlot(multiShotSlotId);
                    break;
                    
                case "shield":
                    powerUpTimerUI.StopTimerOnSlot(shieldSlotId);
                    break;
                    
                case "speed":
                    powerUpTimerUI.StopTimerOnSlot(speedSlotId);
                    break;
                    
                case "firerate":
                    powerUpTimerUI.StopTimerOnSlot(fireRateSlotId);
                    break;
                    
                default:
                    // Fallback: utiliser l'ancienne méthode
                    powerUpTimerUI.StopTimer(powerUpType);
                    break;
            }
        }
    }
    
    // Méthode pour changer dynamiquement les IDs des slots
    public void ConfigureSlotIds(string multiId, string shieldId, string speedId, string fireRateId)
    {
        multiShotSlotId = multiId;
        shieldSlotId = shieldId;
        speedSlotId = speedId;
        fireRateSlotId = fireRateId;
    }
}