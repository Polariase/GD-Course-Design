using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unit AI")]
public class CanSeeTarge : Conditional
{
    public SharedTransform target;
    private EnemyController unit;

    public override void OnAwake()
    {
        unit = GetComponent<EnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (unit != null && unit.Ready && unit.currentTarget != null)
        {
            target.Value = unit.currentTarget;
            return TaskStatus.Success;
        }
        target.Value = null;
        return TaskStatus.Failure;
    }
}