using UnityEngine;

public class SceneBGMTrigger : MonoBehaviour
{
    public AudioClip sceneBGM;

    private void Start()
    {
        if (sceneBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(sceneBGM);
        }
    }
}