using System;
using UnityEngine;

public abstract class UnitController : MonoBehaviour, IHittable
{
    [Header("移动设置")]
    public float moveSpeed = 6f;

    [Header("引用")]
    public Animator animator;
    public Rigidbody rb;
    public CapsuleCollider capsule;

    public bool isMoving;
    public bool isInvincible;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        capsule = GetComponentInChildren<CapsuleCollider>();
    }

    public virtual bool Hit(float damage, RaycastHit hitInfo)
    {
        return TakeDamage(damage);
    }

    public virtual bool TakeDamage(float damage)
    {
        return false;
    }

    public Vector3 HitPoint()
    {
        return capsule.bounds.center;
    }
}