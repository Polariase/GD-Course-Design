using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unit AI")]
public class Chase : Action
{
    public SharedTransform target;
    public SharedVector3 targetPosition;
    public float offset = 1f;
    private EnemyController unit;

    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.3f;

    public override void OnAwake()
    {
        unit = GetComponent<EnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null) return TaskStatus.Failure;
        Vector3 myPos = transform.position;
        Vector3 targetPos = target.Value.position;
        myPos.y = 0;
        targetPos.y = 0;
        float distance = Vector3.Distance(myPos, targetPos);

        if (distance <= unit.combatRadius - offset)
        {
            unit.agent.ResetPath();
            return TaskStatus.Success;
        }

        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;
            if (unit.agent.isActiveAndEnabled)
            {
                targetPosition.Value = targetPos;
                unit.agent.SetDestination(targetPosition.Value);
            }
        }

        return TaskStatus.Running;
    }
}