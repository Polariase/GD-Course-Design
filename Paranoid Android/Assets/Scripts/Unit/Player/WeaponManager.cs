using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("“˝”√")]
    public PlayerStateData stateData;
    public MyPool.BulletPool bulletPool;
    public Transform firePoint;

    [Header("…Ë÷√")]
    public float fireRate = 10f;
    public float loadPerShot = 2f;

    private float _fireTimer;
    private PlayerController _player;

    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
        if (_player.CanFire && _player.isFiring)
        {
            TryFire();
        }
    }

    void TryFire()
    {
        if (_player.CanFire && _player.isFiring && _fireTimer <= 0 && !stateData.overloaded)
        {
            Fire();
            _fireTimer = 1f / fireRate;
        }
    }

    void Fire()
    {
        GameObject bullet = bulletPool.Get("Normal");
        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        stateData.Overload(loadPerShot);
    }
}
