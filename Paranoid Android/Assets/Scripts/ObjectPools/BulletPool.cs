using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace MyPool
{
    public class BulletPool : MyObjectPool
    {
        protected override void OnGet(GameObject obj) { }

        public GameObject GetAndSet(string key, Vector3 pos, Quaternion rot, float bulletSpeed, float distance)
        {
            GameObject obj = Get(key);
            obj.transform.SetPositionAndRotation(pos, rot);
            if (obj.TryGetComponent<Bullet>(out var bullet))
            {
                bullet.Init(bulletSpeed, distance);
            }
            obj.SetActive(true);
            return obj;
        }
    }
}

