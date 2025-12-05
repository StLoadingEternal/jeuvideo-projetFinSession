using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using System;

namespace SlimUI.ModernMenu
{
    public class UIMenuManager : MonoBehaviour
    {
        private Animator CameraObject;

        [Header("MENUS")]
        public GameObject mainMenu;
        public GameObject firstMenu;
        public GameObject playMenu;
        public GameObject exitMenu;
        public GameObject creditsMenu;
        public GameObject savesMenu;

        [Header("SAUVEGARDE")]
        public Button continueButton;
        public TextMeshProUGUI saveInfoText;
        public GameObject newGameConfirmationPanel;

        [Header("LOAD SAVES - UI Elements")]
        public Button saveSlot1Button;
        public Button saveSlot2Button;
        public Button saveSlot3Button;
        public TextMeshProUGUI saveSlot1Text;
        public TextMeshProUGUI saveSlot2Text;
        public TextMeshProUGUI saveSlot3Text;
        public GameObject saveSlot1Panel;
        public GameObject saveSlot2Panel;
        public GameObject saveSlot3Panel;
        public GameObject emptySlot1Panel;
        public GameObject emptySlot2Panel;
        public GameObject emptySlot3Panel;

        public enum Theme { custom1, custom2, custom3 };
        [Header("THEME SETTINGS")]
        public Theme theme;
        public ThemedUIData themeController;

        [Header("PANELS")]
        public GameObject mainCanvas;
        public GameObject PanelSkin;
        public GameObject PanelGame;

        [Header("SETTINGS SCREEN")]
        public GameObject lineGame;
        public GameObject lineSkin;

        [Header("SFX")]
        public AudioSource hoverSound;
        public AudioSource sliderSound;
        public AudioSource swooshSound;

