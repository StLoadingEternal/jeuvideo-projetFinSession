using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameState
{
    public int score;
    public int lives;
    public int currentWave;
    public string saveDate;
    public int saveSlot;

    public GameState(int score, int lives, int currentWave, int slot = 1)
    {
        this.score = score;
        this.lives = lives;
        this.currentWave = currentWave;
        this.saveSlot = slot;
        this.saveDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}

public class SaveSystem 
{
    private static readonly string saveFolder = Application.persistentDataPath;
    private static string lastSaveSlotKey = "LastSaveSlot";

    public static void SaveGame(GameState state)
    {
        try
        {
            string savePath = Path.Combine(saveFolder, $"save_slot_{state.saveSlot}.json");
            string json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(savePath, json);

            PlayerPrefs.SetInt(lastSaveSlotKey, state.saveSlot);
            PlayerPrefs.Save();

            Debug.Log($"Jeu sauvegardé dans le slot {state.saveSlot}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur sauvegarde : " + e.Message);
        }
    }

    public static GameState LoadFromSlot(int slot)
    {
        string savePath = Path.Combine(saveFolder, $"save_slot_{slot}.json");

        if (!File.Exists(savePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            GameState state = JsonConvert.DeserializeObject<GameState>(json);
            return state;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur chargement : " + e.Message);
            return null;
        }
    }

    public static GameState LoadLastSave()
    {
        int lastSlot = PlayerPrefs.GetInt(lastSaveSlotKey, 0);
        if (lastSlot > 0)
        {
            return LoadFromSlot(lastSlot);
        }
        return null;
    }

    public static bool HasSaveInSlot(int slot)
    {
        string savePath = Path.Combine(saveFolder, $"save_slot_{slot}.json");
        return File.Exists(savePath);
    }

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

    public static void DeleteSave(int slot)
    {
        string savePath = Path.Combine(saveFolder, $"save_slot_{slot}.json");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"Sauvegarde slot {slot} supprimée");
        }
    }

    public static void DeleteAllSaves()
    {
        for (int i = 1; i <= 3; i++)
        {
            DeleteSave(i);
        }
    }

    public static int GetLastSaveSlot()
    {
        return PlayerPrefs.GetInt(lastSaveSlotKey, 0);
    }

    public static int GetBestSlotForNewGame()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (!HasSaveInSlot(i))
            {
                return i;
            }
        }

        return FindOldestSaveSlot();
    }

    private static int FindOldestSaveSlot()
    {
        DateTime oldestDate = DateTime.MaxValue;
        int oldestSlot = 1;

        for (int i = 1; i <= 3; i++)
        {
            GameState save = LoadFromSlot(i);
            if (save != null && !string.IsNullOrEmpty(save.saveDate))
            {
                if (DateTime.TryParse(save.saveDate, out DateTime saveDate))
                {
                    if (saveDate < oldestDate)
                    {
                        oldestDate = saveDate;
                        oldestSlot = i;
                    }
                }
            }
        }

        return oldestSlot;
    }
}