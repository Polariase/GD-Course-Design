using UnityEngine;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    public Button quitButton;
    public Button continueButton;

    protected override void Awake()
    {
        base.Awake();
        quitButton.onClick.AddListener(OnQuitClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnQuitClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Back();
        }

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene("EntryScene");
        }
    }

    private void OnContinueClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Back();
        }
    }

    private void OnDestroy()
    {
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}