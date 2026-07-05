using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private SfxPlayer sfxPlayer;
    [SerializeField] private BgmPlayer bgmPlayer;
    [SerializeField] private AudioClip defaultBgm;
    [SerializeField] private AudioClip[] bgmTracks = System.Array.Empty<AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        AudioVolumeSettings.Load();
        ApplyVolumeSettings(
            AudioVolumeSettings.MasterVolume,
            AudioVolumeSettings.MusicVolume,
            AudioVolumeSettings.SfxVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (defaultBgm != null)
            PlayBgm(defaultBgm);
        else
            bgmPlayer?.PlayDefault();
    }

    public void ApplyVolumeSettings(float master, float music, float sfx)
    {
        sfxPlayer?.SetVolumeMultipliers(master, sfx);
        bgmPlayer?.SetVolumeMultipliers(master, music);
    }

    public void SetMasterVolume(float value) => AudioVolumeSettings.SetMasterVolume(value);

    public void SetMusicVolume(float value) => AudioVolumeSettings.SetMusicVolume(value);

    public void SetSfxVolume(float value) => AudioVolumeSettings.SetSfxVolume(value);

    public void PlaySfx(AudioClip clip)
    {
        if (sfxPlayer == null)
            return;

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlayBgm(AudioClip clip)
    {
        if (bgmPlayer == null)
            return;

        bgmPlayer.Play(clip);
    }

    public void PlayBgm(int trackIndex)
    {
        if (bgmTracks == null || trackIndex < 0 || trackIndex >= bgmTracks.Length)
            return;

        PlayBgm(bgmTracks[trackIndex]);
    }

    public void StopBgm()
    {
        if (bgmPlayer == null)
            return;

        bgmPlayer.Stop();
    }

    public AudioClip CurrentBgm => bgmPlayer != null ? bgmPlayer.CurrentClip : null;
}
