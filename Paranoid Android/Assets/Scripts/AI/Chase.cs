using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unit AI")]
public class Chase : Action
{
    public SharedTransform target;
    public SharedVector3 targetPosition;
    public float offset = 1f;
    private PatrolDroneController drone;

    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.3f;

    public override void OnAwake()
    {
        drone = GetComponent<PatrolDroneController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null) return TaskStatus.Failure;
        Vector3 myPos = transform.position;
        Vector3 targetPos = target.Value.position;
        myPos.y = 0;
        targetPos.y = 0;
        float distance = Vector3.Distance(myPos, targetPos);

        if (distance <= drone.combatRadius - offset)
        {
            drone.agent.ResetPath();
            return TaskStatus.Success;
        }

        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;
            if (drone.agent.isActiveAndEnabled)
            {
                targetPosition.Value = targetPos;
                drone.agent.SetDestination(targetPosition.Value);
            }
        }

        return TaskStatus.Running;
    }
}