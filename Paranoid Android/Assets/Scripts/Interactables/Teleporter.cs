using UnityEngine;

public class Teleporter : MonoBehaviour, IInteractable
{
    public string targetSceneName;

    public void Interact()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadScene(targetSceneName);
        }
    }
}