using UnityEngine;

public static class GameSettings
{
    // Clés PlayerPrefs
    private const string KEY_MUSIC_VOLUME = "MusicVolume";
    private const string KEY_FX_ENABLED = "FXEnabled";
    private const string KEY_FULLSCREEN = "FullscreenEnabled";

    // Membres statiques
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
        set => PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, Mathf.Clamp01(value));
    }

    public static bool FXEnabled
    {
        get => PlayerPrefs.GetInt(KEY_FX_ENABLED, 1) == 1;
        set => PlayerPrefs.SetInt(KEY_FX_ENABLED, value ? 1 : 0);
    }

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
        set => PlayerPrefs.SetInt(KEY_FULLSCREEN, value ? 1 : 0);
    }
}
