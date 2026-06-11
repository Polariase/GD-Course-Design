using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWeapon : MonoBehaviour
{
    public MyPool.BulletPool bulletPool;
    public Transform firePoint;
    public BulletType bulletType = BulletType.Normal;

    public float fireRate = 4f;
    public float bulletSpeed = 15f;
    public float distance = 25f;   
    public float spreadAngle = 3.5f;  

    private float _fireTimer;

    private void Awake()
    {
        bulletPool = PoolManager.Instance.bullet;
        firePoint = transform.Find("FirePoint");
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

        Vector3 fireDirection = (target.position - firePoint.position).normalized;

        if (fireDirection == Vector3.zero) fireDirection = firePoint.forward;

        Quaternion baseRotation = Quaternion.LookRotation(fireDirection);

        float randomSpread = Random.Range(-spreadAngle, spreadAngle);
        Quaternion finalBulletRotation = baseRotation * Quaternion.Euler(0, randomSpread, 0);

        string typeKey = bulletType.ToKey();
        bulletPool.GetAndSet(typeKey, firePoint.position, finalBulletRotation, bulletSpeed, distance);

        _fireTimer = (repeating ? 0.5f : 1f) / fireRate;
    }
}
