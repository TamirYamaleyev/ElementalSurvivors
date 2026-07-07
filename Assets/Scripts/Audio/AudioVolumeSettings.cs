using UnityEngine;

public static class AudioVolumeSettings
{
    const string MasterKey = "audio.master";
    const string MusicKey = "audio.music";
    const string SfxKey = "audio.sfx";

    public const float DefaultMaster = 1f;
    public const float DefaultMusic = 0.6f;
    public const float DefaultSfx = 1f;

    public static float MasterVolume { get; private set; } = DefaultMaster;
    public static float MusicVolume { get; private set; } = DefaultMusic;
    public static float SfxVolume { get; private set; } = DefaultSfx;

    public static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterKey, DefaultMaster);
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultMusic);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, DefaultSfx);
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        PlayerPrefs.Save();
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        Save();
        ApplyIfAvailable();
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        Save();
        ApplyIfAvailable();
    }

    public static void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        Save();
        ApplyIfAvailable();
    }

    public static void Apply(AudioManager manager)
    {
        if (manager == null)
            return;

        //manager.ApplyVolumeSettings(MasterVolume, MusicVolume, SfxVolume);
    }

    static void ApplyIfAvailable()
    {
        if (AudioManager.Instance != null)
            Apply(AudioManager.Instance);
    }
}
