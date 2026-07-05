using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text masterValueLabel;
    [SerializeField] private TMP_Text musicValueLabel;
    [SerializeField] private TMP_Text sfxValueLabel;
    [SerializeField] private Button backButton;
    [SerializeField] private AudioClip previewSfxClip;

    private bool suppressEvents;

    public event Action Closed;

    private void OnEnable()
    {
        suppressEvents = true;
        AudioVolumeSettings.Load();

        SetSlider(masterSlider, AudioVolumeSettings.MasterVolume);
        SetSlider(musicSlider, AudioVolumeSettings.MusicVolume);
        SetSlider(sfxSlider, AudioVolumeSettings.SfxVolume);

        RefreshLabels();
        suppressEvents = false;
    }

    private void Start()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(OnMasterChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    public void Close()
    {
        Closed?.Invoke();
    }

    private void OnMasterChanged(float value)
    {
        if (suppressEvents)
            return;

        AudioVolumeSettings.SetMasterVolume(value);
        RefreshLabels();
    }

    private void OnMusicChanged(float value)
    {
        if (suppressEvents)
            return;

        AudioVolumeSettings.SetMusicVolume(value);
        RefreshLabels();
    }

    private void OnSfxChanged(float value)
    {
        if (suppressEvents)
            return;

        AudioVolumeSettings.SetSfxVolume(value);
        RefreshLabels();
        PlayPreviewSfx();
    }

    private void PlayPreviewSfx()
    {
        if (previewSfxClip == null)
            return;

        AudioManager.Instance?.PlaySfx(previewSfxClip);
    }

    private static void SetSlider(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(value);
    }

    private void RefreshLabels()
    {
        SetLabel(masterValueLabel, AudioVolumeSettings.MasterVolume);
        SetLabel(musicValueLabel, AudioVolumeSettings.MusicVolume);
        SetLabel(sfxValueLabel, AudioVolumeSettings.SfxVolume);
    }

    private static void SetLabel(TMP_Text label, float value)
    {
        if (label == null)
            return;

        label.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
