using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class OptionsController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    //[SerializeField] private TMP_Dropdown fullscreenDropdown;
    [SerializeField] private Slider volumeSlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    private const string VolumeKey = "MasterVolume";

    private Resolution[] resolutions;

    private void Start()
    {
        SetupVolume();
        SetupResolutions();
    }

    private void SetupVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        float dB = value <= 0.001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        SetResolution(savedIndex);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < resolutions.Length)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
            PlayerPrefs.SetInt("ResolutionIndex", index);
        }
    }

    /*private void SetupFullscreenDropdown()
    {
        fullscreenDropdown.ClearOptions();
        fullscreenDropdown.AddOptions(new List<string> { "Fullscreen", "Borderless", "Windowed" });

        FullScreenMode currentMode = Screen.fullScreenMode;
        int currentModeIndex = ModeToIndex(currentMode);

        int savedIndex = PlayerPrefs.GetInt("FullscreenMode", currentModeIndex);
        fullscreenDropdown.value = savedIndex;
        fullscreenDropdown.RefreshShownValue();

        SetFullscreenMode(savedIndex);
        fullscreenDropdown.onValueChanged.AddListener(SetFullscreenMode);
    }

    public void SetFullscreenMode(int index)
    {
        FullScreenMode mode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.Windowed
        };

        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt("FullscreenMode", index);

        SetResolution(resolutionDropdown.value);
    }

    private int ModeToIndex(FullScreenMode mode)
    {
        return mode switch
        {
            FullScreenMode.ExclusiveFullScreen => 0,
            FullScreenMode.FullScreenWindow => 1,
            FullScreenMode.Windowed => 2,
            _ => 2
        };
    }
    nie dziala :c
    */
}
