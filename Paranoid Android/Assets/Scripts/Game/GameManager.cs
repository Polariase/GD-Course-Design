using Cinemachine;
using UnityEngine;

public enum GameState
{
    Entry,
    Shelter,
    Exploration
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState currentGameState = GameState.Entry;
    [SerializeField] private PlayerStateData globalPlayerStateData;
    [SerializeField] private GameObject playerPrefab;

    private InventoryData _globalStorageData;
    public InventoryData GlobalStorageData => _globalStorageData;
    private int _defaultStorageCapacity = 60;

    public int globalDataCount = 0;
    public bool isAnalysisComplete = false;
    public bool reward0Claimed = false;
    public bool reward200Claimed = false;
    public bool reward500Claimed = false;
    public bool reward1000Claimed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (_globalStorageData == null)
        {
            _globalStorageData = new InventoryData(_defaultStorageCapacity);
        }
    }

    private void Start()
    {
        GameSceneManager.Instance.OnSceneLoadCompleted += HandleSceneChanged;
    }

    private void OnDestroy()
    {
        if (GameSceneManager.Instance != null)
            GameSceneManager.Instance.OnSceneLoadCompleted -= HandleSceneChanged;
    }

    public void StartGame()
    {
        GameSceneManager.Instance.LoadScene("ShelterScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private void HandleSceneChanged(string sceneName)
    {
        if (sceneName == "EntryScene")
        {
            currentGameState = GameState.Entry;
            UIManager.Instance.SetUIMode(GameState.Entry);
        }
        else if (sceneName == "ShelterScene")
        {
            currentGameState = GameState.Shelter;
            globalPlayerStateData.ClearAllSubscribers();
            globalPlayerStateData.ResetStatus(true);
            SpawnPlayer();
            UIManager.Instance.SetUIMode(GameState.Shelter);
        }
        else if(sceneName == "ExplorationScene")
        {
            currentGameState = GameState.Exploration;
            globalPlayerStateData.ClearAllSubscribers();
            globalPlayerStateData.ResetStatus(true);
            SpawnPlayer();
            UIManager.Instance.SetUIMode(GameState.Exploration);
        }
        else if (sceneName == "TestScene")
        {
            currentGameState = GameState.Exploration;
            globalPlayerStateData.ClearAllSubscribers();
            globalPlayerStateData.ResetStatus(true);
            SpawnPlayer();
            UIManager.Instance.SetUIMode(GameState.Exploration);
        }
    }

    private void SpawnPlayer()
    {
        if (PlayerController.Instance != null) return;

        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.transform.rotation : Quaternion.identity;
        GameObject playerGO = Instantiate(playerPrefab, spawnPos, spawnRot);
        PlayerController newPlayer = playerGO.GetComponent<PlayerController>();
        if (GameObject.FindWithTag("TopdownCam")?.GetComponent<CinemachineVirtualCamera>() is CinemachineVirtualCamera topDownCam)
        {
            newPlayer.Initialize(globalPlayerStateData, topDownCam);
        }
        UIManager.Instance.Initialize(newPlayer);
    }
}