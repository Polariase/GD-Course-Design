using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class Fire : Action
{
    public SharedTransform target;
    public bool isRepeating = false;//连续射击，射速翻倍
    public float warmup = 0.5f;
    public float rotationSpeed = 12f;


    // 可选的时间限制，单位为秒，设置为0或负数表示没有时间限制
    public float maxDuration = 0f;

    private VirtualWeapon weapon;
    private NavMeshAgent agent;
    private float currentTimer = 0f;
    private float warmupTimer;

    public override void OnAwake()
    {
        weapon = GetComponent<VirtualWeapon>();
    }

    public override void OnStart()
    {
        // 每次进入节点时重置计时器
        warmupTimer = warmup;
        currentTimer = 0f;

        if (agent != null)
        {
            agent.updateRotation = false; // 关闭寻路组件的自动旋转
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

        // 检查时间限制
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

        if (distance > weapon.distance) return TaskStatus.Failure;

        weapon.Fire(target.Value, isRepeating);

        return TaskStatus.Running;
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = target.Value.position - transform.position;
        direction.y = 0; // 保持在水平面上旋转

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}