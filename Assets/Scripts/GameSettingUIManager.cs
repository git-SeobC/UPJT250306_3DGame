using UnityEngine;
using UnityEngine.UI;

public class GameSettingUIManager : MonoBehaviour
{
    public GameObject SettingObj;
    public GameObject TitleUI;

    public Text resolutionText;
    public Text graphicsQualityText;
    public Text fullScreenText;

    private bool isSettings = false;

    private int resolutionIndex = 0;
    private int qualityIndex = 0;
    private bool isFullScreen = true;

    private string[] resolutions = { "1280 x 720", "1920 x 1080", "2560 x 1440", "3840 x 2160" };
    private string[] qualityOptions = { "Low", "Normal", "High" };

    public float bgmVolume = 1.0f;


    public void OnResolutionLeftClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionText();
    }

    public void OnResoultionRightClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionText();
    }

    public void OnGrphicsLeftClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
        UpdateGraphicsQulityText();
        bgmVolume -= 0.1f;
        bgmVolume = Mathf.Clamp(bgmVolume, 0, 1);
        SoundManager.Instance.SetBGMVolume(bgmVolume);
    }

    public void OnGraphicsRightClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
        UpdateGraphicsQulityText();
        bgmVolume += 0.1f;
        bgmVolume = Mathf.Clamp(bgmVolume, 0, 1);
        SoundManager.Instance.SetBGMVolume(bgmVolume);
    }

    public void OnFullScreenToggleClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        isFullScreen = !isFullScreen;
        UpdateFullScreenText();
    }

    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }

    private void UpdateGraphicsQulityText()
    {
        graphicsQualityText.text = qualityOptions[qualityIndex];
    }

    private void UpdateFullScreenText()
    {
        fullScreenText.text = isFullScreen ? "On" : "Off";
    }

    public void OnApplySettingsClick()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        ApplySettings();
        SaveSettings();
    }

    private void ApplySettings()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        string[] res = resolutions[resolutionIndex].Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", qualityIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);
        isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1 ? true : false;
    }

    public void OnSettings()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        LoadSettings();
        UpdateResolutionText();
        UpdateGraphicsQulityText();
        UpdateFullScreenText();
        SettingObj.SetActive(true);
    }

    public void OnSettingsBack()
    {
        SoundManager.Instance.PlaySfx("OnSettings", transform.position);
        SettingObj.SetActive(false);
        TitleUI.SetActive(true);
    }
}
