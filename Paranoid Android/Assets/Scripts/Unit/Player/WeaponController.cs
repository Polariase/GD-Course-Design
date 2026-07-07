using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum BulletType
{
    Normal,
    Arrow,
    Cube,
    MagicArrow,
    RedStar,
    Scarlet,
    Twist,
    Missile
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
            BulletType.Missile => "Missile",
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
    private AudioSource _weaponAudioSource;
    private PlayerController _player;
    private PlayerStateData _stateData;

    [Header("武器挂载")]
    public Transform weaponHolder;
    public TwoBoneIKConstraint rightHandIK;
    public TwoBoneIKConstraint leftHandIK;
    public RigBuilder rigBuilder;
    private GameObject _currentModelInstance;

    [Header("设置")]
    public float fireRate = 10f;
    public float loadPerShot = 2f;
    public float baseSpread = 5f;
    public float aimSpreadMult = 0.4f;
    public float aimSpeed = 1f;
    public int damage = 10;
    public float bulletSpeed;
    public float distance;
    public float aimWeight;
    public float critChance = 0.1f;
    public AnimationCurve aimCurve;

    public bool isTargetLocked;
    public IHittable currentUpdateTarget;
    private float _aimTimer;
    private float _fireTimer;


    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        bulletPool = PoolManager.Instance.bullet;
    }

    private void Start()
    {
        _stateData = _player.stateData;
    }

    void Update()
    {
        HandleAimWeight();

        UpdateTargetLockStatus();

        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
        if (_player.CanFire && _player.isFiring)
        {
            TryFire();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            int currentIndex = (int)bulletType;
            int nextIndex = (currentIndex + 1) % 8;
            bulletType = (BulletType)nextIndex;
        }
    }

    private void UpdateTargetLockStatus()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit aimHit, 100f, LayerMask.GetMask("Enemy")))
        {
            currentUpdateTarget = aimHit.collider.GetComponentInParent<IHittable>();
            isTargetLocked = (currentUpdateTarget != null);
        }
        else
        {
            currentUpdateTarget = null;
            isTargetLocked = false;
        }
    }

    public void EquipWeapon(WeaponData data)
    {
        UnloadCurrentWeapon();

        if (data == null) return;

        fireRate = data.fireRate;
        loadPerShot = data.loadPerShot;
        damage = data.damage;
        baseSpread = data.baseSpread;
        aimSpreadMult = data.aimSpreadMult;
        aimSpeed = data.aimSpeed;
        distance = data.distance;
        bulletSpeed = data.bulletSpeed;
        bulletType = data.bulletType;

        string addressKey = data.itemName + "Model";

        GameObject modelPrefab = DataManager.Instance.GetWeaponModel(addressKey);

        if (modelPrefab == null)
        {
            Debug.LogError($"[WeaponController] 装备失败，DataManager 中没有缓存该模型: {addressKey}");
            return;
        }

        _currentModelInstance = Instantiate(modelPrefab, weaponHolder);

        if (_currentModelInstance != null)
        {
            _currentModelInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentModelInstance.transform.localScale = Vector3.one;

            Transform muzzle = _currentModelInstance.transform.Find("Muzzle");
            if (muzzle != null)
            {
                firePoint = muzzle;
                _weaponAudioSource = muzzle.gameObject.GetComponent<AudioSource>();
                if (_weaponAudioSource == null)
                {
                    _weaponAudioSource = muzzle.gameObject.AddComponent<AudioSource>();
                }

                _weaponAudioSource.spatialBlend = 1f;
                _weaponAudioSource.minDistance = 2f;
                _weaponAudioSource.maxDistance = 25f;

                _weaponAudioSource.rolloffMode = AudioRolloffMode.Linear;

                if (AudioManager.Instance != null)
                {
                    _weaponAudioSource.outputAudioMixerGroup = AudioManager.Instance.GetSFXGroup();
                }

                _weaponAudioSource.playOnAwake = false;
                _weaponAudioSource.loop = false;
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
            Destroy(_currentModelInstance);
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

        IHittable aimedTarget = currentUpdateTarget;
        if (aimedTarget != null)
        {
            targetWorldPos = aimedTarget.HitPoint();
        }

        Vector3 mouseDirection = (targetWorldPos - firePoint.position).normalized;

        Vector3 baseDirection;

        if (mouseDirection == Vector3.zero)
        {
            baseDirection = playerForward;
        }
        else
        {
            Vector3 flatMouseDir = new Vector3(mouseDirection.x, 0f, mouseDirection.z).normalized;
            float angle = Vector3.Angle(playerForward, flatMouseDir);
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

        

        int playerLayer = LayerMask.NameToLayer("Player");

        bulletPool.GetAndSet(typeKey, firePoint.position, finalBulletRotation, bulletSpeed, distance, playerLayer, damage, aimedTarget, critChance);

        _stateData.Overload(loadPerShot);

        if (_weaponAudioSource != null && AudioManager.Instance != null)
        {
            AudioClip currentFireClip = AudioManager.Instance.GetWeaponFireClip();
            AudioManager.Instance.Play3DSound(_weaponAudioSource, currentFireClip);
        }
    }
}
