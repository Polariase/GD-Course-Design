using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyPool
{
    public class BulletPool : MyObjectPool
    {
        protected override void OnGet(GameObject obj) { }

        public GameObject GetAndSet(string key, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Get(key);
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
            return obj;
        }
    }
}

