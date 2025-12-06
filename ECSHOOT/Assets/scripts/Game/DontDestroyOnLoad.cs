using UnityEngine;


public class DontDestroyOnLOad : MonoBehaviour
{
    private static DontDestroyOnLOad instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //Preserver l'objet de jeu entre les scenes
        if (FindObjectsByType<DontDestroyOnLOad>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
