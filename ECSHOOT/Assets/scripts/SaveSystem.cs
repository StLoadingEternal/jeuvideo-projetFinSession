using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class GameState
{
    public int score;
    public int lives;
    public int currentWave;
}

public class SaveSystem
{
    private static readonly string savePath = Path.Combine(Application.persistentDataPath, "save.json");

    // Méthode statique pour sauvegarder
    private static void SaveGame(GameState state)
    {
        try
        {
            string json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(savePath, json);
            Debug.Log("Jeu sauvegardé : " + savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur sauvegarde : " + e.Message);
        }
    }

    // Sauvegarde avec paramètres
    public void SaveGame(int score, int lives, int currentWave)
    {
        GameState state = new GameState()
        {
            score = score,
            lives = lives,
            currentWave = currentWave
        };

        SaveGame(state);
    }

    public static bool CheckHasSave()
    {
        return File.Exists(savePath);
    }

    public static GameState LoadStateFromSave()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Aucune sauvegarde trouvée !");
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            GameState state = JsonConvert.DeserializeObject<GameState>(json);
            Debug.Log("Sauvegarde chargée !");
            return state;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur chargement : " + e.Message);
            return null;
        }
    }
    
    // Supprimer la sauvegarde
    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Sauvegarde supprimée");
        }
    }
}