using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SfxPlayer : MonoBehaviour
{
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;
    [SerializeField] private float baseVolume = 1f;

    private float masterMultiplier = 1f;
    private float sfxMultiplier = 1f;

    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f;
    }

    public void SetVolumeMultipliers(float master, float sfx)
    {
        masterMultiplier = Mathf.Clamp01(master);
        sfxMultiplier = Mathf.Clamp01(sfx);
    }

    public float EffectiveVolume => baseVolume * masterMultiplier * sfxMultiplier;

    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null || _source == null)
            return;

        var pitch = Random.Range(pitchMin, pitchMax);
        _source.pitch = pitch;
        _source.PlayOneShot(clip, EffectiveVolume);
        _source.pitch = 1f;
    }
}
