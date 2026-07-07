using MyPool;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子弹设置")]
    public float speed = 15f;
    public float maxDistance = 20f;
    public int damage = 1;
    public bool UseFirePointRotation;
    public float hitOffset = 0f;
    public Vector3 rotationOffset = new(0, 0, 0);
    private float _maxLifeTime;
    private string _poolKey;
    private float _lifeTime = 0f;
    private bool _isHit = false;
    private LayerMask _hitLayer;
    private int _targetLayer;

    [Header("引用")]
    public GameObject hitObj;
    public ParticleSystem hitPS;
    public ParticleSystem shatteringPS;
    public GameObject flashObj;
    public ParticleSystem projectilePS;
    public GameObject[] detachedObj;
    public Light lightSource;
    private BulletPool _pool;
    private ParticleSystem[] _detachedPS;

    public IHittable recordedTarget = null; // 发射时准星锁定的敌人
    public float critRate = 0f;            // 暴击率
    public const int critMultiplier = 2;      // 暴击伤害倍率


    private void Awake()
    {
        if (_pool == null) _pool = PoolManager.Instance.bullet;
        lightSource = GetComponent<Light>();
        _detachedPS = new ParticleSystem[detachedObj.Length];
        for (int i = 0; i < detachedObj.Length; i++)
        {
            if (detachedObj[i] != null)
                _detachedPS[i] = detachedObj[i].GetComponent<ParticleSystem>();
        }
        _poolKey = GetComponent<PoolItem>().key;
    }

    void OnEnable()
    {
        if (flashObj != null)
        {
            flashObj.transform.parent = null;
        }
        if (lightSource != null)
            lightSource.enabled = true;
        _maxLifeTime = speed > 0 ? (maxDistance / speed) : 1f;
        _lifeTime = 0f;
        _isHit = false;
    }

    void Update()
    {
        if (_isHit)
            return;
        float moveStep = speed * Time.deltaTime;
        Vector3 direction = transform.forward;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, moveStep + 0.1f, _hitLayer))
        {
            HandleHit(hitInfo);
            return;
        }

        transform.position += direction * moveStep;

        _lifeTime += Time.deltaTime;

        if (_lifeTime >= _maxLifeTime)
        {
            ReturnToPool();
        }
    }

    public void Init(float bulletSpeed, float maxDistance,int shooterLayer,int baseDamage, IHittable currentAimTarget, float critChance = 0.1f)
    {
        speed = bulletSpeed;
        this.maxDistance = maxDistance;
        gameObject.layer = shooterLayer;
        damage = baseDamage;
        recordedTarget = currentAimTarget;
        critRate = critChance;
        _targetLayer = shooterLayer == LayerMask.NameToLayer("Player") ? LayerMask.NameToLayer("Enemy") : LayerMask.NameToLayer("Player");

        int targetMask = 1 << _targetLayer;
        int defaultMask = 1 << LayerMask.NameToLayer("Default");
        int groundMask = 1 << LayerMask.NameToLayer("Ground");

        _hitLayer = targetMask | defaultMask | groundMask;
    }

    void HandleHit(RaycastHit hit)
    {
        _isHit = true;
        transform.position = hit.point;
        if (lightSource != null) lightSource.enabled = false;
        if (projectilePS != null) projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        bool isCrit = false;
        int tLayer = hit.collider.gameObject.layer;
        IHittable currentTarget = hit.collider.GetComponentInParent<IHittable>();

        bool hitFlesh = tLayer == _targetLayer;

        if (currentTarget != null && tLayer == _targetLayer)
        {
            if (recordedTarget != null && currentTarget == recordedTarget)
            {
                isCrit = true;
            }
            else if (Random.value <= critRate)
            {
                isCrit = true;
            }

            int finalDamage = isCrit ? (damage * critMultiplier) : damage;
            currentTarget.Hit(finalDamage, hit.point, isCrit);
        }

        // 让特效贴合碰撞面的法线
        if (hitObj != null)
        {
            Vector3 pos = hit.point + hit.normal * hitOffset;
            hitObj.transform.position = pos;

            // 旋转修正逻辑
            if (UseFirePointRotation)
                hitObj.transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
            else if (rotationOffset != Vector3.zero)
                hitObj.transform.rotation = Quaternion.Euler(rotationOffset);
            else
                hitObj.transform.LookAt(hit.point + hit.normal);

            if (isCrit)
            {
                // 命中目标触发完整效果
                if (hitPS != null) hitPS.Play();
            }
            else
            {
                // 否则只触发破碎效果
                if (shatteringPS != null) shatteringPS.Play();
            }
        }

        foreach (var ps in _detachedPS)
        {
            if (ps != null) ps.Stop();
        }

        if (PoolManager.Instance != null && PoolManager.Instance.aud != null)
        {
            AudioClip clipToPlay = hitFlesh ? AudioManager.Instance.fleshHitClip : AudioManager.Instance.wallHitClip;

            if (clipToPlay != null)
            {
                PoolManager.Instance.aud.PlaySoundAtPoint("Audio", clipToPlay, hit.point);
            }
        }

        // 根据命中特效时长延迟回收
        float delay = (hitPS != null) ? hitPS.main.duration : 1f;
        StartCoroutine(DisableTimer(delay));
    }

    private IEnumerator DisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        recordedTarget = null;
        StopAllCoroutines();

        if (flashObj != null)
        {
            flashObj.transform.parent = transform;
            flashObj.transform.localPosition = Vector3.zero;
            flashObj.transform.localEulerAngles = Vector3.zero;
        }

        if (hitObj != null)
        {
            if (hitPS != null) hitPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (shatteringPS != null) shatteringPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        foreach (var ps in _detachedPS)
        {
            if (ps != null) ps.Stop();
        }

        if (_pool != null)
        {
            _pool.Release(gameObject, _poolKey);
        }
    }
}
