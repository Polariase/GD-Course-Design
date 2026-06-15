using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class EnemyController : UnitController
{
    public int maxHp = 60;
    public int hp;

    public LayerMask playerLayer;
    public Transform currentTarget;
    public NavMeshAgent agent;
    public Vector3 lostPos;
    public float detectRadius = 25f;
    public float combatRadius = 7f;
    public int damage = 10;
    public GameObject DeathVFX;

    protected BehaviorTree behaviorTree;
    protected float scanTimer = 0f;
    protected const float SCAN_INTERVAL = 0.2f;

    public virtual bool Ready => true;

    protected override void Awake()
    {
        base.Awake();
        playerLayer = 1 << LayerMask.NameToLayer("Player");
        agent = GetComponent<NavMeshAgent>();
        behaviorTree = GetComponent<BehaviorTree>();
        agent.speed = moveSpeed;
        agent.acceleration = 4f * moveSpeed;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 1f;
    }

    protected void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        isDead = false;
        hp = maxHp;
        lostPos = transform.position;
        behaviorTree.EnableBehavior();
        agent.enabled = true;
    }

    protected virtual void Scan()
    {
        if (isDead)
            return;

        if (!Ready)
            return;
        scanTimer += Time.deltaTime;
        if (scanTimer >= SCAN_INTERVAL)
            scanTimer = 0f;
        else
            return;
        Collider[] targets = Physics.OverlapSphere(transform.position, detectRadius, playerLayer);
        if ((targets.Length > 0))
        {
            currentTarget = targets[0].transform;
            lostPos = currentTarget.position;
        }
        else
            currentTarget = null;
    }

    public override bool TakeDamage(int damage, Vector3 hitPoint, bool isCrit)
    {
        if (isDead) return false;
        hp -= damage;
        PopupManager.Instance.ShowDamage(hitPoint, damage, isCrit);
        if (hp <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    public override void Die()
    {
        if (behaviorTree != null)
        {
            behaviorTree.DisableBehavior();
        }

        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        isDead = true;
    }

}
