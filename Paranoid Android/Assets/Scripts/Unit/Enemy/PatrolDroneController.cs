using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolDroneController : UnitController
{
    public int maxHp = 60;
    public int hp;

    public float activateRadius = 5f;
    public float detectRadius = 25f;
    public float combatRadius = 10f;
    public LayerMask playerLayer;
    public Transform currentTarget;
    public NavMeshAgent agent;
    public bool isAlert;
    public Vector3 lostPos;
    private BehaviorTree behaviorTree;

    private float scanTimer = 0f;
    private const float SCAN_INTERVAL = 0.2f;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 8f;
        playerLayer = 1 << LayerMask.NameToLayer("Player");
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        behaviorTree = GetComponent<BehaviorTree>();
        agent.speed = moveSpeed;
        agent.acceleration = 4f * moveSpeed;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 1f;

        Init();
    }

    public void Init()
    {
        hp = maxHp;
        agent.baseOffset = 0f;
        animator.SetBool("IsAlert", false);
        behaviorTree.SetVariableValue("IsAlert", false);
        isAlert = false;
        lostPos = transform.position;
    }

    private void Update()
    {
        Scan(isAlert);
        if (animator != null && agent.isActiveAndEnabled)
        {
            // 如果寻路组件正在移动，且速度大于一个极小值，则认为在移动
            bool moving = agent.remainingDistance > agent.stoppingDistance && agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool("IsMoving", moving);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetBool("IsAlert", !animator.GetBool("IsAlert"));
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            animator.SetBool("IsMoving", !animator.GetBool("IsMoving"));
        }
    }

    private void Scan(bool alert)
    {
        scanTimer += Time.deltaTime;
        if (scanTimer >= SCAN_INTERVAL)
            scanTimer = 0f;
        else
            return;
        float radius = alert ? detectRadius : activateRadius;
        Collider[] targets = Physics.OverlapSphere(transform.position, radius, playerLayer);
        if ((targets.Length > 0))
        {
            if (!alert && !animator.GetBool("IsAlert"))
                animator.SetBool("IsAlert", true);
            currentTarget = targets[0].transform;
            lostPos = currentTarget.position;
        }
        else
            currentTarget = null;
    }

    public void CompleteActivation()
    {
        isAlert = true;
        behaviorTree.SetVariableValue("IsAlert", true);
    }






    // =================================================================
    // 【实时可视化 Gizmos 绘制核心逻辑】
    // =================================================================
    private void OnDrawGizmos()
    {
        // 安全拦截：防止在非运行模式下因为没有获取组件而疯狂报 Null 异常
        if (!Application.isPlaying)
        {
            // 在编辑模式下，只绘制简单的初始默认半径，方便策划摆放关卡
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, activateRadius);
            return;
        }

        // -------------------------------------------------------------
        // 1. 绘制当前行动状态文字（实时悬浮在无人机头顶 2 米处）
        // -------------------------------------------------------------
#if UNITY_EDITOR
        string stateText = isAlert ? "<color=red>激活</color>" : "<color=green>休眠</color>";
        if (agent != null && agent.hasPath)
        {
            stateText += "\n<color=cyan>移动中</color>";
        }
        if (currentTarget != null)
        {
            stateText += $"\n目标: {currentTarget.name}";
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;
        style.richText = true; // 开启富文本颜色支持

        // 将文字世界坐标转换为 Scene 视窗的 2D 坐标绘制
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.0f, stateText, style);
#endif

        // -------------------------------------------------------------
        // 2. 绘制检测圈与射程（使用扁平的同心圆环，不晃眼）
        // -------------------------------------------------------------
#if UNITY_EDITOR
        // 如果处于警戒状态：绘制红色的 25米 大检测圈和 10米 战斗射程圈
        if (isAlert)
        {
            // 25米大检测圈（红色断续或细圈）
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.15f); // 带有半透明填充
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, detectRadius);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, detectRadius);

            // 10米战斗射程（橘红色虚线圈，代表攻击界限）
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, combatRadius);
        }
        else // 如果处于休眠模式：只绘制绿色的 5米 小激活圈
        {
            UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.15f); // 半透明绿
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, activateRadius);
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, activateRadius);
        }
#endif

        // -------------------------------------------------------------
        // 3. 绘制目标位置与物理连线
        // -------------------------------------------------------------
        // 如果发现了玩家：从无人机心部拉一条明艳的粉红红细线连接到玩家脚下
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireCube(currentTarget.position, Vector3.one * 0.5f); // 标记玩家所在方块
        }

        // -------------------------------------------------------------
        // 4. 绘制导航路径（实时连线正在寻路的那条多段路径）
        // -------------------------------------------------------------
        if (agent != null && agent.isActiveAndEnabled && agent.hasPath)
        {
            NavMeshPath path = agent.path;

            // 实时拿到 NavMesh 规划的所有拐角拐点（Corners）
            Vector3[] corners = path.corners;

            if (corners != null && corners.Length > 1)
            {
                Gizmos.color = Color.cyan; // 用青色代表导航路线
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    // 绘制一条粗壮的寻路路径导引线
                    Gizmos.DrawLine(corners[i], corners[i + 1]);
                    Gizmos.DrawSphere(corners[i + 1], 0.15f); // 在每个拐角处画一个小球点
                }
            }

            // 并在最终的巡逻目的地画一面隐形的“科技小旗帜（球体标记）”
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(agent.destination, 0.3f);
        }
    }
}
