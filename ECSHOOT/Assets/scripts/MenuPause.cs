using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuPause : MonoBehaviour
{
    public GameManager gameManagerScript;//Référence sur le script gameManager

   
    void Start()
    {
        gameManagerScript = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void ResumeGame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;   // reprendre le temps
    }

    public void PauseGame()
    {
        Debug.Log("Pause activée");
        gameObject.SetActive(true);
        Time.timeScale = 0f;   
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // éviter un bug si on retourne au menu
        //SceneNavigator.GoToMenu();
    }

    public void SaveGame()
    {
        Debug.Log("Sauvegarde effectuée (placeholder)");
        //gameManagerScript.saveGame();
        // sauvegarde plus tard
    }
}
