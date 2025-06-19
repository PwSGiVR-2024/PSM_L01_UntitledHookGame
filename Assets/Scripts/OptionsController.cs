using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown displayModeDropdown;

    private Resolution[] resolutions;

    private const string VolumeKey = "MasterVolume";
    private const string ResolutionKey = "ResolutionIndex";
    private const string DisplayModeKey = "DisplayMode"; // 0 = Fullscreen, 1 = Borderless

    void Start()
    {
        resolutions = Screen.resolutions
            .Where(r => Mathf.Approximately((float)r.width / r.height, 16f / 9f))
            .Distinct()
            .ToArray();

        resolutionDropdown.ClearOptions();
        List<string> options = resolutions.Select(r => $"{r.width} x {r.height}").ToList();
        resolutionDropdown.AddOptions(options);

        int savedResolution = PlayerPrefs.GetInt(ResolutionKey, resolutions.Length - 1);
        resolutionDropdown.value = Mathf.Clamp(savedResolution, 0, resolutions.Length - 1);
        resolutionDropdown.RefreshShownValue();
        SetResolution(resolutionDropdown.value);

        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string> { "Fullscreen", "Borderless" });
        int savedDisplayMode = PlayerPrefs.GetInt(DisplayModeKey, 0);
        displayModeDropdown.value = Mathf.Clamp(savedDisplayMode, 0, 1);
        displayModeDropdown.RefreshShownValue();
        SetDisplayMode(savedDisplayMode);

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
    }

    public void SetVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;

        Resolution res = resolutions[index];
        FullScreenMode currentMode = Screen.fullScreenMode;
        Screen.SetResolution(res.width, res.height, currentMode);
        PlayerPrefs.SetInt(ResolutionKey, index);
    }

    public void SetDisplayMode(int index)
    {
        FullScreenMode mode = index == 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.MaximizedWindow;
        Resolution res = resolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, mode);
        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt(DisplayModeKey, index);
    }
}
