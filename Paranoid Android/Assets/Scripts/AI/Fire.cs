using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unit AI")]
public class Fire : Action
{
    public SharedTransform target;
    public bool isRepeating = false;//连续射击，射速翻倍

    // 可选的时间限制，单位为秒，设置为0或负数表示没有时间限制
    public float maxDuration = 0f;

    private VirtualWeapon weapon;
    private float currentTimer = 0f;

    public override void OnAwake()
    {
        weapon = GetComponent<VirtualWeapon>();
    }

    public override void OnStart()
    {
        // 每次进入节点时重置计时器
        currentTimer = 0f;
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null || weapon == null) return TaskStatus.Failure;

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
        myPos.y = 0; targetPos.y = 0;
        float distance = Vector3.Distance(myPos, targetPos);

        if (distance > weapon.distance) return TaskStatus.Failure;

        weapon.Fire(target.Value, isRepeating);

        return TaskStatus.Running;
    }
}