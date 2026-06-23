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
