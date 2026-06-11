using UnityEngine;
using UnityEngine.AI;

public class DroneStartAction : StateMachineBehaviour
{
    public float startOffset = 0f;
    public float targetOffset = 6f;

    // 在面板上开放一个曲线，默认给它一个从 0 到 1 的线性初始值
    [SerializeField]
    private AnimationCurve easeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private NavMeshAgent agent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null) agent = animator.GetComponent<NavMeshAgent>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null) return;

        // 1. 获取动画的绝对进度（0.0 到 1.0）
        float progress = Mathf.Clamp01(stateInfo.normalizedTime);

        // 2. 将线性进度传入你的曲线，由曲线计算出“缓出”后的新进度
        float easedProgress = easeCurve.Evaluate(progress);

        // 3. 用缓后进度进行插值
        agent.baseOffset = Mathf.Lerp(startOffset, targetOffset, easedProgress);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null) agent.baseOffset = targetOffset;
        PatrolDroneController drone = animator.GetComponent<PatrolDroneController>();
        if (drone != null)
        {
            // 正式标记无人机激活成功
            drone.CompleteActivation();
        }
    }
}