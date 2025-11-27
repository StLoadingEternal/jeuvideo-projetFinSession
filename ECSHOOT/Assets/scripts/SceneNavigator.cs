using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    
    void Start()
    {
        //Demarre le jeu sur la scène menu
        SceneManager.LoadScene("Menu");
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Aller à la scène menu
    public static void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    //Aller à la scène game
    public static void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    //Sortir du jeu
    public static void ExitApp()
    {
        Application.Quit();
    }
}
