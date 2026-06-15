using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class RollingAttack : Action
{
    private RobotSphereController controller;

    public override void OnStart()
    {
        controller = GetComponent<RobotSphereController>();
        if (controller != null)
        {
            controller.StartRollAttack();
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (controller == null) return TaskStatus.Failure;

        controller.CheckAndExecuteRolling();

        if (controller.isRolling)
        {
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }

    public override void OnEnd()
    {
        controller.StopRollingPhase();
    }
}