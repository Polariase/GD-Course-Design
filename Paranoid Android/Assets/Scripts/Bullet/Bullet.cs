using MyPool;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子弹设置")]
    public float speed = 15f;
    public float maxDistance = 20f;
    public LayerMask hitLayer;
    public bool UseFirePointRotation;
    public float hitOffset = 0f;
    public Vector3 rotationOffset = new(0, 0, 0);
    private float _maxLifeTime;
    private string _poolKey;
    private float _lifeTime = 0f;
    private bool _isHit = false;
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
        _targetLayer = LayerMask.NameToLayer("Enemy");
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
        if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, moveStep + 0.1f, hitLayer))
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

    public void Init(float bulletSpeed, float maxDistance)
    {
        speed = bulletSpeed;
        this.maxDistance = maxDistance;
    }

    void HandleHit(RaycastHit hit)
    {
        _isHit = true;
        transform.position = hit.point;
        if (lightSource != null) lightSource.enabled = false;
        if (projectilePS != null) projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 让特效贴合碰撞面的法线
        if(hitObj != null)
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

            if (hit.collider.gameObject.layer == _targetLayer)
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

        // 根据命中特效时长延迟回收
        float delay = (hitPS != null) ? hitPS.main.duration : 1f;
        StartCoroutine(DisableTimer(delay));
    }

    private IEnumerator DisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(gameObject, _poolKey);
        }
    }
}
