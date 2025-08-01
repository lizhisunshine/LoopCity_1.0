using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyBlackboard : Blackboard
{
    // 添加怪物类型枚举
    public enum EnemyType { Exploder, Dasher, Shooter }
    public EnemyType enemyType;

    // 爆炸怪专属参数
    public float explosionRadius = 1f;
    public float explosionDamage = 20f;

    // 冲刺怪专属参数
    public float dashDistance = 1.5f;
    public float dashCooldown = 1.5f;
    public float dashDamage = 10f;
    [HideInInspector] public float dashTimer; // 冲刺冷却计时
    [HideInInspector] public bool isDashing; // 是否正在冲刺

    // 射手怪专属参数
    [Header("射手怪设置")]
    public float minRange = 3.5f; // 最小攻击距离
    public float maxRange = 4.5f; // 最大攻击距离
    public float projectileDamage = 5f; // 子弹伤害
    public float attackInterval = 2f; // 攻击间隔
    public GameObject projectilePrefab; // 子弹预制体

    [HideInInspector] public float attackTimer; // 攻击计时器
    [HideInInspector] public Vector2 idealPosition; // 理想位置

    [Header("敌人属性")]
    public float maxHealth = 100f;
    public float idleTime = 2f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f; // 追击速度
    public float chaseDistance = 5f; // 开始追击的距离
    public float attackDistance = 1.5f; // 攻击距离

    [Header("引用")]
    public Transform transform;
    public Transform playerTransform; // 玩家引用

    [Header("运行时数据")]
    public float currentHealth;
    public Vector2 targetPos;
}

public class Enemy_ExplodeState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_ExplodeState(FSM fsm)
    {
        // 确保传入的 fsm 不为 null
        if (fsm == null)
        {
            Debug.LogError("FSM is null in Enemy_ExplodeState constructor!");
            return;
        }
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
        // 检查转换是否成功
        if (blackboard == null)
        {
            Debug.LogError("Failed to cast blackboard to EnemyBlackboard in Enemy_ExplodeState");
        }
    }

    public void OnEnter()
    {
        // 播放爆炸动画/特效
        Debug.Log("Exploding!");

        // 检测爆炸范围内的玩家
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            blackboard.transform.position,
            blackboard.explosionRadius
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // 在爆炸怪状态中
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(blackboard.explosionDamage);
            }
        }

        // 销毁敌人
        GameObject.Destroy(blackboard.transform.gameObject);
    }

    public void OnExit() { }
    public void OnUpdate() { }
}

public class Enemy_DashState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;
    private Vector2 dashDirection;
    private Vector2 dashStartPosition;
    private bool hasDamaged;

    public Enemy_DashState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        // 设置冲刺方向（指向玩家）
        dashDirection = (blackboard.playerTransform.position -
                        blackboard.transform.position).normalized;
        dashStartPosition = blackboard.transform.position;
        blackboard.isDashing = true;
        hasDamaged = false;
    }

    public void OnExit()
    {
        blackboard.isDashing = false;
        blackboard.dashTimer = blackboard.dashCooldown; // 重置冷却计时器
    }

    public void OnUpdate()
    {
        // 冲刺移动
        blackboard.transform.Translate(
            dashDirection * blackboard.chaseSpeed * 5 * Time.deltaTime,
            Space.World
        );
        // 处理冲刺怪的冷却
        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            if (blackboard.dashTimer > 0)
            {
                blackboard.dashTimer -= Time.deltaTime;
            }
        }
        // 检测是否达到冲刺距离
        float dashDistance = Vector2.Distance(dashStartPosition, blackboard.transform.position);
        if (dashDistance >= blackboard.dashDistance)
        {
            // 冲刺结束后回到Idle状态
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 检测碰撞玩家（只在冲刺过程中检测一次）
        if (!hasDamaged)
        {
            float distance = Vector2.Distance(
                blackboard.transform.position,
                blackboard.playerTransform.position
            );

            if (distance < 0.5f)
            {
                PlayerHealth playerHealth = blackboard.playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(blackboard.dashDamage);
                    hasDamaged = true;

                    // 冲刺怪碰到玩家后自身销毁
                    GameObject.Destroy(blackboard.transform.gameObject);
                }
            }
        }
    }
}

