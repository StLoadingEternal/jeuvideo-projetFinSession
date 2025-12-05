using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public int score;
    public int lives;
    public int currentWave;
    public string saveName; // Nom personnalisé de la sauvegarde
    public string saveDate; // Date de la sauvegarde
    public int saveSlot; // Slot de sauvegarde (1, 2 ou 3)
    
    // Constructeur pour faciliter la création
    public GameState(int score, int lives, int currentWave, int slot = 1)
    {
        this.score = score;
        this.lives = lives;
        this.currentWave = currentWave;
        this.saveSlot = slot;
        this.saveDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        this.saveName = $"Sauvegarde {slot} - Vague {currentWave}";
    }
}

public class SaveSystem
{
    private static readonly string saveFolder = Application.persistentDataPath;
    private static string lastSaveSlotKey = "LastSaveSlot";

    // ============ GESTION MULTI-SLOTS ============
    
    // Sauvegarder dans un slot spécifique
    public static void SaveGame(GameState state)
    {
        try
        {
            string savePath = GetSavePath(state.saveSlot);
            string json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(savePath, json);
            
            // Mémoriser le dernier slot utilisé pour "Continuer"
            PlayerPrefs.SetInt(lastSaveSlotKey, state.saveSlot);
            PlayerPrefs.Save();
            
            Debug.Log($"Jeu sauvegardé dans le slot {state.saveSlot}: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur sauvegarde : " + e.Message);
        }
    }

    // Charger depuis un slot spécifique
    public static GameState LoadFromSlot(int slot)
    {
        string savePath = GetSavePath(slot);
        
        if (!File.Exists(savePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            GameState state = JsonConvert.DeserializeObject<GameState>(json);
            Debug.Log($"Sauvegarde chargée depuis le slot {slot} !");
            return state;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur chargement : " + e.Message);
            return null;
        }
    }

    // Charger la dernière sauvegarde (pour "Continuer")
    public static GameState LoadLastSave()
    {
        int lastSlot = PlayerPrefs.GetInt(lastSaveSlotKey, 0);
        if (lastSlot > 0)
        {
            return LoadFromSlot(lastSlot);
        }
        return null;
    }

    // Vérifier si un slot a une sauvegarde
    public static bool HasSaveInSlot(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    // Obtenir les infos de tous les slots
    public static List<GameState> GetAllSaveInfos()
    {
        List<GameState> saves = new List<GameState>();
        
        for (int i = 1; i <= 3; i++)
        {
            GameState save = LoadFromSlot(i);
            if (save != null)
            {
                saves.Add(save);
            }
        }
        
        return saves;
    }

    // Supprimer une sauvegarde spécifique
    public static void DeleteSave(int slot)
    {
        string savePath = GetSavePath(slot);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"Sauvegarde slot {slot} supprimée");
        }
    }

    // Obtenir le chemin du fichier de sauvegarde
    private static string GetSavePath(int slot)
    {
        return Path.Combine(saveFolder, $"save_slot_{slot}.json");
    }

    // Vérifier si une sauvegarde existe (n'importe quel slot)
    public static bool CheckHasSave()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (HasSaveInSlot(i))
            {
                return true;
            }
        }
        return false;
    }

    // Obtenir le slot de la dernière sauvegarde
    public static int GetLastSaveSlot()
    {
        return PlayerPrefs.GetInt(lastSaveSlotKey, 0);
    }
}