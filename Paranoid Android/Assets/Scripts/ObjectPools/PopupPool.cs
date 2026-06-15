using UnityEngine;
using System.Collections.Generic;

namespace MyPool
{
    public class PopupPool : MyObjectPool
    {
        public Transform canvasTransform;

        protected override void OnGet(GameObject obj)
        {
            if (obj != null && canvasTransform != null)
            {
                obj.transform.SetParent(canvasTransform, false);
            }
        }

        protected override void OnRelease(GameObject obj)
        {
            if (obj != null)
            {
                obj.transform.SetParent(transform, false);
                obj.SetActive(false);
            }
        }

        public GameObject GetAndSet(string key, Vector3 worldPos, int damage, bool isCrit)
        {
            GameObject obj = Get(key);

            if (obj.TryGetComponent<DamageText>(out var damageText))
            {
                damageText.Init(damage, worldPos, isCrit);
            }

            obj.SetActive(true);

            return obj;
        }

        public GameObject Get(string key,Vector3 pos)
        {
            GameObject go = Get(key);
            go.transform.position = pos;
            go.SetActive(true);
            return go;
        }
    }
}