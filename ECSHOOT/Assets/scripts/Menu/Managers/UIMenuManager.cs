using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu{
	public class UIMenuManager : MonoBehaviour {
		private Animator CameraObject;

		// campaign button sub menu
        [Header("MENUS")]
        [Tooltip("The Menu for when the MAIN menu buttons")]
        public GameObject mainMenu;
        [Tooltip("THe first list of buttons")]
        public GameObject firstMenu;
        [Tooltip("The Menu for when the PLAY button is clicked")]
        public GameObject playMenu;
        [Tooltip("The Menu for when the EXIT button is clicked")]
        public GameObject exitMenu;
        [Tooltip("Optional 4th Menu")]
        public GameObject creditsMenu;
		[Tooltip("The Menu for when the LOAD button is clicked")]
		public GameObject savesMenu;

        // SAUVEGARDE - Nouveaux éléments
        [Header("SAUVEGARDE")]
        public Button continueButton;
        public TextMeshProUGUI saveInfoText;
        public GameObject newGameConfirmationPanel;

        [Header("LOAD SAVES")]
        public Button save1;
        public Button save2;
        public Button save3;

        public enum Theme {custom1, custom2, custom3};
        [Header("THEME SETTINGS")]
        public Theme theme;
        public ThemedUIData themeController;

        [Header("PANELS")]
        [Tooltip("The UI Panel parenting all sub menus")]
        public GameObject mainCanvas;
        [Tooltip("The UI Panel that holds the VIDEO window tab")]
        public GameObject PanelSkin;
        [Tooltip("The UI Panel that holds the GAME window tab")]
        public GameObject PanelGame;
  
        // highlights in settings screen
        [Header("SETTINGS SCREEN")]
        [Tooltip("Highlight Image for when GAME Tab is selected in Settings")]
        public GameObject lineGame;
        [Tooltip("Highlight Image for when VIDEO Tab is selected in Settings")]
        public GameObject lineSkin;
     

		[Header("SFX")]
        [Tooltip("The GameObject holding the Audio Source component for the HOVER SOUND")]
        public AudioSource hoverSound;
        [Tooltip("The GameObject holding the Audio Source component for the AUDIO SLIDER")]
        public AudioSource sliderSound;
        [Tooltip("The GameObject holding the Audio Source component for the SWOOSH SOUND when switching to the Settings Screen")]
        public AudioSource swooshSound;

		void Start(){
			CameraObject = transform.GetComponent<Animator>();

			
			playMenu.SetActive(false);
			exitMenu.SetActive(false);
			if(creditsMenu) creditsMenu.SetActive(false);
			if(newGameConfirmationPanel) newGameConfirmationPanel.SetActive(false);
			firstMenu.SetActive(true);
			mainMenu.SetActive(true);

			CheckSaveFile(); // Vérifier la sauvegarde au démarrage
		}

		// ============ SYSTÈME DE SAUVEGARDE ============

		private void CheckSaveFile()
		{
			bool hasSave = SaveSystem.CheckHasSave();
			
			if (continueButton != null)
			{
				continueButton.interactable = hasSave;
				
				if (hasSave)
				{
					// Charger les infos de sauvegarde
					GameState saveData = SaveSystem.LoadStateFromSave();
					if (saveData != null)
					{
						TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
						if (buttonText != null)
						{
							buttonText.text = $"CONTINUER\nVague {saveData.currentWave} - Score: {saveData.score}";
						}
						
						// Mettre à jour le texte d'info si besoin
						if (saveInfoText != null)
						{
							saveInfoText.text = $"Partie sauvegardée:\nVague {saveData.currentWave} - Score: {saveData.score} - Vies: {saveData.lives}";
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

		public void ContinueGame()
		{
			if (SaveSystem.CheckHasSave())
			{
				PlaySwoosh();
				LoadScene("mainScene"); // Remplacez par le nom de votre scène de jeu
			}
			else
			{
				Debug.LogWarning("Aucune sauvegarde trouvée !");
				PlayHover(); // Jouer un son d'erreur si vous en avez un
			}
		}

		public void NewGame()
		{
			PlaySwoosh();
			
			// Vérifier si une sauvegarde existe
			if (SaveSystem.CheckHasSave() && newGameConfirmationPanel != null)
			{
				// Afficher le panel de confirmation
				newGameConfirmationPanel.SetActive(true);
			}
			else
			{
				// Démarrer directement une nouvelle partie
				StartNewGame();
			}
		}

		public void ConfirmNewGame()
		{
			StartNewGame();
			
			if (newGameConfirmationPanel != null)
			{
				newGameConfirmationPanel.SetActive(false);
			}
		}

		public void CancelNewGame()
		{
			PlayHover();
			
			if (newGameConfirmationPanel != null)
			{
				newGameConfirmationPanel.SetActive(false);
			}
		}

		private void StartNewGame()
		{
			// Supprimer l'ancienne sauvegarde
			SaveSystem.DeleteSave();
			
			// Charger la scène de jeu
			LoadScene("mainScene"); // Remplacez par le nom de votre scène de jeu
		}

		// ============ FONCTIONS ORIGINALES (conservées) ============

		public void LoadScene(string sceneName)
		{
			SceneManager.LoadSceneAsync(sceneName);
			
			
			// if(waitForInput){
   //              StartCoroutine(LoadAsynchronously(sceneName));
   //          }
		}
		
		// gestion fenetre de chargement
		// IEnumerator LoadAsynchronously(string sceneName)
		// {
		// 	// loadingBar.value = 0;
		// 	loadingMenu.SetActive(true);
		//
		// 	AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
		// 	operation.allowSceneActivation = false;
		//
		// 	while (!operation.isDone)
		// 	{
		// 		float progress = Mathf.Clamp01(operation.progress / 0.9f);
		// 		// loadingBar.value = progress;
		//
		// 		if (operation.progress >= 0.9f)
		// 		{
		// 			loadPromptText.text = "Appuyez sur " + userPromptKey + " pour continuer";
		//
		// 			if (Input.GetKeyDown(userPromptKey))
		// 			{
		// 				operation.allowSceneActivation = true;
		// 			}
		// 		}
		//
		// 		yield return null;
		// 	}
		// }
		
		

		//Naviagations Main Menus
		public void PlayCampaign(){
			PlaySwoosh();
			exitMenu.SetActive(false);
			if(creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            playMenu.SetActive(true);
		}
		
		public void ReturnMenu(){
			PlaySwoosh();
			playMenu.SetActive(false);
			if(creditsMenu) creditsMenu.SetActive(false);
			exitMenu.SetActive(false);
			if(newGameConfirmationPanel) newGameConfirmationPanel.SetActive(false);
			if (savesMenu) savesMenu.SetActive(false);
			mainMenu.SetActive(true);
		}

		public void  DisablePlayCampaign(){
			playMenu.SetActive(false);
		}

        // Are You Sure - Quit Panel Pop Up
        public void AreYouSure()
        {
            PlaySwoosh();
            exitMenu.SetActive(true);
            if(creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(false);
            DisablePlayCampaign();
        }

        //Ouvrir le menu des crédits
        public void CreditsMenu()
        {
            PlaySwoosh();
            playMenu.SetActive(false);
            if (creditsMenu) creditsMenu.SetActive(true);
            if (savesMenu) savesMenu.SetActive(false);
            exitMenu.SetActive(false);
        }

        //Ouvrir le menu des sauvegardes
        public void SavesMenus()
        {
            PlaySwoosh();
            if (creditsMenu) creditsMenu.SetActive(false);
            if (savesMenu) savesMenu.SetActive(true);
            exitMenu.SetActive(false);
        }

        //Position de la caméra (Pour l'animation du menu)
        public void Position2(){
			DisablePlayCampaign();
			CameraObject.SetFloat("Animate",1);
		}

		public void Position1(){
			CameraObject.SetFloat("Animate",0);
		}


        //Désactivation et Navigation dans le Menu Settings
        void DisablePanels(){
			PanelSkin.SetActive(false);
			PanelGame.SetActive(false);
			lineGame.SetActive(false);
			lineSkin.SetActive(false);
		}

		public void GamePanel(){
			DisablePanels();
			PanelGame.SetActive(true);
			lineGame.SetActive(true);
		}

		public void SkinPanel(){
			DisablePanels();
			PanelSkin.SetActive(true);
			lineSkin.SetActive(true);
		}

		//Sons Fx pour le menu

		public void PlayHover(){
			hoverSound.Play();
		}

		public void PlaySFXHover(){
			sliderSound.Play();
		}

		public void PlaySwoosh(){
			swooshSound.Play();
		}


		//Quiter le jeu
		public void QuitGame(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}
	}
}