        void Start()
        {
            CameraObject = transform.GetComponent<Animator>();

            //Appliquer les préférences au lancement 
            AudioListener.volume = GameSettings.MusicVolume;
            Screen.fullScreen = GameSettings.Fullscreen;

            playMenu.SetActive(false);
            exitMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(false);
            if (newGameConfirmationPanel) newGameConfirmationPanel.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            firstMenu.SetActive(true);
            mainMenu.SetActive(true);

            CheckSaveFile();
        }

       
        private void CheckSaveFile()
        {
            bool hasSave = SaveSystem.CheckHasSave();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;

                if (hasSave)
                {
                    GameState lastSave = SaveSystem.LoadLastSave();
                    if (lastSave != null)
                    {
                        TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = $"CONTINUER\nSlot {lastSave.saveSlot}: Vague {lastSave.currentWave}";
                        }

                        if (saveInfoText != null)
                        {
                            saveInfoText.text = $"Dernière partie:\nVague {lastSave.currentWave} - Score: {lastSave.score}";
                        }
                    }
                }
                else
                {
                    TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "CONTINUER\n(Aucune sauvegarde)";
                    }
                }
            }
        }

        public void LoadSavesInfo()
        {
            // Slot 1
            GameState save1 = SaveSystem.LoadFromSlot(1);
            if (save1 != null)
            {
                if (saveSlot1Text != null)
                {
                    saveSlot1Text.text = $"Vague {save1.currentWave}\nScore: {save1.score}\n{save1.saveDate}";
                }
                if (saveSlot1Panel != null) saveSlot1Panel.SetActive(true);
                if (emptySlot1Panel != null) emptySlot1Panel.SetActive(false);
                if (saveSlot1Button != null) saveSlot1Button.interactable = true;
            }
            else
            {
                if (saveSlot1Text != null)
                {
                    saveSlot1Text.text = "SLOT 1\n(Vide)";
                }
                if (saveSlot1Panel != null) saveSlot1Panel.SetActive(false);
                if (emptySlot1Panel != null) emptySlot1Panel.SetActive(true);
                if (saveSlot1Button != null) saveSlot1Button.interactable = false;
            }

            // Slot 2
            GameState save2 = SaveSystem.LoadFromSlot(2);
            if (save2 != null)
            {
                if (saveSlot2Text != null)
                {
                    saveSlot2Text.text = $"Vague {save2.currentWave}\nScore: {save2.score}\n{save2.saveDate}";
                }
                if (saveSlot2Panel != null) saveSlot2Panel.SetActive(true);
                if (emptySlot2Panel != null) emptySlot2Panel.SetActive(false);
                if (saveSlot2Button != null) saveSlot2Button.interactable = true;
            }
            else
            {
                if (saveSlot2Text != null)
                {
                    saveSlot2Text.text = "SLOT 2\n(Vide)";
                }
                if (saveSlot2Panel != null) saveSlot2Panel.SetActive(false);
                if (emptySlot2Panel != null) emptySlot2Panel.SetActive(true);
                if (saveSlot2Button != null) saveSlot2Button.interactable = false;
            }

            // Slot 3
            GameState save3 = SaveSystem.LoadFromSlot(3);
            if (save3 != null)
            {
                if (saveSlot3Text != null)
                {
                    saveSlot3Text.text = $"Vague {save3.currentWave}\nScore: {save3.score}\n{save3.saveDate}";
                }
                if (saveSlot3Panel != null) saveSlot3Panel.SetActive(true);
                if (emptySlot3Panel != null) emptySlot3Panel.SetActive(false);
                if (saveSlot3Button != null) saveSlot3Button.interactable = true;
            }
            else
            {
                if (saveSlot3Text != null)
                {
                    saveSlot3Text.text = "SLOT 3\n(Vide)";
                }
                if (saveSlot3Panel != null) saveSlot3Panel.SetActive(false);
                if (emptySlot3Panel != null) emptySlot3Panel.SetActive(true);
                if (saveSlot3Button != null) saveSlot3Button.interactable = false;
            }
        }

        // ============ FONCTIONS DE CHARGEMENT ============

        public void ContinueGame()
        {
            GameState lastSave = SaveSystem.LoadLastSave();
            if (lastSave != null)
            {
                PlaySwoosh();
                PlayerPrefs.SetInt("LoadSlot", lastSave.saveSlot);
                PlayerPrefs.Save();
                LoadScene("mainScene");
            }
            else
            {
                Debug.LogWarning("Aucune sauvegarde trouvée !");
                PlayHover();
            }
        }

        public void LoadGameSlot1() { LoadGameFromSlot(1); }
        public void LoadGameSlot2() { LoadGameFromSlot(2); }
        public void LoadGameSlot3() { LoadGameFromSlot(3); }

        private void LoadGameFromSlot(int slot)
        {
            if (SaveSystem.HasSaveInSlot(slot))
            {
                PlaySwoosh();
                PlayerPrefs.SetInt("LoadSlot", slot);
                PlayerPrefs.Save();
                LoadScene("mainScene");
            }
            else
            {
                Debug.LogWarning($"Aucune sauvegarde dans le slot {slot} !");
                PlayHover();
            }
        }

        // ============ FONCTIONS DE SUPPRESSION ============

        public void DeleteSaveSlot1() { DeleteSaveConfirmation(1); }
        public void DeleteSaveSlot2() { DeleteSaveConfirmation(2); }
        public void DeleteSaveSlot3() { DeleteSaveConfirmation(3); }

        private void DeleteSaveConfirmation(int slot)
        {
            PlayHover();
            SaveSystem.DeleteSave(slot);
            LoadSavesInfo();
            CheckSaveFile();
        }

        // ============ NOUVELLE PARTIE ============

        public void NewGame()
        {
            PlaySwoosh();

            int bestSlot = SaveSystem.GetBestSlotForNewGame();
            
            PlayerPrefs.SetInt("NewGameSlot", bestSlot);
            PlayerPrefs.SetInt("NewGame", 1);
            PlayerPrefs.Save();
            
            Debug.Log($"Nouvelle partie démarrée dans le slot {bestSlot}");

            if (SaveSystem.HasSaveInSlot(bestSlot) && newGameConfirmationPanel != null)
            {
                newGameConfirmationPanel.SetActive(true);
            }
            else
            {
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            int slot = PlayerPrefs.GetInt("NewGameSlot", 1);
            SaveSystem.DeleteSave(slot);
            
            if (newGameConfirmationPanel != null)
            {
                newGameConfirmationPanel.SetActive(false);
            }
            
            LoadScene("mainScene");
        }

        public void CancelNewGame()
        {
            PlayHover();

            if (newGameConfirmationPanel != null)
            {
                newGameConfirmationPanel.SetActive(false);
            }
        }

        // ============ NAVIGATION MENUS ============

        public void PlayCampaign()
        {
            PlaySwoosh();
            exitMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            playMenu.SetActive(true);
        }

        public void ReturnMenu()
        {
            PlaySwoosh();
            playMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(false);
            exitMenu.SetActive(false);
            if (newGameConfirmationPanel) newGameConfirmationPanel.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void DisablePlayCampaign()
        {
            playMenu.SetActive(false);
        }

        public void AreYouSure()
        {
            PlaySwoosh();
            exitMenu.SetActive(true);
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            DisablePlayCampaign();
        }

        public void CreditsMenu()
        {
            PlaySwoosh();
            playMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(true);
            if (savesMenu) savesMenu.SetActive(false);
            exitMenu.SetActive(false);
        }

        public void SavesMenus()
        {
            PlaySwoosh();
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu)
            {
                savesMenu.SetActive(true);
                LoadSavesInfo();
            }
            exitMenu.SetActive(false);
        }

        // ============ POSITION CAMERA ============

        public void Position2()
        {
            DisablePlayCampaign();
            CameraObject.SetFloat("Animate", 1);
        }

        public void Position1()
        {
            CameraObject.SetFloat("Animate", 0);
        }

        // ============ SETTINGS PANELS ============

        void DisablePanels()
        {
            PanelSkin.SetActive(false);
            PanelGame.SetActive(false);
            lineGame.SetActive(false);
            lineSkin.SetActive(false);
        }

        public void GamePanel()
        {
            DisablePanels();
            PanelGame.SetActive(true);
            lineGame.SetActive(true);
        }

        public void SkinPanel()
        {
            DisablePanels();
            PanelSkin.SetActive(true);
            lineSkin.SetActive(true);
        }

        // ============ FONCTIONS SCENE ============

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        // ============ SONS FX ============

        public void PlayHover()
        {
            hoverSound.Play();
        }

        public void PlaySFXHover()
        {
            sliderSound.Play();
        }

        public void PlaySwoosh()
        {
            swooshSound.Play();
        }

        // ============ QUITTER LE JEU ============

        public void QuitGame()
        {
            SceneNavigator.ExitApp();
        }
    }
}