using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("“˝”√")]
    public PlayerStateData stateData;
    public MyPool.BulletPool bulletPool;
    public Transform firePoint;
    public string bulletType;

    [Header("…Ë÷√")]
    public float fireRate = 10f;
    public float loadPerShot = 2f;
    public float baseSpread = 5f;
    public float aimSpreadMult = 0.4f;
    public float aimSpeed = 1f;
    private float _aimWeight;

    private float _fireTimer;
    private PlayerController _player;


    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        HandleAimWeight();
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
        if (_player.CanFire && _player.isFiring)
        {
            TryFire();
        }
    }
    public float GetSpreadAngle()
    {
        float spread = baseSpread * (1f + stateData.OverloadPercentage);
        float currentMult = Mathf.Lerp(1f, aimSpreadMult, _aimWeight);
        return spread * currentMult;
    }

    void HandleAimWeight()
    {
        float targetWeight = _player.isAiming && !_player.isDashing ? 1f : 0f;
        _aimWeight = Mathf.MoveTowards(_aimWeight, targetWeight, Time.deltaTime / aimSpeed);
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
        float curAngle = GetSpreadAngle();
        float randomSpread = Random.Range(-curAngle, curAngle);
        Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(0, randomSpread, 0);
        bulletPool.GetAndSet(bulletType, firePoint.position, spreadRotation);
        stateData.Overload(loadPerShot);
    }
}
