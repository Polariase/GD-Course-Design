using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unit AI")]
public class CanSeeTarge : Conditional
{
    public SharedTransform target;
    public SharedVector3 lostPos;
    private PatrolDroneController drone;

    public override void OnAwake()
    {
        drone = GetComponent<PatrolDroneController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (drone != null && drone.isAlert && drone.currentTarget != null)
        {
            target.Value = drone.currentTarget;
            return TaskStatus.Success;
        }
        target.Value = null;
        lostPos = drone.lostPos;
        return TaskStatus.Failure;
    }
}