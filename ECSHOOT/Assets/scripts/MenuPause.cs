using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuPause : MonoBehaviour
{

    private GameObject menuPausePanelInstance;
    
    private bool isPaused = false;


    private void Start()
    {
        menuPausePanelInstance = gameObject;
    }
    void Update()
    {
         //Mettre ailleurs
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        menuPausePanelInstance.SetActive(false);
        Time.timeScale = 1f;   // reprendre le temps
        isPaused = false;
    }

    void PauseGame()
    {
        menuPausePanelInstance.SetActive(true);
        Time.timeScale = 0f;   // arrêter le temps
        isPaused = true;
        EventSystem.current.IsPointerOverGameObject();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // éviter un bug si on retourne au menu
        SceneManager.LoadScene("Menu_Scene");
    }

    public void SaveGame()
    {
        Debug.Log("Sauvegarde effectuée (placeholder)");
        // Ici tu mets ton vrai système de sauvegarde plus tard
    }
}
