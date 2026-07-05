using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BgmPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip defaultClip;
    [SerializeField] private float baseVolume = 0.6f;

    private float masterMultiplier = 1f;
    private float musicMultiplier = 1f;

    private AudioSource _source;

    public AudioClip CurrentClip => _source != null ? _source.clip : null;

    public float EffectiveVolume => baseVolume * masterMultiplier * musicMultiplier;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.spatialBlend = 0f;
        RefreshSourceVolume();
    }

    public void SetVolumeMultipliers(float master, float music)
    {
        masterMultiplier = Mathf.Clamp01(master);
        musicMultiplier = Mathf.Clamp01(music);
        RefreshSourceVolume();
    }

    public void PlayDefault()
    {
        if (defaultClip != null)
            Play(defaultClip);
    }

    public void Play(AudioClip clip)
    {
        if (clip == null || _source == null)
            return;

        _source.clip = clip;
        RefreshSourceVolume();
        _source.Play();
    }

    public void Stop()
    {
        if (_source == null)
            return;

        _source.Stop();
    }

    void RefreshSourceVolume()
    {
        if (_source != null)
            _source.volume = EffectiveVolume;
    }
}
