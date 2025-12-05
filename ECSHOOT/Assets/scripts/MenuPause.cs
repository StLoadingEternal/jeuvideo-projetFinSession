using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPause : MonoBehaviour
{
    public GameManager gameManagerScript;
    
    [Header("Save Menu UI")]
    public GameObject saveMenuPanel;
    public Button saveSlot1Button;
    public Button saveSlot2Button;
    public Button saveSlot3Button;
    public TextMeshProUGUI saveSlot1Info;
    public TextMeshProUGUI saveSlot2Info;
    public TextMeshProUGUI saveSlot3Info;
    public Button saveMenuCancelButton;
    
    [Header("UI Elements")]
    public GameObject saveNotificationPanel;
    public TextMeshProUGUI saveNotificationText;
    public float notificationDisplayTime = 2f;

    void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        if (saveNotificationPanel != null) saveNotificationPanel.SetActive(false);
        if (saveMenuPanel != null) saveMenuPanel.SetActive(false);
        
        if (saveSlot1Button != null) saveSlot1Button.onClick.AddListener(() => SaveToSlot(1));
        if (saveSlot2Button != null) saveSlot2Button.onClick.AddListener(() => SaveToSlot(2));
        if (saveSlot3Button != null) saveSlot3Button.onClick.AddListener(() => SaveToSlot(3));
        if (saveMenuCancelButton != null) saveMenuCancelButton.onClick.AddListener(CloseSaveMenu);
    }

    public void ResumeGame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        CloseSaveMenu();
    }

    public void PauseGame()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void QuitGame()
    {
        if (gameManagerScript != null)
        {   
            int currentSlot = gameManagerScript.GetCurrentSaveSlot();
            if (currentSlot == 0)
            {
                currentSlot = PlayerPrefs.GetInt("LoadSlot", 1);
            }
            
            gameManagerScript.SaveGame(currentSlot);
        }
        
        Time.timeScale = 1f;
        Invoke("LoadMenuScene", 1.5f);
    }

    private void LoadMenuScene()
    {
        SceneNavigator.GoToMenu();
    }

    // ============ MENU DE SAUVEGARDE MANUELLE ============

    public void OpenSaveMenu()
    {
        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(true);
            UpdateSaveMenuInfo();
        }
    }

    private void CloseSaveMenu()
    {
        if (saveMenuPanel != null)
        {
            saveMenuPanel.SetActive(false);
        }
    }

    private void UpdateSaveMenuInfo()
    {
        UpdateSlotInfo(1, saveSlot1Info);
        UpdateSlotInfo(2, saveSlot2Info);
        UpdateSlotInfo(3, saveSlot3Info);
    }

    private void UpdateSlotInfo(int slot, TextMeshProUGUI textUI)
    {
        if (textUI == null) return;
        
        GameState save = SaveSystem.LoadFromSlot(slot);
        if (save != null)
        {
            textUI.text = 
                         $"Vague {save.currentWave}\n" +
                         $"Score: {save.score}\n" +
                         $"{save.saveDate}";
        }
        else
        {
            textUI.text = $"Slot {slot}:\n(Vide)";
        }
    }

    private void SaveToSlot(int slot)
    {
        if (gameManagerScript != null)
        {
            gameManagerScript.SaveGame(slot);
            gameManagerScript.SetCurrentSaveSlot(slot);
            ShowSaveNotification($"Sauvegardé dans le Slot {slot}");
            CloseSaveMenu();
        }
    }

    // ============ NOTIFICATION ============

    private void ShowSaveNotification(string message)
    {
        if (saveNotificationPanel != null && saveNotificationText != null)
        {
            saveNotificationText.text = message;
            saveNotificationPanel.SetActive(true);
            Invoke("HideSaveNotification", notificationDisplayTime);
        }
    }

    private void HideSaveNotification()
    {
        if (saveNotificationPanel != null)
        {
            saveNotificationPanel.SetActive(false);
        }
    }

    // ============ AUTRES FONCTIONS ============

    public void NewGame()
    {
        SaveSystem.DeleteAllSaves();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}