// 射手怪移动状态
public class Enemy_ShooterMoveState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_ShooterMoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        // 计算理想位置（玩家位置后退到maxRange处）
        Vector2 direction = (blackboard.transform.position -
                            blackboard.playerTransform.position).normalized;

        // 在minRange和maxRange之间随机选择一个距离
        float idealDistance = Random.Range(blackboard.minRange, blackboard.maxRange);

        blackboard.idealPosition = (Vector2)blackboard.playerTransform.position +
                                  direction * idealDistance;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 检查是否死亡
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // 检查玩家是否超出追击范围
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 向理想位置移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            blackboard.idealPosition,
            blackboard.moveSpeed * Time.deltaTime
        );

        // 检查是否到达理想位置
        float distanceToIdeal = Vector2.Distance(
            blackboard.transform.position,
            blackboard.idealPosition
        );

        if (distanceToIdeal < 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Shoot);
        }
    }
}

// 射手怪攻击状态
public class Enemy_ShootState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_ShootState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        blackboard.attackTimer = 0;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 检查是否死亡
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // 检查玩家是否超出追击范围
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 检查是否需要调整位置
        if (distanceToPlayer < blackboard.minRange || distanceToPlayer > blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.ShooterMove);
            return;
        }

        // 攻击计时
        blackboard.attackTimer += Time.deltaTime;
        if (blackboard.attackTimer >= blackboard.attackInterval)
        {
            Shoot();
            blackboard.attackTimer = 0;
        }
    }

    private void Shoot()
    {
        if (blackboard.projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        // 计算射击方向
        Vector2 direction = (blackboard.playerTransform.position -
                            blackboard.transform.position).normalized;

        // 创建子弹
        GameObject projectile = GameObject.Instantiate(
            blackboard.projectilePrefab,
            blackboard.transform.position,
            Quaternion.identity
        );

        // 设置子弹属性
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(direction);
            proj.damage = blackboard.projectileDamage;
            proj.isPlayerBullet = false; // 这是敌人子弹
        }
    }
}


// 死亡状态
public class Enemy_DieState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_DieState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        // 播放死亡动画或效果
        Debug.Log("Enemy died!");

        // 销毁敌人对象
        GameObject.Destroy(blackboard.transform.gameObject, 0.5f);
    }

    public void OnExit() { }

    public void OnUpdate() { }
}

// 待机状态
public class Enemy_IdleState : IState
{
    private float idleTimer;
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        idleTimer = 0;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 检查是否死亡
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // 检查玩家距离
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        // 如果玩家在追击范围内，切换到追击状态
        if (distanceToPlayer <= blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

        // 待机计时
        idleTimer += Time.deltaTime;
        if (idleTimer > blackboard.idleTime)
        {
            fsm.SwitchState(MY_FSM.StateType.Move);
        }

        Vector2 playerPosition = blackboard.playerTransform.position;
        _ = Vector2.Distance(
            blackboard.transform.position,
            playerPosition
        );

        // 玩家超出追击范围
        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 根据怪物类型执行不同行为
        switch (blackboard.enemyType)
        {
            case EnemyBlackboard.EnemyType.Exploder:
                HandleExploder(playerPosition, distanceToPlayer);
                break;

            case EnemyBlackboard.EnemyType.Dasher:
                HandleDasher(distanceToPlayer);
                break;

            case EnemyBlackboard.EnemyType.Shooter:
                HandleShooter(playerPosition, distanceToPlayer);
                break;
        }
    }

    private void HandleExploder(Vector2 playerPosition, float distance)
    {
        // 到达爆炸距离
        if (distance <= blackboard.attackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Explode);
            return;
        }

