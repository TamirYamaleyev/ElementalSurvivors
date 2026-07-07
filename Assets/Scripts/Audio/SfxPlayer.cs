using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SfxPlayer : MonoBehaviour
{
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;
    [SerializeField] private float volume = 1f;

    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f;
    }

    public void PlayOneShot(AudioClip clip, float overrideVolume = 1f)
    {
        volume = overrideVolume;

        if (clip == null || _source == null)
            return;

        var pitch = Random.Range(pitchMin, pitchMax);
        _source.pitch = pitch;
        _source.PlayOneShot(clip, volume);
        _source.pitch = 1f;
    }
}
