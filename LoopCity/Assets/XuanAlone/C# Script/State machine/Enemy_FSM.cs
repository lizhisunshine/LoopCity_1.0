using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyBlackboard : Blackboard
{
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

    public void OnEnter() { }

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

        // 如果玩家在攻击范围内，切换到攻击状态（可选）
        /*
        if (distanceToPlayer <= blackboard.attackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Attack);
            return;
        }
        */

        // 向玩家移动
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }
}

public class Enemy_FSM : MonoBehaviour
{
    private FSM fsm;
    public EnemyBlackboard blackboard;

    void Start()
    {
        // 初始化黑板数据
        if (blackboard == null)
            blackboard = new EnemyBlackboard();

        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // 关键修改：设置碰撞层
        gameObject.layer = LayerMask.NameToLayer("Enemy");


        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            blackboard.playerTransform = player.transform;

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
        fsm.OnUpdate();
        Flip();
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