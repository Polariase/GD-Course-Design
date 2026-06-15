using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unit AI")]
public class NeedToChase : Conditional
{
    public SharedTransform target;
    private EnemyController unit;

    public override void OnAwake()
    {
        unit = GetComponent<EnemyController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (unit != null && target.Value!=null)
        {
            Vector3 myPos = transform.position;
            Vector3 targetPos = target.Value.position;
            myPos.y = 0;
            targetPos.y = 0;
            float distance = Vector3.Distance(myPos, targetPos);

            if (distance > unit.combatRadius)
            {
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failure;
    }
}