        // 继续向玩家移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleDasher(float distance)
    {
        // 进入冲刺范围且不在冷却中
        if (distance <= blackboard.attackDistance &&
            !blackboard.isDashing &&
            blackboard.dashTimer <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Dash);
            return;
        }

        // 冲刺冷却计时
        if (blackboard.dashTimer > 0)
        {
            blackboard.dashTimer -= Time.deltaTime;
        }

        // 向玩家移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            blackboard.playerTransform.position,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleShooter(Vector2 playerPosition, float distance)
    {
        // 进入射击范围
        if (distance >= blackboard.minRange && distance <= blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.Shoot);
            return;
        }

        // 计算理想位置（玩家位置后退到maxRange处）
        Vector2 direction = (blackboard.transform.position -
                            (Vector3)playerPosition).normalized;

        Vector2 targetPosition = playerPosition + direction *
                                ((blackboard.minRange + blackboard.maxRange) / 2);

        // 向理想位置移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            targetPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }
}


// 移动状态（游走）
public class Enemy_MoveState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_MoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        // 随机生成目标位置
        float randomX = Random.Range(-5, 5);
        float randomY = Random.Range(-5, 5);
        blackboard.targetPos = new Vector2(
            blackboard.transform.position.x + randomX,
            blackboard.transform.position.y + randomY
        );
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 检查是否死亡
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // 检查玩家距离
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        // 如果玩家在追击范围内，切换到追击状态
        if (distanceToPlayer <= blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

        // 移动逻辑
        if (Vector2.Distance(blackboard.transform.position, blackboard.targetPos) < 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
        }
        else
        {
            blackboard.transform.position = Vector2.MoveTowards(
                blackboard.transform.position,
                blackboard.targetPos,
                blackboard.moveSpeed * Time.deltaTime
            );
        }
    }
}

// 追击状态（新添加）
public class Enemy_ChaseState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_ChaseState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        // 进入追击状态时重置冲刺计时器（针对冲刺怪）
        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            blackboard.dashTimer = 0;
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 检查是否死亡
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // 获取玩家位置
        Vector2 playerPosition = blackboard.playerTransform.position;

        // 检查玩家距离
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            playerPosition
        );

        // 如果玩家超出追击范围，返回待机状态
        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 根据怪物类型执行不同行为
        switch (blackboard.enemyType)
        {
            case EnemyBlackboard.EnemyType.Exploder:
                HandleExploder(playerPosition, distanceToPlayer);
                break;

            case EnemyBlackboard.EnemyType.Dasher:
                HandleDasher(playerPosition, distanceToPlayer);
                break;

            case EnemyBlackboard.EnemyType.Shooter:
                HandleShooter();
                break;
        }
    }

    private void HandleExploder(Vector2 playerPosition, float distance)
    {
        // 如果进入爆炸距离，切换到爆炸状态
        if (distance <= blackboard.attackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Explode);
            return;
        }

        // 向玩家移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleDasher(Vector2 playerPosition, float distance)
    {
        // 如果进入冲刺范围且冷却结束，切换到冲刺状态
        if (distance <= blackboard.attackDistance &&
            !blackboard.isDashing &&
            blackboard.dashTimer <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Dash);
            return;
        }

        // 更新冲刺冷却计时
        if (blackboard.dashTimer > 0)
        {
            blackboard.dashTimer -= Time.deltaTime;
        }

        // 向玩家移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleShooter()
    {
        // 射手怪直接切换到射手移动状态
        fsm.SwitchState(MY_FSM.StateType.ShooterMove);
    }
}

public class Enemy_FSM : MonoBehaviour
{
    private FSM fsm;
    public EnemyBlackboard blackboard;

