using BehaviorDesigner.Runtime;
using MyPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolDroneController : EnemyController
{
    public float activateRadius = 5f;
    public bool isAlert;
    public Rigidbody rb;
    public override bool Ready => isAlert;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        Init();
    }

    public override void Init()
    {
        base.Init();
        agent.baseOffset = 0f;
        animator.SetBool("IsAlert", false);
        behaviorTree.SetVariableValue("IsAlert", false);
        isAlert = false;
    }

    private void Update()
    {
        Scan();
        if (animator != null && agent.isActiveAndEnabled)
        {
            // 如果寻路组件正在移动，且速度大于一个极小值，则认为在移动
            bool moving = agent.remainingDistance > agent.stoppingDistance && agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool("IsMoving", moving);
        }
    }

    protected override void Scan()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer >= SCAN_INTERVAL)
            scanTimer = 0f;
        else
            return;
        float radius = Ready ? detectRadius : activateRadius;
        Collider[] targets = Physics.OverlapSphere(transform.position, radius, playerLayer);
        if ((targets.Length > 0))
        {
            if (!Ready && !animator.GetBool("IsAlert"))
                animator.SetBool("IsAlert", true);
            currentTarget = targets[0].transform;
            lostPos = currentTarget.position;
        }
        else
            currentTarget = null;
    }

    public void CompleteActivation()
    {
        isAlert = true;
        behaviorTree.SetVariableValue("IsAlert", true);
    }

    public override bool TakeDamage(int damage, Vector3 hitPoint, bool isCrit)
    {
        bool isDead = base.TakeDamage(damage, hitPoint, isCrit);
        if (!isDead && !Ready && !animator.GetBool("IsAlert"))
        {
            animator.SetBool("IsAlert", true);
        }
        return isDead;
    }

    public override void Die()
    {
        base.Die();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }

        StartCoroutine(DieCoroutine(2f));
    }

    private IEnumerator DieCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(DeathVFX, HitPoint(), Quaternion.identity);
        PoolManager.Instance.enemy.Release(gameObject,GetComponent<PoolItem>().key);
        Vector3 explosionPos = HitPoint();
        if (PoolManager.Instance != null && PoolManager.Instance.aud != null && AudioManager.Instance != null)
        {
            AudioClip explosionAudio = AudioManager.Instance.genericExplosionClip;

            if (explosionAudio != null)
            {
                PoolManager.Instance.aud.PlaySoundAtPoint("Audio", explosionAudio, explosionPos, 0.88f, 1.12f, true);
            }
        }
    }
}
