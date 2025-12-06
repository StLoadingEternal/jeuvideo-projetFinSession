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
    /// <summary>
    /// Gestionnaire principal du menu UI. Contrôle la navigation, les sauvegardes et les paramètres du menu principal.
    /// </summary>
    public class UIMenuManager : MonoBehaviour
    {
        private Animator CameraObject; // Référence à l'Animator de la caméra pour les transitions

        [Header("MENUS")]
        public GameObject mainMenu; // Menu principal
        public GameObject firstMenu; // Premier menu (écran titre)
        public GameObject playMenu; // Menu de sélection de jeu
        public GameObject exitMenu; // Menu de confirmation de sortie
        public GameObject creditsMenu; // Menu des crédits
        public GameObject savesMenu; // Menu de gestion des sauvegardes

        [Header("SAUVEGARDE")]
        public Button continueButton; // Bouton "Continuer"
        public TextMeshProUGUI saveInfoText; // Texte d'information sur la sauvegarde
        public GameObject newGameConfirmationPanel; // Panel de confirmation pour nouvelle partie

        [Header("LOAD SAVES - UI Elements")]
        // Boutons et UI pour les 3 slots de sauvegarde
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

        // Enumération pour les thèmes disponibles
        public enum Theme { custom1, custom2, custom3 };
        
        [Header("THEME SETTINGS")]
        public Theme theme; // Thème actuel
        public ThemedUIData themeController; // Contrôleur de thème UI

        [Header("PANELS")]
        public GameObject mainCanvas; // Canvas principal
        public GameObject PanelSkin; // Panel des skins
        public GameObject PanelGame; // Panel des paramètres jeu

        [Header("SETTINGS SCREEN")]
        public GameObject lineGame; // Ligne décorative pour le panel jeu
        public GameObject lineSkin; // Ligne décorative pour le panel skin

        [Header("SFX")]
        public AudioSource hoverSound; // Son de survol
        public AudioSource sliderSound; // Son de slider
        public AudioSource swooshSound; // Son de transition

        /// <summary>
        /// Initialisation au démarrage
        /// </summary>
        void Start()
        {
            // Récupère l'Animator de la caméra
            CameraObject = transform.GetComponent<Animator>();

            // Applique les préférences sauvegardées
            AudioListener.volume = GameSettings.MusicVolume;
            Screen.fullScreen = GameSettings.Fullscreen;

            // Initialise l'état des menus
            playMenu.SetActive(false);
            exitMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(false);
            if (newGameConfirmationPanel) newGameConfirmationPanel.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            firstMenu.SetActive(true);
            mainMenu.SetActive(true);

            // Vérifie les sauvegardes existantes
            CheckSaveFile();
        }

        /// <summary>
        /// Vérifie l'existence de sauvegardes et met à jour le bouton Continuer
        /// </summary>
        private void CheckSaveFile()
        {
            bool hasSave = SaveSystem.CheckHasSave();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;

                if (hasSave)
                {
                    // Charge la dernière sauvegarde et affiche ses infos
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
                    // Aucune sauvegarde trouvée
                    TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "CONTINUER\n(Aucune sauvegarde)";
                    }
                }
            }
        }

        /// <summary>
        /// Charge et affiche les informations des sauvegardes pour les 3 slots
        /// </summary>
        public void LoadSavesInfo()
        {
            // ============ SLOT 1 ============
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

            // ============ SLOT 2 ============
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

            // ============ SLOT 3 ============
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

        /// <summary>
        /// Continue la dernière partie sauvegardée
        /// </summary>
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

        // Fonctions publiques pour les boutons de slots
        public void LoadGameSlot1() { LoadGameFromSlot(1); }
        public void LoadGameSlot2() { LoadGameFromSlot(2); }
        public void LoadGameSlot3() { LoadGameFromSlot(3); }

        /// <summary>
        /// Charge une partie depuis un slot spécifique
        /// </summary>
        /// <param name="slot">Numéro du slot (1-3)</param>
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

        // Fonctions publiques pour les boutons de suppression
        public void DeleteSaveSlot1() { DeleteSaveConfirmation(1); }
        public void DeleteSaveSlot2() { DeleteSaveConfirmation(2); }
        public void DeleteSaveSlot3() { DeleteSaveConfirmation(3); }

        /// <summary>
        /// Supprime une sauvegarde après confirmation
        /// </summary>
        /// <param name="slot">Numéro du slot à supprimer</param>
        private void DeleteSaveConfirmation(int slot)
        {
            PlayHover();
            SaveSystem.DeleteSave(slot);
            LoadSavesInfo(); // Met à jour l'affichage
            CheckSaveFile(); // Vérifie l'état des sauvegardes
        }

        // ============ NOUVELLE PARTIE ============

        /// <summary>
        /// Démarre une nouvelle partie
        /// </summary>
        public void NewGame()
        {
            PlaySwoosh();

            // Trouve le meilleur slot pour une nouvelle partie
            int bestSlot = SaveSystem.GetBestSlotForNewGame();
            
            PlayerPrefs.SetInt("NewGameSlot", bestSlot);
            PlayerPrefs.SetInt("NewGame", 1);
            PlayerPrefs.Save();
            
            Debug.Log($"Nouvelle partie démarrée dans le slot {bestSlot}");

            // Affiche une confirmation si le slot contient déjà une sauvegarde
            if (SaveSystem.HasSaveInSlot(bestSlot) && newGameConfirmationPanel != null)
            {
                newGameConfirmationPanel.SetActive(true);
            }
            else
            {
                StartNewGame();
            }
        }

        /// <summary>
        /// Confirme et démarre réellement la nouvelle partie
        /// </summary>
        public void StartNewGame()
        {
            int slot = PlayerPrefs.GetInt("NewGameSlot", 1);
            SaveSystem.DeleteSave(slot); // Nettoie l'ancienne sauvegarde
            
            if (newGameConfirmationPanel != null)
            {
                newGameConfirmationPanel.SetActive(false);
            }
            
            LoadScene("mainScene");
        }

        /// <summary>
        /// Annule le démarrage d'une nouvelle partie
        /// </summary>
        public void CancelNewGame()
        {
            PlayHover();

            if (newGameConfirmationPanel != null)
            {
                newGameConfirmationPanel.SetActive(false);
            }
        }

        // ============ NAVIGATION MENUS ============

        /// <summary>
        /// Ouvre le menu de sélection de campagne
        /// </summary>
        public void PlayCampaign()
        {
            PlaySwoosh();
            exitMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            playMenu.SetActive(true);
        }

        /// <summary>
        /// Retourne au menu principal
        /// </summary>
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

        /// <summary>
        /// Désactive le menu de jeu
        /// </summary>
        public void DisablePlayCampaign()
        {
            playMenu.SetActive(false);
        }

        /// <summary>
        /// Affiche le menu de confirmation de sortie
        /// </summary>
        public void AreYouSure()
        {
            PlaySwoosh();
            exitMenu.SetActive(true);
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            DisablePlayCampaign();
        }

        /// <summary>
        /// Ouvre le menu des crédits
        /// </summary>
        public void CreditsMenu()
        {
            PlaySwoosh();
            playMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(true);
            if (savesMenu) savesMenu.SetActive(false);
            exitMenu.SetActive(false);
        }

        /// <summary>
        /// Ouvre le menu de gestion des sauvegardes
        /// </summary>
        public void SavesMenus()
        {
            PlaySwoosh();
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu)
            {
                savesMenu.SetActive(true);
                LoadSavesInfo(); // Charge les infos à l'ouverture
            }
            exitMenu.SetActive(false);
        }

        // ============ POSITION CAMERA ============

        /// <summary>
        /// Déplace la caméra en position 2 (pour les sous-menus)
        /// </summary>
        public void Position2()
        {
            DisablePlayCampaign();
            CameraObject.SetFloat("Animate", 1);
        }

        /// <summary>
        /// Déplace la caméra en position 1 (menu principal)
        /// </summary>
        public void Position1()
        {
            CameraObject.SetFloat("Animate", 0);
        }

        // ============ SETTINGS PANELS ============

        /// <summary>
        /// Désactive tous les panels de paramètres
        /// </summary>
        void DisablePanels()
        {
            PanelSkin.SetActive(false);
            PanelGame.SetActive(false);
            lineGame.SetActive(false);
            lineSkin.SetActive(false);
        }

        /// <summary>
        /// Active le panel des paramètres jeu
        /// </summary>
        public void GamePanel()
        {
            DisablePanels();
            PanelGame.SetActive(true);
            lineGame.SetActive(true);
        }

        /// <summary>
        /// Active le panel des skins
        /// </summary>
        public void SkinPanel()
        {
            DisablePanels();
            PanelSkin.SetActive(true);
            lineSkin.SetActive(true);
        }

        // ============ FONCTIONS SCENE ============

        /// <summary>
        /// Charge une scène de manière asynchrone
        /// </summary>
        /// <param name="sceneName">Nom de la scène à charger</param>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        // ============ SONS FX ============

        /// <summary>
        /// Joue le son de survol d'élément UI
        /// </summary>
        public void PlayHover()
        {
            hoverSound.Play();
        }

        /// <summary>
        /// Joue le son de déplacement de slider
        /// </summary>
        public void PlaySFXHover()
        {
            sliderSound.Play();
        }

        /// <summary>
        /// Joue le son de transition (swoosh)
        /// </summary>
        public void PlaySwoosh()
        {
            swooshSound.Play();
        }

        // ============ QUITTER LE JEU ============

        /// <summary>
        /// Quitte l'application
        /// </summary>
        public void QuitGame()
        {
            SceneNavigator.ExitApp();
        }
    }
}