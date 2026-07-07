using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    public event Action<string> OnSceneLoadStarted;
    public event Action<string> OnSceneLoadCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        UIManager.Instance.ShowLoading(true);
        PoolManager.Instance.DeactiveAll();
        yield return new WaitForSeconds(0.2f);
        OnSceneLoadStarted?.Invoke(sceneName);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OnSceneLoadCompleted?.Invoke(sceneName);
        UIManager.Instance.ShowLoading(false);
    }
}