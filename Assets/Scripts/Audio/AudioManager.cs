using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip uiClickSfx;

    [SerializeField] private SfxPlayer sfxPlayer;
    [SerializeField] private BgmPlayer bgmPlayer;
    [SerializeField] private AudioSource sfxAudioSrc;
    [SerializeField] private AudioSource bgmAudioSrc;

    [SerializeField] private AudioClip defaultBgm;
    [SerializeField] private AudioClip[] bgmTracks = System.Array.Empty<AudioClip>();

    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    public void PlayUIClick()
    {
        Instance.PlaySfx(uiClickSfx, 1.1f);
    }

    public void PauseSFX()
    {
        sfxAudioSrc.Pause();
    }

    public void UnPauseSFX()
    {
        sfxAudioSrc.UnPause();
    }

    public void PauseBGM()
    {
        bgmAudioSrc.Pause();
    }

    public void UnPauseBGM()
    {
        bgmAudioSrc.UnPause();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (sfxPlayer == null)
            return;

        sfxAudioSrc.pitch = Random.Range(minPitch, maxPitch);
        sfxPlayer.PlayOneShot(clip, volume);
        sfxAudioSrc.pitch = 1f;
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
