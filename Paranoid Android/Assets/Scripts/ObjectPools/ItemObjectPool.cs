using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using MyPool;
using System.Collections.Generic;

public class ItemObjectPool : MyObjectPool
{
    private Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();

    public async Task SpawnItemAsync(InventoryItem data, Vector3 position, Quaternion rotation = default)
    {
        string key = data.data.prefabAddress;

        if (!pool.ContainsKey(key))
        {
            await CreatePoolFromAddressable(key);
        }

        GameObject obj = Get(key);
        obj.transform.SetPositionAndRotation(position, rotation);

        if (obj.TryGetComponent<ItemObject>(out var itemObj))
        {
            itemObj.Setup(data);
        }
    }

    public async Task SpawnAndThrowItemAsync(InventoryItem data, Vector3 position, Vector3 targetDirection, float upwardForce = 8f, float forwardForce = 2f)
    {
        string key = data.data.prefabAddress;
        if (!pool.ContainsKey(key)) await CreatePoolFromAddressable(key);

        GameObject obj = Get(key);
        obj.transform.SetPositionAndRotation(position, Quaternion.identity);

        if (obj.TryGetComponent<ItemObject>(out var itemObj))
        {
            itemObj.Setup(data);
        }

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            targetDirection.y = 0;
            Vector3 throwDir = targetDirection.normalized;
            Vector3 forceVector = (throwDir * forwardForce) + (Vector3.up * upwardForce);
            rb.AddForce(forceVector, ForceMode.Impulse);
        }
    }

    private async Task CreatePoolFromAddressable(string key)
    {
        if (pool.ContainsKey(key)) return;

        if (_loadedPrefabs.ContainsKey(key))
        {
            while (!pool.ContainsKey(key))
            {
                await Task.Yield();
            }
            return;
        }

        _loadedPrefabs.Add(key, null);

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedPrefabs[key] = handle.Result;

            pool.Add(key, CreatePool(handle.Result));
        }
    }

    public void ReleaseItem(GameObject obj)
    {
        obj.GetComponent<ItemObject>().Setup(null);
        Release(obj, obj.GetComponent<PoolItem>().key);
    }
}