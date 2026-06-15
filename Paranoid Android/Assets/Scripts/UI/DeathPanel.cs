using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathPanel : BasePanel
{
    public Button _actionButton;
    public string _shelterSceneName = "ShelterScene";

    protected override void Awake()
    {
        base.Awake();
        if (_actionButton != null)
        {
            _actionButton.onClick.AddListener(OnReturnClicked);
        }
    }

    public override void Open()
    {
        base.Open();
        if (_actionButton != null) _actionButton.interactable = true;
    }

    private void OnReturnClicked()
    {
        if (_actionButton != null) _actionButton.interactable = false;
        GameSceneManager.Instance.LoadScene(_shelterSceneName);
        if (_actionButton != null) _actionButton.interactable = true;
    }
}