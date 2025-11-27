using System.IO;
using UnityEngine;
using Newtonsoft.Json;

//Classe GameState enregistre l'état d'une partie 
[System.Serializable]
public class GameState
{
    public int score;
    public int lives;
    public float vague;
}


public class SaveSystem
{
    //Fichier de sauvegarde d'une partie
    private static readonly string savePath = Path.Combine(Application.persistentDataPath, "save.json");

    //Sauvegarde l'état du jeu par sérialisation
    public static void SaveGame(GameState state)
    {
        string json = JsonConvert.SerializeObject(state, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log($"Jeu sauvegardé dans : {savePath}");
    }

    //Check si une sauvegarde est existante
    public static bool CheckHasSave()
    {
        return File.Exists(savePath);
    }

    //Chargée une sauvegarde existante par déssérialisation
    public static GameState LoadStateFromSave()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Aucune sauvegarde trouvée !");
            return null;
        }

        string json = File.ReadAllText(savePath);
        GameState state = JsonConvert.DeserializeObject<GameState>(json);
        Debug.Log("Sauvegarde chargée !");
        return state;
    }
}
