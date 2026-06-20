using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BgmPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip defaultClip;
    [SerializeField] private float volume = 0.6f;

    private AudioSource _source;

    public AudioClip CurrentClip => _source != null ? _source.clip : null;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.spatialBlend = 0f;
        _source.volume = volume;
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
        _source.volume = volume;
        _source.Play();
    }

    public void Stop()
    {
        if (_source == null)
            return;

        _source.Stop();
    }
}
