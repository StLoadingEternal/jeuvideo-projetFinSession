using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PowerUpTimerUI : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpSlot
    {
        public string slotId;                // Identifiant unique pour le slot
        public Image radialTimer;           // Le cercle qui tourne
        public TextMeshProUGUI powerUpIdText; // Texte qui affiche l'ID (MULTI, SHIELD, etc.)
        public TextMeshProUGUI timerText;   // Texte qui affiche le temps
        public GameObject slotContainer;
        [HideInInspector] public string currentPowerUp;
        [HideInInspector] public bool isActive;
        [HideInInspector] public float remainingTime;
        [HideInInspector] public float totalDuration;
    }

    [Header("UI Slots")]
    public PowerUpSlot[] powerUpSlots;

    [Header("Text Colors")]
    public Color multiShotColor = Color.yellow;
    public Color shieldColor = Color.blue;
    public Color speedColor = Color.green;
    public Color fireRateColor = Color.red;

    [Header("Timer Colors")]
    public Color normalColor = new Color(0, 1, 1, 0.8f); // Cyan
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    private Dictionary<string, int> activePowerUps = new Dictionary<string, int>();
    private Dictionary<string, int> slotIdToIndex = new Dictionary<string, int>();

    void Start()
    {
        InitializeSlots();
        BuildSlotIdDictionary();
    }

    void BuildSlotIdDictionary()
    {
        slotIdToIndex.Clear();
        for (int i = 0; i < powerUpSlots.Length; i++)
        {
            if (powerUpSlots[i] != null && !string.IsNullOrEmpty(powerUpSlots[i].slotId))
            {
                string slotId = powerUpSlots[i].slotId.Trim().ToLower();
                slotIdToIndex[slotId] = i;
                Debug.Log($"Slot ID '{slotId}' mappé à l'index {i}");
            }
        }
    }

    void InitializeSlots()
    {
        foreach (var slot in powerUpSlots)
        {
            if (slot == null) continue;
            
            if (slot.slotContainer != null)
            {
                slot.slotContainer.SetActive(false);
            }
            
            slot.isActive = false;
            slot.currentPowerUp = "";
            slot.remainingTime = 0f;
            slot.totalDuration = 0f;
            
            // Initialiser le texte d'ID
            if (slot.powerUpIdText != null)
            {
                slot.powerUpIdText.text = "";
                slot.powerUpIdText.color = Color.white;
            }
            
            // Initialiser le texte de timer
            if (slot.timerText != null)
            {
                slot.timerText.text = "";
                slot.timerText.color = Color.white;
            }
            
            // Initialiser le timer radial
            if (slot.radialTimer != null)
            {
                slot.radialTimer.fillAmount = 0f;
                slot.radialTimer.color = normalColor;
            }
        }
    }

    void Update()
    {
        UpdateAllTimers();
    }

    // ============ MÉTHODES PUBLIQUES ============

    // Méthode pour démarrer un timer sur un slot spécifique par son ID
    public void StartTimerOnSlot(string slotId, string powerUpType, float duration)
    {
        string normalizedSlotId = slotId.Trim().ToLower();
        string normalizedPowerUpType = NormalizePowerUpType(powerUpType);
        
        if (!slotIdToIndex.ContainsKey(normalizedSlotId))
        {
            Debug.LogWarning($"Aucun slot trouvé avec l'ID: {slotId}");
            return;
        }
        
        int slotIndex = slotIdToIndex[normalizedSlotId];
        Debug.Log($"Démarrage timer sur slot {slotId} (index {slotIndex}): {normalizedPowerUpType} ({duration}s)");
        
        // Si ce slot est déjà utilisé pour un autre power-up, le libérer d'abord
        if (powerUpSlots[slotIndex].isActive && powerUpSlots[slotIndex].currentPowerUp != normalizedPowerUpType)
        {
            string oldPowerUp = powerUpSlots[slotIndex].currentPowerUp;
            if (activePowerUps.ContainsKey(oldPowerUp))
            {
                activePowerUps.Remove(oldPowerUp);
            }
        }
        
        // Si ce power-up est déjà actif sur un autre slot, le supprimer
        if (activePowerUps.ContainsKey(normalizedPowerUpType))
        {
            int oldSlotIndex = activePowerUps[normalizedPowerUpType];
            if (oldSlotIndex != slotIndex)
            {
                DeactivateSlot(oldSlotIndex);
                activePowerUps.Remove(normalizedPowerUpType);
            }
        }
        
        // Configurer le slot
        SetupSlot(slotIndex, normalizedPowerUpType, duration);
        activePowerUps[normalizedPowerUpType] = slotIndex;
    }

    // Méthode originale pour démarrer un timer sur n'importe quel slot disponible
    public void StartTimer(string powerUpType, float duration)
    {
        string normalizedType = NormalizePowerUpType(powerUpType);
        
        Debug.Log($"Démarrage timer: {normalizedType} ({duration}s)");
        int slotIndex;
        // Si déjà actif, réinitialiser
        if (activePowerUps.ContainsKey(normalizedType))
        {
            slotIndex = activePowerUps[normalizedType];
            ResetSlot(slotIndex, duration);
            return;
        }

        // Trouver un slot libre
        slotIndex = GetAvailableSlot();
        if (slotIndex == -1)
        {
            Debug.LogWarning("Plus de slots disponibles");
            return;
        }

        // Configurer le slot
        SetupSlot(slotIndex, normalizedType, duration);
        activePowerUps[normalizedType] = slotIndex;
    }

    public void StopTimer(string powerUpType)
    {
        string normalizedType = NormalizePowerUpType(powerUpType);
        
        if (activePowerUps.ContainsKey(normalizedType))
        {
            int slotIndex = activePowerUps[normalizedType];
            StartCoroutine(FlashAndDeactivateSlot(slotIndex));
            activePowerUps.Remove(normalizedType);
        }
    }

    // Méthode pour arrêter un timer sur un slot spécifique
    public void StopTimerOnSlot(string slotId)
    {
        string normalizedSlotId = slotId.Trim().ToLower();
        
        if (!slotIdToIndex.ContainsKey(normalizedSlotId))
        {
            Debug.LogWarning($"Aucun slot trouvé avec l'ID: {slotId}");
            return;
        }
        
        int slotIndex = slotIdToIndex[normalizedSlotId];
        if (powerUpSlots[slotIndex].isActive)
        {
            string powerUpType = powerUpSlots[slotIndex].currentPowerUp;
            StartCoroutine(FlashAndDeactivateSlot(slotIndex));
            if (activePowerUps.ContainsKey(powerUpType))
            {
                activePowerUps.Remove(powerUpType);
            }
        }
    }

    public void UpdateTimer(string powerUpType, float remainingTime)
    {
        string normalizedType = NormalizePowerUpType(powerUpType);
        
        if (activePowerUps.ContainsKey(normalizedType))
        {
            int slotIndex = activePowerUps[normalizedType];
            PowerUpSlot slot = powerUpSlots[slotIndex];
            
            if (slot.isActive)
            {
                slot.remainingTime = Mathf.Max(0, remainingTime);
                UpdateSlotDisplay(slot);
            }
        }
    }

    // Méthode pour mettre à jour un timer sur un slot spécifique
    public void UpdateTimerOnSlot(string slotId, float remainingTime)
    {
        string normalizedSlotId = slotId.Trim().ToLower();
        
        if (!slotIdToIndex.ContainsKey(normalizedSlotId))
        {
            Debug.LogWarning($"Aucun slot trouvé avec l'ID: {slotId}");
            return;
        }
        
        int slotIndex = slotIdToIndex[normalizedSlotId];
        PowerUpSlot slot = powerUpSlots[slotIndex];
        
        if (slot.isActive)
        {
            slot.remainingTime = Mathf.Max(0, remainingTime);
            UpdateSlotDisplay(slot);
        }
    }

    public void StopAllTimers()
    {
        foreach (var kvp in activePowerUps)
        {
            int slotIndex = kvp.Value;
            if (slotIndex >= 0 && slotIndex < powerUpSlots.Length)
            {
                DeactivateSlot(slotIndex);
            }
        }
        
        activePowerUps.Clear();
    }

    // Méthode pour vérifier si un slot spécifique est actif
    public bool IsSlotActive(string slotId)
    {
        string normalizedSlotId = slotId.Trim().ToLower();
        
        if (!slotIdToIndex.ContainsKey(normalizedSlotId))
        {
            Debug.LogWarning($"Aucun slot trouvé avec l'ID: {slotId}");
            return false;
        }
        
        int slotIndex = slotIdToIndex[normalizedSlotId];
        return powerUpSlots[slotIndex].isActive;
    }

    // Méthode pour obtenir le power-up actuel d'un slot spécifique
    public string GetCurrentPowerUpOnSlot(string slotId)
    {
        string normalizedSlotId = slotId.Trim().ToLower();
        
        if (!slotIdToIndex.ContainsKey(normalizedSlotId))
        {
            Debug.LogWarning($"Aucun slot trouvé avec l'ID: {slotId}");
            return "";
        }
        
        int slotIndex = slotIdToIndex[normalizedSlotId];
        if (powerUpSlots[slotIndex].isActive)
        {
            return powerUpSlots[slotIndex].currentPowerUp;
        }
        
        return "";
    }

    // ============ MÉTHODES PRIVÉES ============

    private void UpdateAllTimers()
    {
        for (int i = 0; i < powerUpSlots.Length; i++)
        {
            PowerUpSlot slot = powerUpSlots[i];
            
            if (slot.isActive && slot.remainingTime > 0)
            {
                slot.remainingTime -= Time.deltaTime;
                UpdateSlotDisplay(slot);
                
                if (slot.remainingTime <= 0)
                {
                    slot.remainingTime = 0;
                    StartCoroutine(FlashAndDeactivateSlot(i));
                }
            }
        }
    }

    private void UpdateSlotDisplay(PowerUpSlot slot)
    {
        if (!slot.isActive) return;
        
        // Mettre à jour le fill amount
        if (slot.radialTimer != null && slot.totalDuration > 0)
        {
            float fillPercent = slot.remainingTime / slot.totalDuration;
            slot.radialTimer.fillAmount = Mathf.Clamp01(fillPercent);
            
            // Changer la couleur du cercle
            UpdateTimerColor(slot.radialTimer, fillPercent);
        }
        
        // Mettre à jour le texte de timer
        if (slot.timerText != null)
        {
            if (slot.remainingTime > 1f)
            {
                slot.timerText.text = $"{slot.remainingTime:F1}s";
            }
            else
            {
                slot.timerText.text = $"{slot.remainingTime:F2}s";
            }
        }
    }

    private void SetupSlot(int slotIndex, string powerUpType, float duration)
    {
        if (slotIndex < 0 || slotIndex >= powerUpSlots.Length) return;
        
        PowerUpSlot slot = powerUpSlots[slotIndex];
        
        // Activer le slot
        slot.isActive = true;
        slot.currentPowerUp = powerUpType;
        slot.remainingTime = duration;
        slot.totalDuration = duration;
        
        // Configurer le texte d'ID
        if (slot.powerUpIdText != null)
        {
            string displayText = GetDisplayTextForPowerUp(powerUpType);
            slot.powerUpIdText.text = displayText;
            slot.powerUpIdText.color = GetColorForPowerUp(powerUpType);
        }

        // Configurer le timer radial
        if (slot.radialTimer != null)
        {
            slot.radialTimer.fillAmount = 1f;
            slot.radialTimer.color = normalColor;
        }

        // Configurer le texte de timer
        if (slot.timerText != null)
        {
            slot.timerText.text = $"{duration:F1}s";
            slot.timerText.color = Color.white;
        }
        
        // Activer le container
        if (slot.slotContainer != null)
        {
            slot.slotContainer.SetActive(true);
        }
        
        Debug.Log($"Slot {slotIndex} (ID: {slot.slotId}) configuré: {powerUpType}");
    }

    private void ResetSlot(int slotIndex, float duration)
    {
        if (slotIndex < 0 || slotIndex >= powerUpSlots.Length) return;
        
        PowerUpSlot slot = powerUpSlots[slotIndex];
        
        slot.remainingTime = duration;
        slot.totalDuration = duration;
        
        if (slot.radialTimer != null)
        {
            slot.radialTimer.fillAmount = 1f;
            slot.radialTimer.color = normalColor;
        }
        
        if (slot.timerText != null)
        {
            slot.timerText.text = $"{duration:F1}s";
        }
        
        Debug.Log($"Slot {slotIndex} réinitialisé: {duration}s");
    }

    private void UpdateTimerColor(Image timerImage, float fillPercent)
    {
        if (timerImage == null) return;
        
        if (fillPercent <= 0.1f) // 10%
        {
            timerImage.color = criticalColor;
        }
        else if (fillPercent <= 0.3f) // 30%
        {
            timerImage.color = warningColor;
        }
        else
        {
            timerImage.color = normalColor;
        }
    }

    private string GetDisplayTextForPowerUp(string powerUpType)
    {
        switch (powerUpType.ToLower())
        {
            case "multishot":
                return "MULTI";
            case "shield":
                return "SHIELD";
            case "speed":
            case "speedboost":
                return "SPEED";
            case "firerate":
            case "firerateboost":
                return "RAPID";
            default:
                return powerUpType.ToUpper();
        }
    }

    private Color GetColorForPowerUp(string powerUpType)
    {
        switch (powerUpType.ToLower())
        {
            case "multishot":
                return multiShotColor;
            case "shield":
                return shieldColor;
            case "speed":
            case "speedboost":
                return speedColor;
            case "firerate":
            case "firerateboost":
                return fireRateColor;
            default:
                return Color.white;
        }
    }

    private string NormalizePowerUpType(string powerUpType)
    {
        if (string.IsNullOrEmpty(powerUpType)) return "unknown";
        
        string normalized = powerUpType.ToLower().Trim();
        
        // Normalisation des noms
        if (normalized.Contains("multi") || normalized.Contains("shot"))
            return "multishot";
        if (normalized.Contains("shield") || normalized.Contains("protect"))
            return "shield";
        if (normalized.Contains("speed") || normalized.Contains("fast") || normalized.Contains("boost"))
            return "speed";
        if (normalized.Contains("fire") || normalized.Contains("rate") || normalized.Contains("rapid"))
            return "firerate";
        
        return normalized;
    }

    private IEnumerator FlashAndDeactivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= powerUpSlots.Length) yield break;
        
        PowerUpSlot slot = powerUpSlots[slotIndex];
        
        if (!slot.isActive) yield break;
        
        // Effet de clignotement sur le texte
        if (slot.powerUpIdText != null)
        {
            Color originalColor = slot.powerUpIdText.color;
            
            for (int i = 0; i < 3; i++)
            {
                slot.powerUpIdText.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                slot.powerUpIdText.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // Désactiver le slot
        DeactivateSlot(slotIndex);
        
        // Retirer du dictionnaire
        string powerUpType = slot.currentPowerUp;
        if (activePowerUps.ContainsKey(powerUpType))
        {
            activePowerUps.Remove(powerUpType);
        }
    }

    private void DeactivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= powerUpSlots.Length) return;
        
        PowerUpSlot slot = powerUpSlots[slotIndex];
        
        slot.isActive = false;
        slot.currentPowerUp = "";
        slot.remainingTime = 0f;
        slot.totalDuration = 0f;
        
        // Effacer les textes
        if (slot.powerUpIdText != null)
        {
            slot.powerUpIdText.text = "";
        }
        
        if (slot.timerText != null)
        {
            slot.timerText.text = "";
        }
        
        // Désactiver le container
        if (slot.slotContainer != null)
        {
            slot.slotContainer.SetActive(false);
        }
        
        Debug.Log($"Slot {slotIndex} (ID: {slot.slotId}) désactivé");
    }

    private int GetAvailableSlot()
    {
        for (int i = 0; i < powerUpSlots.Length; i++)
        {
            if (powerUpSlots[i] != null && !powerUpSlots[i].isActive)
            {
                return i;
            }
        }
        return -1;
    }
}