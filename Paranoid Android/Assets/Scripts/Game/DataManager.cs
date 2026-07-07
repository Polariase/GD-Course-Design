using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    private readonly Dictionary<int, ItemData> _itemCache = new();
    private readonly Dictionary<string, Sprite> _iconCache = new();
    private readonly Dictionary<string, GameObject> _weaponModelCache = new();
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        LoadAllData();
    }

    private async void LoadAllData()
    {
        AsyncOperationHandle<IList<ItemData>> handle = Addressables.LoadAssetsAsync<ItemData>("ItemData", null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _itemCache.Clear();
            foreach (var data in handle.Result)
            {
                if (!_itemCache.ContainsKey(data.itemID))
                {
                    _itemCache.Add(data.itemID, data);

                    if (!string.IsNullOrEmpty(data.iconAddress) && !_iconCache.ContainsKey(data.iconAddress))
                    {
                        var iconHandle = Addressables.LoadAssetAsync<Sprite>(data.iconAddress);
                        await iconHandle.Task;

                        if (iconHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            _iconCache.Add(data.iconAddress, iconHandle.Result);
                        }
                    }

                    if (data is WeaponData weaponData)
                    {
                        string addressKey = weaponData.itemName + "Model";
                        if (!_weaponModelCache.ContainsKey(addressKey))
                        {
                            var modelHandle = Addressables.LoadAssetAsync<GameObject>(addressKey);
                            await modelHandle.Task;
                            if (modelHandle.Status == AsyncOperationStatus.Succeeded)
                            {
                                _weaponModelCache.Add(addressKey, modelHandle.Result);
                            }
                        }
                    }
                }
            }


            IsInitialized = true;
            Debug.Log($"[DataManager]成功加载 {_itemCache.Count} 个配置及其缓存图标。");
        }
        else
        {
            Debug.LogError("[DataManager] 道具配置加载失败！");
        }
    }

    public ItemData GetItemData(int id)
    {
        if (_itemCache.TryGetValue(id, out var data))
        {
            return data;
        }
        return null;
    }

    public Sprite GetIcon(string addr)
    {
        if(_iconCache.TryGetValue(addr, out var sprite))
        {
            return sprite;
        }
        return null;
    }

    public GameObject GetWeaponModel(string addressKey)
    {
        if (_weaponModelCache.TryGetValue(addressKey, out var prefab))
        {
            return prefab;
        }
        return null;
    }
}