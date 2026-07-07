using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class SweeperFire : Action
{
    public SharedTransform target;
    public float warmup = 1f;
    public float rotationSpeed = 12f;
    public bool slowDownWhileFiring = false;
    public bool setRotation = true;
    public EnemyController unit;

    public float maxDuration = 0f;

    private SweeperWeapon weapon;
    private NavMeshAgent agent;
    private float currentTimer = 0f;
    private float warmupTimer;

    public override void OnAwake()
    {
        weapon = GetComponent<SweeperWeapon>();
        unit = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnStart()
    {
        warmupTimer = warmup;
        currentTimer = 0f;
        if (slowDownWhileFiring && agent != null)
        {
            agent.speed = unit.moveSpeed / 2f;
        }

        if (setRotation && agent != null)
        {
            agent.updateRotation = false;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null || weapon == null) return TaskStatus.Failure;

        RotateTowardsTarget();

        if (warmupTimer > 0f)
        {
            warmupTimer -= Time.deltaTime;
            return TaskStatus.Running;
        }

        if (maxDuration > 0f)
        {
            currentTimer += Time.deltaTime;
            if (currentTimer >= maxDuration)
            {
                return TaskStatus.Success;
            }
        }

        Vector3 myPos = transform.position;
        Vector3 targetPos = target.Value.position;
        myPos.y = 0;
        targetPos.y = 0;
        float distance = Vector3.Distance(myPos, targetPos);

        if (distance > weapon.GetMaxWeaponDistance()) return TaskStatus.Failure;

        weapon.FireMain(target.Value);
        weapon.FireSub(target.Value);

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        if (setRotation && agent != null)
        {
            agent.updateRotation = true;
        }
        if (slowDownWhileFiring && agent != null)
        {
            agent.speed = unit.moveSpeed;
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = target.Value.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}