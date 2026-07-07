using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class CanRollAttack : Conditional
{
    private RobotSphereController _controller;

    public override void OnAwake()
    {
        _controller = GetComponent<RobotSphereController>();
    }

    public override TaskStatus OnUpdate()
    {
        if (_controller == null || _controller.currentTarget == null)
            return TaskStatus.Failure;

        Vector3 startPos = transform.position;
        Vector3 targetPos = _controller.currentTarget.position;

        int layerMask = LayerMask.GetMask("Player", "Default");
        Vector3 rayDirection = (targetPos - startPos).normalized;
        float distanceToPlayer = Vector3.Distance(startPos, targetPos);

        if (Physics.Raycast(_controller.HitPoint(), rayDirection, out RaycastHit hit, distanceToPlayer + 1, layerMask))
        {
            if (!hit.transform.CompareTag("Player"))
            {
                // 有物理障碍遮挡
                return TaskStatus.Failure;
            }
        }

        if (NavMesh.Raycast(startPos, targetPos, out NavMeshHit _, _controller.agent.areaMask))
        {
            // 寻路网格不连通
            return TaskStatus.Failure;
        }

        return TaskStatus.Success;
    }
}