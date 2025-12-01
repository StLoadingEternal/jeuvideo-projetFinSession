using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuPause : MonoBehaviour
{
    public GameManager gameManagerScript;

    void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void ResumeGame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        Debug.Log("Pause activée");
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void QuitGame()
    {
        // Sauvegarder avant de quitter
        if (gameManagerScript != null)
        {
            gameManagerScript.SaveGame();
            SceneManager.LoadScene("Menu_Scene");
        }
        
        Time.timeScale = 1f;
        //SceneNavigator.GoToMenu();
    }

    public void SaveGame()
    {
        if (gameManagerScript != null)
        {
            gameManagerScript.SaveGame();
            Debug.Log("Sauvegarde effectuée !");
        }
    }

    public void LoadGame()
    {
        if (gameManagerScript != null)
        {
            gameManagerScript.LoadGame();
            Debug.Log("Chargement effectué !");
        }
    }

    public void NewGame()
    {
        // Supprimer la sauvegarde existante
        if (gameManagerScript != null)
        {
            gameManagerScript.DeleteSave();
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}