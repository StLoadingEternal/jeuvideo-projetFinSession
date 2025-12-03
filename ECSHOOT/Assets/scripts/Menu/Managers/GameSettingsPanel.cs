using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class GameSettingsPanel : MonoBehaviour
{
    [Header("UI Handles")]
    public Slider musicVolumeSlider;

    [Header("Buttons with text")]
    public Button fxButton;
    public TextMeshPro fxButtonText;

    public Button fullscreenButton;
    public TextMeshPro fullscreenButtonText;

    // ON / OFF colors
    private Color onColor = new Color(0.2f, 0.9f, 0.2f);   // vert
    private Color offColor = new Color(0.9f, 0.2f, 0.2f);  // rouge

    void Start()
    {
        InitializeHandles();
        AddListeners();
    }

    void InitializeHandles()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = GameSettings.MusicVolume;

        UpdateFXButtonDisplay();
        UpdateFullscreenButtonDisplay();
    }

    void AddListeners()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);

        if (fxButton != null)
            fxButton.onClick.AddListener(ToggleFX);

        if (fullscreenButton != null)
            fullscreenButton.onClick.AddListener(ToggleFullscreen);
    }

    // -------- UPDATE --------

    void UpdateMusicVolume(float value)
    {
        GameSettings.MusicVolume = value;
        AudioListener.volume = value;
    }

    void ToggleFX()
    {
        GameSettings.FXEnabled = !GameSettings.FXEnabled;
        UpdateFXButtonDisplay();
    }

    void ToggleFullscreen()
    {
        GameSettings.Fullscreen = !GameSettings.Fullscreen;
        Screen.fullScreen = GameSettings.Fullscreen;
        UpdateFullscreenButtonDisplay();
    }

    // -------- UI DISPLAY --------

    void UpdateFXButtonDisplay()
    {
        bool isOn = GameSettings.FXEnabled;
        fxButtonText.text = isOn ? "ON" : "OFF";
        fxButtonText.color = isOn ? onColor : offColor;
    }

    void UpdateFullscreenButtonDisplay()
    {
        bool isOn = GameSettings.Fullscreen;
        fullscreenButtonText.text = isOn ? "ON" : "OFF";
        fullscreenButtonText.color = isOn ? onColor : offColor;
    }
}
