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
    public EnemyPool enemy;
    public AudioPool aud;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            bullet = GetComponentInChildren<BulletPool>();
            item = GetComponentInChildren<ItemObjectPool>();
            popup = GetComponentInChildren<PopupPool>();
            aud = GetComponentInChildren<AudioPool>();
            enemy = GetComponentInChildren<EnemyPool>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DeactiveAll()
    {
        bullet.DeactivateAllPoolObjects();
        item.DeactivateAllPoolObjects();
        popup.DeactivateAllPoolObjects();
        enemy.DeactivateAllPoolObjects();
        aud.DeactivateAllPoolObjects();
    }
}