using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum BulletType
{
    Normal,
    Arrow,
    Cube,
    MagicArrow,
    RedStar,
    Scarlet,
    Twist
}

public static class BulletTypeExtensions
{
    public static string ToKey(this BulletType type)
    {
        return type switch
        {
            BulletType.Normal => "Normal",
            BulletType.Arrow => "Arrow",
            BulletType.Cube => "Cube",
            BulletType.MagicArrow => "MagicArrow",
            BulletType.RedStar => "RedStar",
            BulletType.Scarlet => "Scarlet",
            BulletType.Twist => "Twist",
            _ => "Normal"
        };
    }
}


public class WeaponController : MonoBehaviour
{
    [Header("“˝”√")]
    public MyPool.BulletPool bulletPool;
    public Transform firePoint;
    public BulletType bulletType;
    private PlayerController _player;
    private PlayerStateData _stateData;

    [Header("…Ë÷√")]
    public float fireRate = 10f;
    public float loadPerShot = 2f;
    public float baseSpread = 5f;
    public float aimSpreadMult = 0.4f;
    public float aimSpeed = 1f;
    public float aimWeight;
    public AnimationCurve aimCurve;

    private float _aimTimer;
    private float _fireTimer;


    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    private void Start()
    {
        _stateData = _player.stateData;
    }

    void Update()
    {
        HandleAimWeight();
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
        if (_player.CanFire && _player.isFiring)
        {
            TryFire();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            int currentIndex = (int)bulletType;
            int nextIndex = (currentIndex + 1) % 7;
            bulletType = (BulletType)nextIndex;
        }
    }

    public float GetSpreadAngle()
    {
        float spread = baseSpread * (1f + _stateData.OverloadPercentage);
        float currentMult = Mathf.Lerp(1f, aimSpreadMult, aimWeight);
        return spread * currentMult;
    }

    void HandleAimWeight()
    {
        float direction = _player.isAiming && !_player.isDashing ? 1f : -2f;
        _aimTimer += direction * Time.deltaTime;
        _aimTimer = Mathf.Clamp(_aimTimer, 0, aimSpeed);
        float normalizedTime = _aimTimer / aimSpeed;
        aimWeight = aimCurve.Evaluate(normalizedTime);
    }

    void TryFire()
    {
        if (_player.CanFire && _player.isFiring && _fireTimer <= 0 && !_stateData.overloaded)
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
        string typeKey = bulletType.ToKey();
        bulletPool.GetAndSet(typeKey, firePoint.position, spreadRotation);
        _stateData.Overload(loadPerShot);
    }
}
