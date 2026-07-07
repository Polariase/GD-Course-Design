using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWeapon : MonoBehaviour
{
    public MyPool.BulletPool bulletPool;
    public Transform firePoint;
    public BulletType bulletType = BulletType.Normal;
    public EnemyController unit;

    public float fireRate = 4f;
    public float bulletSpeed = 15f;
    public float distance = 25f;   
    public float spreadAngle = 3.5f;

    private AudioSource _weaponAudioSource;

    private float _fireTimer;

    private void Awake()
    {
        firePoint = transform.Find("FirePoint");
        if (firePoint != null)
        {
            _weaponAudioSource = firePoint.GetComponent<AudioSource>();
        }
        unit = GetComponent<EnemyController>();
    }

    private void Start()
    {
        bulletPool = PoolManager.Instance.bullet;
        if (_weaponAudioSource != null && AudioManager.Instance != null)
        {
            _weaponAudioSource.spatialBlend = 1.0f;
            _weaponAudioSource.minDistance = 2f;
            _weaponAudioSource.maxDistance = 25f;
            _weaponAudioSource.rolloffMode = AudioRolloffMode.Linear;
            _weaponAudioSource.outputAudioMixerGroup = AudioManager.Instance.GetSFXGroup();
            _weaponAudioSource.playOnAwake = false;
            _weaponAudioSource.loop = false;
        }
    }

    void Update()
    {
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
    }

    public bool CanFire()
    {
        return _fireTimer <= 0f;
    }

    public void Fire(Transform target, bool repeating = false)
    {
        if (!CanFire() || firePoint == null || target == null) return;

        Vector3 targetPos;

        UnitController targetUnit = target.gameObject.GetComponentInParent<UnitController>();

        if (targetUnit != null)
        {
            targetPos = targetUnit.HitPoint(); 
        }
        else
        {
            Debug.LogWarning("Target does not have UnitController, using target's position instead.");
            targetPos = target.position;
        }

        Vector3 fireDirection = (targetPos - firePoint.position).normalized;

        if (fireDirection == Vector3.zero) fireDirection = firePoint.forward;

        Quaternion baseRotation = Quaternion.LookRotation(fireDirection);

        float randomSpread = Random.Range(-spreadAngle, spreadAngle);
        Quaternion finalBulletRotation = baseRotation * Quaternion.Euler(0, randomSpread, 0);

        string typeKey = bulletType.ToKey();

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        bulletPool.GetAndSet(typeKey, firePoint.position, finalBulletRotation, bulletSpeed, distance, enemyLayer, unit.damage, null);

        _fireTimer = (repeating ? 0.5f : 1f) / fireRate;

        if (_weaponAudioSource != null && AudioManager.Instance != null)
        {
            AudioClip currentFireClip = AudioManager.Instance.GetWeaponFireClip();

            if (currentFireClip != null)
            {
                AudioManager.Instance.Play3DSound(_weaponAudioSource, currentFireClip);
            }
        }
    }
}
