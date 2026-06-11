using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class Patrol : Action
{
    public SharedVector3 lostPos;
    public SharedVector3 targetPosition;
    public float patrolRadius = 5f;

    private NavMeshAgent agent;

    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null || !agent.isActiveAndEnabled) return TaskStatus.Failure;

        if (agent.hasPath)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                agent.ResetPath();
                return TaskStatus.Failure;
            }

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.ResetPath();
                return TaskStatus.Success;
            }
        }

        if (agent.hasPath || agent.pathPending)
        {
            return TaskStatus.Running;
        }

        Vector3 randomSphere = Random.insideUnitSphere * patrolRadius;
        Vector3 candidatePos = lostPos.Value + new Vector3(randomSphere.x, 0, randomSphere.z);

        if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            targetPosition.Value = hit.position;
            agent.SetDestination(targetPosition.Value);

            return TaskStatus.Running;
        }

        return TaskStatus.Failure;
    }
}