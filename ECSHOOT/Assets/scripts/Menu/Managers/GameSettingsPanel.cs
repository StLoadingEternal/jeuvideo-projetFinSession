using UnityEngine;
using UnityEngine.UI;

public class GameSettingsPanel : MonoBehaviour
{
    [Header("UI Handles")]
    public Slider musicVolumeSlider;
    public Toggle fxToggle;
    public Toggle fullscreenToggle;

    void Start()
    {
        InitializeHandles();
        AddListeners();
    }

    // Initialise les handles avec les valeurs actuelles
    void InitializeHandles()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = GameSettings.MusicVolume;

        if (fxToggle != null)
            fxToggle.isOn = GameSettings.FXEnabled;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = GameSettings.Fullscreen;
    }

    // Ajoute les listeners pour appeler Update quand l’utilisateur change le handle
    void AddListeners()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);

        if (fxToggle != null)
            fxToggle.onValueChanged.AddListener(UpdateFXEnabled);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(UpdateFullscreen);
    }

    // ===== Fonctions d’update =====
    void UpdateMusicVolume(float value)
    {
        GameSettings.MusicVolume = value;
        AudioListener.volume = value; // applique immédiatement
    }

    void UpdateFXEnabled(bool value)
    {
        GameSettings.FXEnabled = value;
        // ici tu peux activer/désactiver les effets sonores
    }

    void UpdateFullscreen(bool value)
    {
        GameSettings.Fullscreen = value;
        Screen.fullScreen = value;
    }
}
