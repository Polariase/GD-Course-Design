using UnityEngine;
using UnityEngine.UI;

public class EntrySceneController : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;

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

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(OnStartButtonClicked);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }
}