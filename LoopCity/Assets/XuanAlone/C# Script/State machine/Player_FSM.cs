using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

// 主角黑板数据
[Serializable]
public class PlayerBlackboard : Blackboard
{
    [Header("角色属性")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float attackDamage = 20f;
    public float attackCooldown = 0.5f;

    [Header("引用")]
    public Transform transform;
    public Rigidbody2D rb;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("运行时数据")]
    public float currentHealth;
    public Vector2 moveDirection;
    public Vector2 aimDirection;
    public float lastAttackTime;

    [Header("动画控制")]
    public Animator animator;
    public float lastHorizontal = 1f; // 默认朝右
    [Header("方向控制")]
    public float lastValidHorizontal = 1f; // 默认朝右
    public bool facingRight = true; // 当前朝向

    // 新增方法：更新瞄准方向
    public void UpdateAimDirection()
    {
        // 获取鼠标位置并转换为世界坐标
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // 计算朝向鼠标的方向
        aimDirection = (mousePosition - transform.position).normalized;
    }
}

// 待机状态
public class Player_IdleState : IState
{
    private FSM fsm;
    public PlayerBlackboard blackboard;

    public Player_IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // 使用最后有效方向
        if (blackboard.animator)
        {
            blackboard.animator.SetFloat("Horizontal", blackboard.lastValidHorizontal);
            blackboard.animator.SetFloat("Vertical", 0f);
            blackboard.animator.SetFloat("Speed", 0f);
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 获取输入
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 更新瞄准方向（关键修复：在Idle状态也要更新）
        blackboard.UpdateAimDirection();

        // 如果有移动输入，切换到移动状态
        if (blackboard.moveDirection.magnitude > 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Move);
        }

        // 处理攻击输入
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        // 鼠标左键攻击
        if (Input.GetMouseButton(0))
        {
            // 检查攻击冷却
            if (Time.time - blackboard.lastAttackTime > blackboard.attackCooldown)
            {
                fsm.SwitchState(MY_FSM.StateType.Attack);
            }
        }
    }
}

// 移动状态
public class Player_MoveState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;

    public Player_MoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter() { }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 方向控制 ================================
        if (blackboard.moveDirection.x != 0)
        {
            // 更新水平方向
            blackboard.facingRight = blackboard.moveDirection.x > 0;
            blackboard.lastValidHorizontal = blackboard.facingRight ? 1f : -1f;
        }

        // 设置动画参数
        if (blackboard.animator)
        {
            // 上下移动时使用最后有效水平方向
            float animHorizontal = blackboard.moveDirection.x != 0 ?
                blackboard.moveDirection.x :
                blackboard.lastValidHorizontal;

            blackboard.animator.SetFloat("Horizontal", animHorizontal);
            blackboard.animator.SetFloat("Vertical", blackboard.moveDirection.y);
            blackboard.animator.SetFloat("Speed", blackboard.moveDirection.magnitude);
        }
        // 获取输入
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // 更新瞄准方向
        blackboard.UpdateAimDirection();

        // 应用移动
        blackboard.rb.velocity = blackboard.moveDirection * blackboard.moveSpeed;

        // 如果没有移动输入，切换到待机状态
        if (blackboard.moveDirection.magnitude < 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
        }

        // 处理攻击输入
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (Time.time - blackboard.lastAttackTime > blackboard.attackCooldown)
            {
                fsm.SwitchState(MY_FSM.StateType.Attack);
            }
        }
    }
}

// 攻击状态
public class Player_AttackState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;

    public Player_AttackState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // 使用实时方向（鼠标瞄准方向）
        if (blackboard.animator)
        {
            // 计算水平方向：如果鼠标在右侧则为1，左侧则为-1
            float horizontal = blackboard.aimDirection.x > 0 ? 1f : -1f;
            blackboard.animator.SetFloat("Horizontal", horizontal);
            blackboard.animator.SetTrigger("Attack");
        }

        // 发射子弹
        FireBullet();

        // 记录攻击时间
        blackboard.lastAttackTime = Time.time;

        // 立即返回到上一个状态
        if (fsm.prevState != null)
        {
            fsm.SwitchState(fsm.prevStateType);
        }
        else
        {
            // 默认返回待机状态
            fsm.SwitchState(MY_FSM.StateType.Idle);
        }
    }

    public void OnExit() { }

    public void OnUpdate() { }

    private void FireBullet()
    {
        if (blackboard.bulletPrefab && blackboard.firePoint)
        {
            GameObject bullet = GameObject.Instantiate(
                blackboard.bulletPrefab,
                blackboard.firePoint.position,
                Quaternion.identity
            );

            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController)
            {
                // 确保在实例化后立即设置方向
                bulletController.direction = blackboard.aimDirection;
                bulletController.damage = blackboard.attackDamage;
                bulletController.speed = 10f; // 确保设置速度值
                bulletController.isPlayerBullet = true;

                // 立即应用方向（可选）
                if (bulletController.direction != Vector2.zero)
                {
                    float angle = Mathf.Atan2(
                        bulletController.direction.y,
                        bulletController.direction.x
                    ) * Mathf.Rad2Deg;

                    bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
        }
    }
}

// 死亡状态
public class Player_DieState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;

    public Player_DieState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // 死亡处理 - 停止移动，禁用控制等
        blackboard.rb.velocity = Vector2.zero;
        Debug.Log("Player died!");

        // 这里可以添加死亡动画、游戏结束逻辑等
    }

    public void OnExit() { }

    public void OnUpdate() { }
}


// 主角状态机控制器
public class Player_FSM : MonoBehaviour
{
    public PlayerBlackboard blackboard;
    private FSM fsm;

    [Header("子弹设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Start()
    {
        blackboard.animator = GetComponent<Animator>();

        // 初始化黑板
        if (blackboard == null) blackboard = new PlayerBlackboard();

        // 设置引用
        blackboard.transform = transform;
        blackboard.rb = GetComponent<Rigidbody2D>();
        blackboard.firePoint = firePoint;
        blackboard.bulletPrefab = bulletPrefab;
        blackboard.currentHealth = blackboard.maxHealth;

        // 初始化状态机
        fsm = new FSM(blackboard);
        // 关键修改：设置碰撞层
        gameObject.layer = LayerMask.NameToLayer("Player");

        // 添加状态
        fsm.AddState(MY_FSM.StateType.Idle, new Player_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Player_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Attack, new Player_AttackState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Player_DieState(fsm));

        // 初始状态
        fsm.SwitchState(MY_FSM.StateType.Idle);
    }

    void Update()
    {
        fsm.OnUpdate();
        UpdateHealth();

        // 调试：显示当前状态
        if (fsm.curState is Player_IdleState) Debug.Log("State: Idle");
        else if (fsm.curState is Player_MoveState) Debug.Log("State: Move");
        else if (fsm.curState is Player_AttackState) Debug.Log("State: Attack");
    }

    void FixedUpdate()
    {
        // 物理更新
    }

    // 处理碰撞伤害
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(10f);
        }
    }

    // 更新生命值
    void UpdateHealth()
    {
        if (blackboard.currentHealth <= 0 && fsm.curState != fsm.states[MY_FSM.StateType.Die])
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        blackboard.currentHealth -= damage;
        blackboard.currentHealth = Mathf.Clamp(blackboard.currentHealth, 0, blackboard.maxHealth);
        Debug.Log($"Player took {damage} damage! Health: {blackboard.currentHealth}/{blackboard.maxHealth}");
    }

    // 调试绘制
    
}