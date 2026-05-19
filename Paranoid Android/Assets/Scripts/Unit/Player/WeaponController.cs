using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations.Rigging;
using UnityEngine.ResourceManagement.AsyncOperations;

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
    [Header("引用")]
    public MyPool.BulletPool bulletPool;
    public Transform firePoint;
    public BulletType bulletType;
    private PlayerController _player;
    private PlayerStateData _stateData;

    [Header("武器挂载")]
    public Transform weaponHolder;
    public TwoBoneIKConstraint rightHandIK;
    public TwoBoneIKConstraint leftHandIK;
    public RigBuilder rigBuilder;
    private GameObject _currentModelInstance;
    private AsyncOperationHandle<GameObject> _loadHandle;

    [Header("设置")]
    public float fireRate = 10f;
    public float loadPerShot = 2f;
    public float baseSpread = 5f;
    public float aimSpreadMult = 0.4f;
    public float aimSpeed = 1f;
    public int damage;
    public float bulletSpeed;
    public float distance;
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

    public async void EquipWeapon(WeaponData data)
    {
        UnloadCurrentWeapon();

        if (data == null) return;

        fireRate = data.fireRate;
        loadPerShot = data.loadPerShot;
        baseSpread = data.baseSpread;
        aimSpreadMult = data.aimSpreadMult;
        aimSpeed = data.aimSpeed;
        distance = data.distance;
        bulletSpeed = data.bulletSpeed;

        string addressKey = data.itemName + "Model";

        _loadHandle = Addressables.InstantiateAsync(addressKey, weaponHolder);
        _currentModelInstance = await _loadHandle.Task;

        if (_currentModelInstance != null)
        {
            _currentModelInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentModelInstance.transform.localScale = Vector3.one;

            Transform muzzle = _currentModelInstance.transform.Find("Muzzle");
            if (muzzle != null)
            {
                firePoint = muzzle;
            }
            else
            {
                Debug.LogWarning($"在武器模型 {addressKey} 中未找到名为 'Muzzle' 的子物体！");
            }

            Transform rhTarget = _currentModelInstance.transform.Find("RightTarget");
            Transform lhTarget = _currentModelInstance.transform.Find("LeftTarget");

            if (rhTarget != null && rightHandIK != null) rightHandIK.data.target = rhTarget;
            if (lhTarget != null && leftHandIK != null) leftHandIK.data.target = lhTarget;

            if (rigBuilder != null)
            {
                rigBuilder.Build();
            }
        }
    }

    public void UnloadCurrentWeapon()
    {
        firePoint = null;
        if (rightHandIK != null) rightHandIK.data.target = null;
        if (leftHandIK != null) leftHandIK.data.target = null;

        if (rigBuilder != null) rigBuilder.Build();

        if (_currentModelInstance != null)
        {
            Addressables.Release(_loadHandle);
            _currentModelInstance = null;
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
        if (firePoint == null) return;

        Vector3 playerForward = _player.transform.forward;
        playerForward.y = 0f;
        playerForward = playerForward.normalized;

        Vector3 targetWorldPos = _player.mouseWorldPosition;
        targetWorldPos.y = firePoint.position.y;

        Vector3 mouseDirection = (targetWorldPos - firePoint.position).normalized;

        Vector3 baseDirection;

        if (mouseDirection == Vector3.zero)
        {
            baseDirection = playerForward;
        }
        else
        {
            float angle = Vector3.Angle(playerForward, mouseDirection);
            if (angle <= 45f)
            {
                baseDirection = mouseDirection;
            }
            else
            {
                baseDirection = playerForward;
            }
        }

        Quaternion lookRotation = Quaternion.LookRotation(baseDirection);

        float curAngle = GetSpreadAngle();
        float randomSpread = Random.Range(-curAngle, curAngle);
        Quaternion finalBulletRotation = lookRotation * Quaternion.Euler(0, randomSpread, 0);

        string typeKey = bulletType.ToKey();
        bulletPool.GetAndSet(typeKey, firePoint.position, finalBulletRotation, bulletSpeed, distance);

        _stateData.Overload(loadPerShot);
    }
}
