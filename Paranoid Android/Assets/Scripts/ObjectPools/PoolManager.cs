using MyPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public BulletPool bullet;
    public ItemObjectPool item;
    public PopupPool popup;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            bullet = GetComponentInChildren<BulletPool>();
            item = GetComponentInChildren<ItemObjectPool>();
            popup = GetComponentInChildren<PopupPool>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DeactiveAll()
    {
        bullet.DeactivateAllPoolObjects();
    }
}