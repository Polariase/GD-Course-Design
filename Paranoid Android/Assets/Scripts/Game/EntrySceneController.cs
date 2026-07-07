using BehaviorDesigner.Runtime.Tasks.Unity.UnityInput;
using UnityEngine;
using UnityEngine.UI;

public class EntrySceneController : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public Button settingButton;
    public GameObject volumePanel;   
    public Slider bgmSlider;         
    public Slider sfxSlider;         


    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        if (settingButton != null && volumePanel != null)
        {
            settingButton.onClick.AddListener(OnSettingButtonClicked);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = 0.2f;
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
            bgmSlider.onValueChanged.Invoke(bgmSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = 1f;
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            sfxSlider.onValueChanged.Invoke(sfxSlider.value);
        }

        if (volumePanel != null)
        {
            volumePanel.SetActive(false);
        }
    }

    private void OnSettingButtonClicked()
    {
        if (volumePanel != null)
        {
            bool currentState = volumePanel.activeSelf;
            volumePanel.SetActive(!currentState);
        }
    }

    private void OnBgmSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGroupVolume("BGMVolume", value);
        }
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGroupVolume("SFXVolume", value);
        }
    }

    private void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnQuitButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameSceneManager.Instance.LoadScene("TestScene");
        }
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(OnStartButtonClicked);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        if (settingButton != null) settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
    }
}