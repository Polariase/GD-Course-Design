using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        if (GameSystem.Instance == null)
        {
            var initHandle = Addressables.InitializeAsync();
            initHandle.WaitForCompletion();

            var loadHandle = Addressables.InstantiateAsync("GameSystem");
            GameObject gameSystemGO = loadHandle.WaitForCompletion();

            if (gameSystemGO != null)
            {
                Object.DontDestroyOnLoad(gameSystemGO);
            }
            else
            {
                Debug.LogError("º”‘ÿ ß∞‹");
            }
        }
    }
}