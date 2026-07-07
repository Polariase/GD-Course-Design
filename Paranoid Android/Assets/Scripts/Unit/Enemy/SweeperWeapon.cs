using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperWeapon : MonoBehaviour
{
    public MyPool.BulletPool bulletPool;
    public EnemyController unit;

    public Transform mainFirePoint;
    public BulletType mainBulletType = BulletType.Missile;
    public float mainFireRate = 1f;
    public float mainBulletSpeed = 50f;
    public float mainDistance = 80f;   
    public float mainSpreadAngle = 1f; 
    public int mainDamage = 40;        

    public Transform[] subFirePoints;
    public BulletType subBulletType = BulletType.Normal;
    public float subFireRate = 9f;
    public float subBulletSpeed = 25f;
    public float subDistance = 35f;
    public float subSpreadAngle = 25f; 
    public int subDamage = 12;         

    private float _mainFireTimer;
    private float _subFireTimer;
    private int _currentSubBarrelIndex = 0;

    private void Awake()
    {
        unit = GetComponent<EnemyController>();
    }

    private void Start()
    {
        bulletPool = PoolManager.Instance.bullet;
    }

    void Update()
    {
        if (_mainFireTimer > 0) _mainFireTimer -= Time.deltaTime;
        if (_subFireTimer > 0) _subFireTimer -= Time.deltaTime;
    }

    public void FireMain(Transform target)
    {
        if (_mainFireTimer > 0f || mainFirePoint == null || target == null) return;

        Vector3 targetPos = GetTargetHitPoint(target);
        Vector3 fireDirection = (targetPos - mainFirePoint.position).normalized;
        if (fireDirection == Vector3.zero) fireDirection = mainFirePoint.forward;

        Quaternion finalRotation = CalculateSpread(fireDirection, mainSpreadAngle);
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        bulletPool.GetAndSet(mainBulletType.ToKey(), mainFirePoint.position, finalRotation,
                             mainBulletSpeed, mainDistance, enemyLayer, mainDamage, null);

        _mainFireTimer = 1f / mainFireRate;
    }

    public void FireSub(Transform target)
    {
        if (_subFireTimer > 0f || subFirePoints == null || subFirePoints.Length == 0 || target == null) return;

        Transform currentFirePoint = subFirePoints[_currentSubBarrelIndex];

        if (currentFirePoint != null)
        {
            Vector3 targetPos = GetTargetHitPoint(target);
            Vector3 fireDirection = (targetPos - currentFirePoint.position).normalized;
            if (fireDirection == Vector3.zero) fireDirection = currentFirePoint.forward;

            Quaternion finalRotation = CalculateSpread(fireDirection, subSpreadAngle);
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            bulletPool.GetAndSet(subBulletType.ToKey(), currentFirePoint.position, finalRotation,
                                 subBulletSpeed, subDistance, enemyLayer, subDamage, null);
        }

        _currentSubBarrelIndex = (_currentSubBarrelIndex + 1) % subFirePoints.Length;

        _subFireTimer = 1f / subFireRate;
    }

    private Vector3 GetTargetHitPoint(Transform target)
    {
        UnitController targetUnit = target.gameObject.GetComponentInParent<UnitController>();
        return targetUnit != null ? targetUnit.HitPoint() : target.position;
    }

    private Quaternion CalculateSpread(Vector3 direction, float spread)
    {
        Quaternion baseRotation = Quaternion.LookRotation(direction);
        float randomSpread = Random.Range(-spread, spread);
        return baseRotation * Quaternion.Euler(0, randomSpread, 0);
    }

    public float GetMaxWeaponDistance()
    {
        return Mathf.Max(mainDistance, subDistance);
    }
}