using Cinemachine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 6f;
    public float dashSpeedMult = 3.5f;
    public float aimSpeedMult = 0.6f;
    public float fireSpeedMult = 0.5f;
    public float dashLoadCost = 20f;

    [Header("平滑设置")]
    public float minSmoothTime = 0.02f;
    public float maxSmoothTime = 0.15f;
    public float rotationSpeed = 15f;

    [Header("引用")]
    public Animator animator;
    public CinemachineVirtualCamera vc;
    public LayerMask groundLayer;
    [SerializeField] private PlayerStateData stateData;

    public Vector3 mouseWorldPosition;

    //复合状态判断
    public bool CanFire => !isDashing && isArmed && !stateData.overloaded;

    private CinemachineFramingTransposer transposer;
    private Vector3 _lookDir;
    private Rigidbody _rb;
    private GameInput _input;
    private Vector2 _moveInput;
    private Vector3 _dashDirection;

    //单位状态
    public bool isFiring;
    public bool isAiming;
    public bool isScouting;
    public bool isDashing;
    public bool isArmed;
    public bool isInvincible;
    public bool isMoving;

    readonly private float _dashDuration = 0.355f;
    private PlayerVisual _visual;
    private float _hVelocity, _vVelocity, _rVelocity;  // 用于平滑记录的临时变量

    void Awake()
    {
        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _input = new GameInput();
        transposer = vc.GetCinemachineComponent<CinemachineFramingTransposer>();

        _input.Player.Aim.performed += ctx => isAiming = true;
        _input.Player.Aim.canceled += ctx => isAiming = false;

        _input.Player.Scout.performed += ctx => isScouting = !isScouting;

        _input.Player.Dash.performed += ctx => TryDash();

        stateData.OnSlotChanged += OnSwitchSlot;

        _input.Player.SwitchSlot.performed += ctx =>
        {
            int slot = Mathf.RoundToInt(ctx.ReadValue<float>());
            stateData.SelectSlot(slot);
        };

        _input.Player.Fire.performed += ctx =>
        {
            if (isArmed)
            {
                isFiring = true;
            }
        };
        _input.Player.Fire.canceled += ctx => isFiring = false;

        _visual = GetComponent<PlayerVisual>();
    }

    void OnEnable()
    {
        _input.Player.Enable();
    }

    void OnDisable()
    {
        _input.Player.Disable();
    }

    void Update()
    {
        _moveInput = _input.Player.Move.ReadValue<Vector2>();

        UpdateLookDirection();

        stateData.Cooling(Time.deltaTime);

        HandleRotation();

        HandleCameraOffset();

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void OnSwitchSlot(int slot)
    {
        if (slot > 0)
            isArmed = true;
        else
        {
            isArmed = false;
            isFiring = false;
        }
    }

    float SpeedScale()
    {
        float mult = 1f;
        if (isAiming)
            mult *= aimSpeedMult;
        if (isFiring && CanFire)
            mult *= fireSpeedMult;
        return mult;
    }

    void TryDash()
    {
        if (!isDashing && !stateData.overloaded)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        isInvincible = true;

        // 锁定方向
        Vector3 inputDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        // 如果按下时有移动输入，冲向移动方向；否则冲向角色面对的方向
        _dashDirection = inputDir != Vector3.zero ? inputDir : transform.forward;

        // 瞬间转向冲刺方向
        transform.rotation = Quaternion.LookRotation(_dashDirection);

        _visual.SetElectric(1f);

        stateData.Overload(dashLoadCost);

        animator.SetTrigger("Dash");

        yield return new WaitForSeconds(_dashDuration);

        _visual.SetElectric(0f);

        isDashing = false;
        isInvincible = false;
    }

    void UpdateLookDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100, groundLayer))
        {
            mouseWorldPosition = hit.point;
            mouseWorldPosition.y = transform.position.y;
        }
        _lookDir = (mouseWorldPosition - transform.position).normalized;
    }

    void HandleCameraOffset()
    {
        if (transposer == null) return;

        if (isScouting)
        {
            // 计算从玩家指向鼠标的世界空间向量
            Vector3 worldOffset = mouseWorldPosition - transform.position;

            // 将这个世界向量缩短到 1/5
            worldOffset /= 5f;

            Vector3 localOffset = transform.InverseTransformDirection(worldOffset);

            // 应用偏移
            transposer.m_TrackedObjectOffset = localOffset;
        }
        else
        {
            transposer.m_TrackedObjectOffset = Vector3.zero;
        }
    }

    void UnitMove(Vector3 dir, float mult)
    {
        _rb.MovePosition(_rb.position + (moveSpeed * mult) * Time.fixedDeltaTime * dir);
    }

    void HandleMovement()
    {
        if (isDashing)
        {
            UnitMove(_dashDirection, dashSpeedMult);
            return;
        }

        Vector3 inputDir = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        if (inputDir == Vector3.zero) return;

        UnitMove(inputDir, SpeedScale());
    }

    void HandleRotation()
    {
        if (isDashing) return;

        // 执行旋转
        if (_lookDir != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(_lookDir.x, _lookDir.z) * Mathf.Rad2Deg;

            // 角度平滑阻尼
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref _rVelocity,
                0.1f
            );

            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }

    void UpdateAnimation()
    {
        isMoving = _moveInput.magnitude > 0.01f && !isDashing;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            Vector3 movementVector = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            Vector3 localVelocity = transform.InverseTransformDirection(movementVector);
            float speedMultiplier = SpeedScale();

            float targetH = localVelocity.x * speedMultiplier;
            float targetV = localVelocity.z * speedMultiplier;

            // 1. 计算差异的绝对值 (范围通常在 0 到 2 之间，因为是从 -1 到 1)
            float deltaH = Mathf.Abs(targetH - animator.GetFloat("Horizontal"));
            float deltaV = Mathf.Abs(targetV - animator.GetFloat("Vertical"));

            // 2. 将差异映射到平滑时间 (这里假设差异达到 1 时达到最大平滑时间)
            // t 的值越大，SmoothDamp 越慢
            float dynamicSmoothH = Mathf.Lerp(minSmoothTime, maxSmoothTime, deltaH);
            float dynamicSmoothV = Mathf.Lerp(minSmoothTime, maxSmoothTime, deltaV);

            // 3. 应用动态平滑时间
            float currentH = Mathf.SmoothDamp(animator.GetFloat("Horizontal"), targetH, ref _hVelocity, dynamicSmoothH);
            float currentV = Mathf.SmoothDamp(animator.GetFloat("Vertical"), targetV, ref _vVelocity, dynamicSmoothV);

            animator.SetFloat("Horizontal", currentH);
            animator.SetFloat("Vertical", currentV);
            animator.SetFloat("TurnSpeed", 0f);
        }
        else
        {
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", 0f);

            // 计算转身逻辑
            if (_lookDir != Vector3.zero)
            {
                bool isActuallyRotating = Mathf.Abs(_rVelocity) > 20f;
                float targetTurn = isActuallyRotating ? Mathf.Sign(_rVelocity) : 0f;
                float currentTurn = Mathf.MoveTowards(
                    animator.GetFloat("TurnSpeed"),
                    targetTurn,
                    Time.deltaTime * 2f
                );
                animator.SetFloat("TurnSpeed", currentTurn);
            }
        }

        animator.SetBool("IsArmed", isArmed);
        animator.SetBool("IsAiming", isAiming);
        if (CanFire)
            animator.SetBool("IsFiring", isFiring);
        else
            animator.SetBool("IsFiring", false);
    }
}
