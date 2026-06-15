using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class TacticMoveAir : Action
{
    public SharedTransform target;
    public SharedVector3 targetPosition;
    public float minRange;
    public float maxRange;
    public NavMeshAgent agent;
    private EnemyController drone;

    public override void OnAwake()
    {
        drone = GetComponent<EnemyController>();
        minRange = 0.3f * drone.combatRadius;
        maxRange = 0.7f * drone.combatRadius;
        agent = GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null) return TaskStatus.Failure;

        if (agent.hasPath)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                agent.ResetPath();
                return TaskStatus.Failure;
            }

            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.ResetPath();
                return TaskStatus.Success;
            }
        }

        if (agent.hasPath || agent.pathPending)
        {
            return TaskStatus.Running;
        }

        Vector3 bestPoint = Vector3.zero;
        float highestScore = float.MinValue;
        bool foundValidPoint = false;

        Vector3 targetPos = target.Value.position;
        Vector3 selfPos = transform.position;

        float idealDistance = (minRange + maxRange) * 0.5f;

        for (int i = 0; i < 4; i++)
        {
            // 在环状舒适区内生成随机极坐标点
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minRange, maxRange);
            Vector3 candidatePos = targetPos + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                // 初始化基础分
                float score = 100f;

                float distToTarget = Vector3.Distance(hit.position, targetPos);
                float distToSelf = Vector3.Distance(hit.position, selfPos);

                // 离理想距离每偏差 1 米，扣除 15 分
                score -= Mathf.Abs(distToTarget - idealDistance) * 15f;

                // 如果该点离自己太近，说明收益极低
                if (distToSelf < 2.0f)
                {
                    score -= 50f; // 予以重罚
                }

                // 筛选出最高分候选点
                if (score > highestScore)
                {
                    highestScore = score;
                    bestPoint = hit.position;
                    foundValidPoint = true;
                }
            }
        }

        if (foundValidPoint)
        {
            targetPosition.Value = bestPoint;
            agent.SetDestination(targetPosition.Value);
            return TaskStatus.Running;
        }
        return TaskStatus.Failure;
    }
}