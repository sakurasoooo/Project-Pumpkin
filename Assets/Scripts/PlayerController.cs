using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Celeste 级手感的跳跃控制器
/// 
/// 核心机制参考自 Celeste Player.cs：
/// 1. Jump Grace Time (土狼时间)  — 离开平台后仍允许短暂跳跃
/// 2. Variable Jump (可变跳跃高度) — 按住跳跃键跳更高，松手立即下降
/// 3. Apex Half Gravity (顶点半重力) — 跳跃最高点附近重力减半，增加滞空操控感
/// 4. Jump Buffering (跳跃输入缓冲) — 落地前一小段时间按跳跃，落地瞬间自动起跳
/// 5. Squash & Stretch (挤压拉伸)  — 起跳时竖拉、落地时横压，增加弹性活力感
/// 6. Auto Hop (自动蹦跳)         — 落地后按一定频率自动做小幅弹跳，还原倭瓜蹦跶的感觉
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    // =====================================================
    //  来自 Celeste 的"魔法数字" (Celeste-inspired Constants)
    // =====================================================

    [Header("跳跃力度")]
    [Tooltip("玩家点击时的跳跃速度 (对应 Celeste JumpSpeed = -105)")]
    public float jumpSpeed = 14f;
    [Tooltip("自动蹦跳的力度")]
    public float autoSmallJumpForce = 5f;
    [Tooltip("最大连跳次数 (2 = 二连跳)")]
    public int maxJumps = 2;

    [Header("土狼时间与输入缓冲 (Celeste核心手感)")]
    [Tooltip("离地后仍可跳跃的宽限时间 (Celeste = 0.1s)")]
    public float jumpGraceTime = 0.1f;
    [Tooltip("可变跳跃高度的持续时间 (Celeste = 0.2s)：按住跳跃键期间持续获得向上的力")]
    public float varJumpTime = 0.2f;
    [Tooltip("跳跃输入缓冲时间：落地前这段时间内按跳跃，落地后自动触发")]
    public float jumpBufferTime = 0.1f;

    [Header("重力微调")]
    [Tooltip("半重力阈值速度 (Celeste = 40)：当上升速度小于此值且按住跳跃时，重力减半")]
    public float halfGravThreshold = 4f;

    [Header("地面检测")]
    public Transform groundCheck;
    [Tooltip("向下射线检测长度")]
    public float groundRayLength = 0.5f;
    public LayerMask groundLayer;

    [Header("自动蹦跳")]
    public float autoJumpInterval = 0.3f;

    // =====================================================
    //  Private State
    // =====================================================

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 地面状态
    private bool isGrounded = false;
    private bool wasGrounded = false;

    // 跳跃计数
    private int currentJumpCount = 0;

    // 土狼时间计时器 (Celeste: jumpGraceTimer)
    private float jumpGraceTimer = 0f;

    // 可变跳跃计时器 (Celeste: varJumpTimer)
    private float varJumpTimer = 0f;
    private float varJumpSpeed = 0f;
    private bool holdingJump = false;

    // 跳跃输入缓冲
    private float jumpBufferTimer = 0f;

    // 自动小跳计时器
    private float autoJumpTimer = 0f;

    // Squash & Stretch
    private Vector3 originalScale;
    private Vector3 targetScale;
    private float scaleReturnSpeed = 8f;

    // 碰撞式地面检测（作为射线的后备）
    private int groundContactCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        wasGrounded = isGrounded;

        CheckGrounded();
        UpdateTimers();
        HandleJumpInput();
        ApplyVariableJump();
        ApplyApexGravity();
        HandleAutoJump();
        UpdateSquashStretch();

        // ===== 运行时射线可视化 =====
        if (groundCheck != null)
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * groundRayLength,
                isGrounded ? Color.green : Color.red);
        }
    }

    // =====================================================
    //  地面检测（双重保险：射线 + 碰撞回调）
    // =====================================================
    private void CheckGrounded()
    {
        bool rayGrounded = false;

        // 方法1：射线检测
        if (groundCheck != null)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                groundCheck.position, Vector2.down, groundRayLength, groundLayer);

            foreach (var h in hits)
            {
                if (h.collider != null && !h.collider.transform.IsChildOf(transform))
                {
                    rayGrounded = true;
                    break;
                }
            }
        }

        // 方法2：碰撞回调（后备）
        bool collisionGrounded = groundContactCount > 0;

        // 任一方法检测到地面即为着地
        isGrounded = rayGrounded || collisionGrounded;

        // 刚落地瞬间
        if (!wasGrounded && isGrounded)
        {
            OnLanded();
        }
    }

    // =====================================================
    //  碰撞式地面检测回调（后备方案，绝对可靠）
    // =====================================================
    private void OnCollisionEnter2D(Collision2D col)
    {
        // 只计算来自下方的碰撞（法线朝上）
        foreach (ContactPoint2D contact in col.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundContactCount++;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        groundContactCount = Mathf.Max(0, groundContactCount - 1);
    }

    // =====================================================
    //  计时器更新
    // =====================================================
    private void UpdateTimers()
    {
        // 土狼时间：站在地上时持续刷新
        if (isGrounded)
        {
            jumpGraceTimer = jumpGraceTime;
            currentJumpCount = 0; // 持续重置，而不是只在 landing 瞬间
        }
        else
        {
            if (jumpGraceTimer > 0)
                jumpGraceTimer -= Time.deltaTime;
        }

        // 可变跳跃计时器
        if (varJumpTimer > 0)
            varJumpTimer -= Time.deltaTime;

        // 跳跃输入缓冲计时器
        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }

    // =====================================================
    //  跳跃输入处理
    // =====================================================
    private void HandleJumpInput()
    {
        bool jumpPressed = false;
        holdingJump = false;

#if ENABLE_INPUT_SYSTEM
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            jumpPressed = true;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
        // 检测是否持续按住
        if ((Pointer.current != null && Pointer.current.press.isPressed) ||
            (Keyboard.current != null && Keyboard.current.spaceKey.isPressed))
            holdingJump = true;
#else
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            holdingJump = true;
#endif

        // 按下跳跃键：写入缓冲
        if (jumpPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }

        // 尝试消费跳跃缓冲
        if (jumpBufferTimer > 0)
        {
            // 情况1：在地上，或在土狼时间内（算作第一次跳）
            if (jumpGraceTimer > 0 && currentJumpCount == 0)
            {
                PerformJump();
                jumpBufferTimer = 0;
            }
            // 情况2：空中二连跳（已经跳过一次，还有次数）
            else if (currentJumpCount > 0 && currentJumpCount < maxJumps)
            {
                PerformJump();
                jumpBufferTimer = 0;
            }
        }
    }

    // =====================================================
    //  执行跳跃
    // =====================================================
    private void PerformJump()
    {
        currentJumpCount++;

        // 清零土狼时间（Celeste: jumpGraceTimer = 0）
        jumpGraceTimer = 0;

        // 覆盖垂直速度，确保每次跳跃高度一致
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);

        // 启动可变跳跃计时器
        varJumpTimer = varJumpTime;
        varJumpSpeed = jumpSpeed;

        // 标记离地
        isGrounded = false;
        autoJumpTimer = 0f;

        // Squash & Stretch：起跳时竖向拉伸 (Celeste: Scale = 0.6, 1.4)
        ApplyScale(0.7f, 1.3f);

        // TODO: 结合 Feel 插件触发屏幕缩放/挤压效果
        // TODO: 触发跳跃扬尘特效
    }

    // =====================================================
    //  可变跳跃高度 (Celeste: Variable Jumping)
    //  按住跳跃键期间，保持向上速度不低于 varJumpSpeed
    //  松手则立刻结束上升
    // =====================================================
    private void ApplyVariableJump()
    {
        if (varJumpTimer > 0)
        {
            if (holdingJump)
            {
                // 保持最小上升速度
                if (rb.linearVelocity.y < varJumpSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, varJumpSpeed);
                }
            }
            else
            {
                // 松手：立刻结束可变跳跃
                varJumpTimer = 0;
            }
        }
    }

    // =====================================================
    //  顶点半重力 (Celeste: Half Gravity at Apex)
    //  当上升速度很小（接近跳跃最高点）且按住跳跃时，
    //  暂时将重力减半，让玩家有更多时间做出操作。
    // =====================================================
    private void ApplyApexGravity()
    {
        if (!isGrounded && Mathf.Abs(rb.linearVelocity.y) < halfGravThreshold && holdingJump)
        {
            // 用施加反向力模拟半重力
            float counterForce = Physics2D.gravity.y * rb.gravityScale * 0.5f;
            rb.AddForce(Vector2.up * counterForce * rb.mass, ForceMode2D.Force);
        }
    }

    // =====================================================
    //  自动蹦跳 (Auto Hop) — 倭瓜持续小跳
    // =====================================================
    private void HandleAutoJump()
    {
        if (isGrounded)
        {
            autoJumpTimer += Time.deltaTime;
            if (autoJumpTimer >= autoJumpInterval)
            {
                autoJumpTimer = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, autoSmallJumpForce);
                isGrounded = false;
            }
        }
    }

    // =====================================================
    //  落地回调
    // =====================================================
    private void OnLanded()
    {
        currentJumpCount = 0;
        varJumpTimer = 0;

        // Squash & Stretch：落地时横向挤压 (Celeste: Scale lerp based on fall speed)
        float fallSpeed = Mathf.Abs(rb.linearVelocity.y);
        float squish = Mathf.Clamp01(fallSpeed / 20f);
        float scaleX = Mathf.Lerp(1f, 1.4f, squish);
        float scaleY = Mathf.Lerp(1f, 0.6f, squish);
        ApplyScale(scaleX, scaleY);

        // TODO: 播放落地音效
        // TODO: 生成落地扬尘粒子
    }

    // =====================================================
    //  Squash & Stretch 视觉系统
    // =====================================================
    private void ApplyScale(float xMult, float yMult)
    {
        targetScale = new Vector3(
            originalScale.x * xMult,
            originalScale.y * yMult,
            originalScale.z);
        transform.localScale = targetScale;
    }

    private void UpdateSquashStretch()
    {
        // 平滑回弹到原始比例
        transform.localScale = Vector3.Lerp(
            transform.localScale, originalScale,
            scaleReturnSpeed * Time.deltaTime);
    }

    // =====================================================
    //  Gizmos 调试（编辑器中也可见）
    // =====================================================
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            float len = groundRayLength;
            Gizmos.color = Application.isPlaying && isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * len);
            // 画一个小球标记射线终点
            Gizmos.DrawWireSphere(groundCheck.position + Vector3.down * len, 0.05f);
        }
    }
}

