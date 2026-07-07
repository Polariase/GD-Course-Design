using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("Unit AI")]
public class TacticMoveLand : Action
{
    public SharedTransform target;
    public SharedVector3 targetPosition;

    private float minRange;
    private float maxRange;
    private NavMeshAgent agent;
    private EnemyController enemy;

    // 调试可视化开关
    private readonly bool showDebugGizmos = true;

    public override void OnAwake()
    {
        enemy = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();

        // 设定地面的舒适交战区：在战斗半径的 40% ~ 85% 之间
        minRange = 0.4f * enemy.combatRadius;
        maxRange = 0.85f * enemy.combatRadius;
    }

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null || agent == null || !agent.isActiveAndEnabled)
            return TaskStatus.Failure;

        // ─── 1. 路径状态监控 ───
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

        // ─── 2. 核心：高级走位多点空间采样与评分 ───
        Vector3 bestPoint = Vector3.zero;
        float highestScore = float.MinValue;
        bool foundValidPoint = false;

        Vector3 targetPos = target.Value.position;
        Vector3 selfPos = transform.position;
        float idealDistance = (minRange + maxRange) * 0.5f;

        // 地面单位增加采样点数量（从4提高到8），确保能从复杂地形中筛选出最佳出路
        int sampleCount = 6;
        for (int i = 0; i < sampleCount; i++)
        {
            // 改进：采用基于自身当前位置微调的环形采样，配合围绕玩家的侧向扇形
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minRange, maxRange);
            Vector3 candidatePos = targetPos + new Vector3(randomCircle.x, 0, randomCircle.y);

            // 在地面上采样，检测是否在可移动的 NavMesh 上
            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 3.0f, agent.areaMask))
            {
                float score = 100f; // 初始满分

                float distToTarget = Vector3.Distance(hit.position, targetPos);
                float distToSelf = Vector3.Distance(hit.position, selfPos);

                //  惩罚项一：距离惩罚（确保不脱离，也不过分贴脸）
                score -= Mathf.Abs(distToTarget - idealDistance) * 20f;

                // 惩罚项二：原地蠕动惩罚（如果新点离自己太近，属于无效走位）
                if (distToSelf < 2.5f)
                {
                    score -= 60f;
                }

                // 惩罚项三：死角与墙壁规避（核心升级点）
                // 寻找该候选点最近的 NavMesh 边界边缘
                if (NavMesh.FindClosestEdge(hit.position, out NavMeshHit edgeHit, agent.areaMask))
                {
                    // 如果该点距离墙壁/死角边缘小于 1.8 米，说明空间极度狭窄
                    if (edgeHit.distance < 1.2f)
                    {
                        // 距离墙壁越近，扣分越狠（反比指数级严惩）
                        score -= (2f - edgeHit.distance) * 60f;
                    }
                }

                // ─── 3. 现场可视化调试绘制（调试完可一键无视） ───
                if (showDebugGizmos)
                {
                    // 越接近绿色的点分数越高，越红的点分数越低
                    Color debugColor = score > 40 ? Color.green : (score > 0 ? Color.yellow : Color.red);
                    Debug.DrawLine(hit.position, hit.position + Vector3.up * 0.5f, debugColor, 0.4f);
                }

                // ─── 4. 筛选最高得分 ───
                if (score > highestScore)
                {
                    highestScore = score;
                    bestPoint = hit.position;
                    foundValidPoint = true;
                }
            }
        }

        // ─── 5. 执行走位决策 ───
        if (foundValidPoint)
        {
            targetPosition.Value = bestPoint;
            agent.SetDestination(targetPosition.Value);

            if (showDebugGizmos)
            {
                // 最终选中的黄金走位点，在 Scene 窗口中画个蓝十字标记
                Debug.DrawRay(bestPoint, Vector3.up * 1.5f, Color.cyan, 1f);
            }

            return TaskStatus.Running;
        }

        return TaskStatus.Failure;
    }
}