using UnityEngine;

namespace MyPool
{
    public class EnemyPool : MyObjectPool
    {
        protected override void OnGet(GameObject obj) { }

        public GameObject SpawnAt(string key, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Get(key);
            if (obj == null) return null;
            if (obj.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.Spawn(pos, rot);
            }
            else
            {
                Debug.LogWarning($"{obj.name} 上找不到 EnemyController 脚本。");
                obj.transform.SetPositionAndRotation(pos, rot);
            }
            obj.SetActive(true);
            return obj;
        }
    }
}