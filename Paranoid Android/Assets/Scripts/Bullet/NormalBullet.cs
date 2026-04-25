using MyPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    [Header("…Ë÷√")]
    public float speed = 40f;
    public float maxDistance = 20f;
    public LayerMask hitLayer;
    public string poolKey = "Normal";

    private Vector3 _startPosition;
    [SerializeField] private BulletPool _pool;

    void OnEnable()
    {
        _startPosition = transform.position;
        if (_pool == null) _pool = FindFirstObjectByType<BulletPool>();
    }

    void Update()
    {
        float moveStep = speed * Time.deltaTime;
        Vector3 direction = transform.forward;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveStep + 0.1f, hitLayer))
        {
            HandleHit(hit);
            return;
        }

        transform.position += direction * moveStep;

        if (Vector3.Distance(_startPosition, transform.position) >= maxDistance)
        {
            ReturnToPool();
        }
    }

    void HandleHit(RaycastHit hit)
    {
        transform.position = hit.point;
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(gameObject, poolKey);
        }
    }
}
