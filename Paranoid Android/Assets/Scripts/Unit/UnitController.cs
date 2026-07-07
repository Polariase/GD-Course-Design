using System;
using UnityEngine;

public abstract class UnitController : MonoBehaviour, IHittable
{
    [Header("移动设置")]
    public float moveSpeed = 6f;

    [Header("引用")]
    public Animator animator;
    public CapsuleCollider capsule;

    public bool isDead;
    public bool isMoving;
    public bool isInvincible;

    protected virtual void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        capsule = GetComponentInChildren<CapsuleCollider>();
    }

    public virtual void Say(string text, float duration = 2f, WordTextType type = WordTextType.Neutral)
    {
        if (isDead) return;

        Vector3 spawnPos = capsule != null  ? capsule.bounds.center + Vector3.up * capsule.bounds.extents.y
            : transform.position + Vector3.up * 2f;

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowWordText(spawnPos, text, type, transform, duration);
        }
    }

    public virtual bool Hit(int damage, Vector3 hitPoint, bool isCrit)
    {
        if (isInvincible) return false;
        return TakeDamage(damage,hitPoint,isCrit);
    }

    public virtual bool TakeDamage(int damage,Vector3 hitPoint,bool isCrit)
    {
        return false;
    }

    public Vector3 HitPoint()
    {
        return capsule.bounds.center;
    }

    public virtual void Die()
    {
        return;
    }
}