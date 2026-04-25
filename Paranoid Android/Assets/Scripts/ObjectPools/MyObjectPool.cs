using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MyPool
{
    public class MyObjectPool : MonoBehaviour
    {
        public Dictionary<string, ObjectPool<GameObject>> pool;
        public Dictionary<string, GameObject> config;
        public List<GameObject> prefabs;

        public virtual GameObject Get(string key)
        {
            return pool[key].Get();
        }
        public virtual void Release(GameObject obj, string key)
        {
            pool[key].Release(obj);
        }

        protected virtual void Awake()
        {
            config = new Dictionary<string, GameObject>();
            Configure();
            pool = new Dictionary<string, ObjectPool<GameObject>>();
            foreach (var _item in config)
            {
                string _key = _item.Key;
                GameObject _obj = _item.Value;
                ObjectPool<GameObject> _pool = CreatePool(_obj);
                pool.Add(_key, _pool);
            }
        }

        protected virtual ObjectPool<GameObject> CreatePool(GameObject _obj)
        {
            return new(
                    /*createFunc:*/ () => Instantiate(_obj, transform),
                    /*actionOnGet:*/ (obj) => { if (obj != null) obj.SetActive(true); else Debug.LogWarning("Get Null From Pool"); },
                    /*actionOnRelease:*/ (obj) => { if (obj != null) obj.SetActive(false); else Debug.LogWarning("Free Null From Pool"); },
                    /*actionOnDestroy*:*/ (obj) => { if (obj != null) Destroy(obj); },
                    /*collectionCheck:*/ true,
                    /*defaultCapacity:*/ 10,
                    /*maxSize:*/ 80);
        }

        protected virtual void Configure()
        {
            foreach (var item in prefabs)
            {
                config.Add(item.GetComponent<PoolItem>().key, item);
            }
        }

        public void DeactivateAllPoolObjects()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    Release(child.gameObject, child.gameObject.GetComponent<PoolItem>().key);
                }
            }
        }
    }
}