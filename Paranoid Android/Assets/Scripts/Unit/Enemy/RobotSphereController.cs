using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class RobotSphereController : EnemyController
{
    public float activateRadius = 15f;
    public float rollDistance;
    public bool isAlert;
    public bool isRolling;
    public float rollSpeedMultiplier = 3f;
    public bool isRollReady;
    public int rollDamage = 50;

    private Vector3 rollDirection;
    private Vector3 rollStartPos;

    public override bool Ready => isAlert;

    protected override void Awake()
    {
        base.Awake();
        combatRadius = detectRadius * 0.55f;
        rollDistance = detectRadius * 0.6f;
        Init();
    }

    public override void Init()
    {
        base.Init();
        animator.SetBool("IsDead", false);
        isAlert = false;
        animator.SetBool("IsAlert", false);
        behaviorTree.SetVariableValue("IsAlert", false);
        isRolling = false;
        animator.SetBool("IsRolling", false);
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
        if (isDead)
            return;

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

    public void StartRollAttack()
    {
        if (currentTarget == null || isDead) return;

        isRolling = true;
        isRollReady = false;
        animator.SetBool("IsRolling", true);
        animator.SetTrigger("Roll");

        rollDirection = (currentTarget.position - transform.position).normalized;
        rollDirection.y = 0;

        rollStartPos = transform.position;

        agent.speed = moveSpeed * rollSpeedMultiplier;
        agent.acceleration = agent.speed * 4;
    }

    public void OnRollStartFinished()
    {
        isRollReady = true;
        if (currentTarget != null)
        {
            rollDirection = (currentTarget.position - transform.position).normalized;
            rollDirection.y = 0;

            if (rollDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(rollDirection);

            rollStartPos = transform.position;
        }
    }

    public bool CheckAndExecuteRolling()
    {
        if (!isRolling) return false;

        if (!isRollReady)
        {
            if (agent.speed != moveSpeed)
            {
                agent.ResetPath();
                if (currentTarget != null)
                {
                    Vector3 targetDir = (currentTarget.position - transform.position).normalized;
                    targetDir.y = 0;

                    if (targetDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(targetDir);
                    }
                }
            }
            return true;
        }

        Vector3 currentMoveDelta = transform.position - rollStartPos;
        float actualRolledDistance = Vector3.Project(currentMoveDelta, rollDirection).magnitude;

        if (actualRolledDistance >= rollDistance - 0.5f)
        {
            StopRollingPhase();
            return false;
        }

        Vector3 desiredMove = rollDirection * agent.speed * Time.deltaTime;
        float idealStepDistance = desiredMove.magnitude;
        Vector3 posBeforeMove = transform.position;
        agent.Move(desiredMove);
        Vector3 posAfterMove = transform.position;
        float actualStepDistance = Vector3.Distance(posBeforeMove, posAfterMove);
        if (actualStepDistance < idealStepDistance * 0.5f)
        {
            StopRollingPhase();
            return false;
        }

        return true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (isRolling && isRollReady)
        {
            UnitController unit = collision.gameObject.GetComponent<UnitController>();
            if (unit != null)
            {
                if (unit is PlayerController playerController)
                {
                    Vector3 contactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : unit.transform.position;
                    playerController.TakeDamage(50, contactPoint, true);
                }
                StopRollingPhase();
            }

        }
    }

    public void CompleteActivation()
    {
        isAlert = true;
        behaviorTree.SetVariableValue("IsAlert", true);
    }

    public void CompleteRolling()
    {
        isRolling = false;
    }

    public void StopRollingPhase()
    {
        agent.speed = moveSpeed;
        agent.acceleration = agent.speed * 4;
        isRollReady = false;
        agent.ResetPath();
        animator.SetBool("IsRolling", false); // 此时 isRolling 依然为 true，直到 Roll_End 动画播完触发 CompleteRolling() 才会完全彻底结束
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
        if (isRolling)
        {
            agent.speed = moveSpeed;
            agent.acceleration = agent.speed * 4f;
            agent.ResetPath();

            isRolling = false;
            isRollReady = false;

            if (animator != null)
            {
                animator.SetBool("IsRolling", false);
            }
        }

        base.Die();

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
    }

    public void OnDeathAnimationFinished()
    {
        if (DeathVFX != null)
        {
            Instantiate(DeathVFX, HitPoint(), Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