    void Start()
    {
        // 使用专用初始化方法
        InitializeFSM();

        // 初始化黑板数据
        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            blackboard.playerTransform = player.transform;
        else
            Debug.LogError("Player not found!");

        // 初始化状态机
        fsm = new FSM(blackboard);

        // 添加公共状态
        fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));

        // 添加特定怪物状态
        fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
        fsm.AddState(MY_FSM.StateType.Dash, new Enemy_DashState(fsm));
        fsm.AddState(MY_FSM.StateType.ShooterMove, new Enemy_ShooterMoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));

        // 初始状态
        fsm.SwitchState(MY_FSM.StateType.Idle);

        // 设置碰撞层
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            Debug.LogWarning("'Enemy' layer not defined. Creating it.");
            // 这里可以添加代码自动创建层
        }
        // 添加新状态
        fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
        fsm.AddState(MY_FSM.StateType.Dash, new Enemy_DashState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));
        // 添加新状态
        fsm.AddState(MY_FSM.StateType.ShooterMove, new Enemy_ShooterMoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));

        // 初始化黑板数据
        if (blackboard == null)
            blackboard = new EnemyBlackboard();

        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // 关键修改：设置碰撞层
        gameObject.layer = LayerMask.NameToLayer("Enemy");


        

        // 初始化状态机
        fsm = new FSM(blackboard);

        // 添加状态
        fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));

        // 初始状态
        fsm.SwitchState(MY_FSM.StateType.Idle);
    }

    void Update()
    {
        // 添加空值检查
        if (fsm != null)
        {
            fsm.OnUpdate();
        }
        else
        {
            // 尝试重新初始化状态机
            Debug.LogWarning("FSM is null in Update, attempting to reinitialize.");
            InitializeFSM();
        }
        Flip();
    }
    // 初始化状态机的专用方法
    private void InitializeFSM()
    {
        // 确保黑板存在
        if (blackboard == null)
        {
            blackboard = new EnemyBlackboard();
            Debug.Log("EnemyBlackboard created in InitializeFSM.");
        }

        // 设置关键引用
        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // 查找玩家
        if (blackboard.playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                blackboard.playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Player not found in scene.");
            }
        }

        // 初始化状态机
        fsm = new FSM(blackboard);

        // 添加状态 - 确保所有状态都能安全初始化
        try
        {
            fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
            fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
            fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
            fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));
            fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
            // 添加其他状态...

            fsm.SwitchState(MY_FSM.StateType.Idle);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing FSM states: {ex.Message}");
        }
    }
    // 翻转敌人朝向
    void Flip()
    {
        if (blackboard.playerTransform != null)
        {
            // 根据玩家位置决定朝向
            if (blackboard.playerTransform.position.x > transform.position.x)
            {
                transform.localScale = new Vector2(-1, 1);
            }
            else
            {
                transform.localScale = new Vector2(1, 1);
            }
        }
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        if (blackboard.currentHealth <= 0) return; // 已死亡

        blackboard.currentHealth -= damage;
        blackboard.currentHealth = Mathf.Max(blackboard.currentHealth, 0);

        Debug.Log($"Enemy took {damage} damage! Health: {blackboard.currentHealth}/{blackboard.maxHealth}");

        // 如果血量归零，切换到死亡状态
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    // 调试绘制
    void OnDrawGizmosSelected()
    {
        // 检查 blackboard 是否为空
        if (blackboard == null)
            return;

        // 检查 blackboard 的属性是否存在
        if (blackboard.transform == null)
            return;

        // 绘制追击范围（确保有有效的 chaseDistance）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blackboard.chaseDistance);

        // 绘制攻击范围（确保有有效的 attackDistance）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blackboard.attackDistance);

        // 绘制当前目标位置 - 只在运行时且有有效目标时绘制
        if (Application.isPlaying && blackboard.targetPos != Vector2.zero)
        {
            // 检查 fsm 和当前状态
            if (fsm != null && fsm.curState != null)
            {
                // 检查当前状态是否为移动状态
                if (fsm.curState is Enemy_MoveState)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, blackboard.targetPos);
                    Gizmos.DrawSphere(blackboard.targetPos, 0.2f);
                }
            }
        }
    }

}