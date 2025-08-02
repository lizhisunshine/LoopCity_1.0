using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

[Serializable]

#region  敌人黑板
public class EnemyBlackboard : Blackboard
{
    [Header("动画参数")]
    public string moveAnimParam = "IsMoving";
    public string explodeTriggerParam = "Explode";
    public string dieTriggerParam = "Die"; // 死亡动画触发器
    public bool hasExplodeAnimation = true;
    public bool hasDieAnimation = true;   // 新增死亡动画标志

    [Header("动画引用")]
    public Animator animator;

    public enum EnemyType { Exploder, Dasher, Shooter }
    public EnemyType enemyType;

    // 爆炸怪专属参数
    public float explosionRadius = 1f;
    public float explosionDamage = 20f;

    // 冲刺怪专属参数
    public float dashDistance = 1.5f;
    public float dashCooldown = 1.5f;
    public float dashDamage = 10f;
    [HideInInspector] public float dashTimer;
    [HideInInspector] public bool isDashing;

    // 射手怪专属参数
    [Header("射手怪设置")]
    public float minRange = 3.5f;
    public float maxRange = 4.5f;
    public float projectileDamage = 5f;
    public float attackInterval = 2f;
    public GameObject projectilePrefab;

    [HideInInspector] public float attackTimer;
    [HideInInspector] public Vector2 idealPosition;

    [Header("敌人属性")]
    public float maxHealth = 100f;
    public float idleTime = 2f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseDistance = 5f;
    public float attackDistance = 1.5f;

    [Header("引用")]
    public Transform transform;
    public Transform playerTransform;

    [Header("运行时数据")]
    public float currentHealth;
    public Vector2 targetPos;
}
#endregion

#region  爆炸怪
public class Enemy_ExplodeState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;
    private bool hasExploded;
    private bool isDeathExplosion; // 标记是否为死亡爆炸
    private float explosionTimer;
    private float explosionAnimationLength = 0.5f; // 默认爆炸动画长度

    public Enemy_ExplodeState(FSM fsm)
    {
        if (fsm == null)
        {
            Debug.LogError("FSM is null in Enemy_ExplodeState constructor!");
            return;
        }
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
        if (blackboard == null)
        {
            Debug.LogError("Failed to cast blackboard to EnemyBlackboard in Enemy_ExplodeState");
        }
    }

    public void OnEnter()
    {
        Debug.Log("主动爆炸攻击!");
        hasExploded = false;
        explosionTimer = 0f;

        // 只播放爆炸动画（主动攻击）
        if (blackboard.animator != null && blackboard.hasExplodeAnimation)
        {
            if (HasAnimationTrigger(blackboard.explodeTriggerParam))
            {
                blackboard.animator.SetTrigger(blackboard.explodeTriggerParam);
            }
            else if (HasAnimationState("Explode"))
            {
                blackboard.animator.Play("Explode");
            }
            explosionAnimationLength = GetAnimationLength("Explode");
        }

        // 禁用碰撞体
        Collider2D collider = blackboard.transform.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // 停止移动
        if (blackboard.transform.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 总是执行爆炸伤害（主动攻击）
        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            blackboard.transform.position,
            blackboard.explosionRadius
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(blackboard.explosionDamage);
            }
        }

        hasExploded = true;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        explosionTimer += Time.deltaTime;

        // 如果是死亡爆炸，标记为已爆炸（不执行伤害）
        if (isDeathExplosion && !hasExploded && explosionTimer > 0.1f)
        {
            hasExploded = true;
        }

        // 动画播放完毕后销毁
        if (explosionTimer >= explosionAnimationLength)
        {
            GameObject.Destroy(blackboard.transform.gameObject);
        }
    }


    // 检查动画触发器是否存在
    private bool HasAnimationTrigger(string triggerName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                param.name == triggerName)
            {
                return true;
            }
        }
        return false;
    }

    // 检查是否有特定名称的动画状态
    private bool HasAnimationState(string stateName)
    {
        if (blackboard.animator == null) return false;

        var controller = blackboard.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }
        return false;
    }

    // 获取动画长度
    private float GetAnimationLength(string stateName)
    {
        if (blackboard.animator == null) return explosionAnimationLength;

        var controller = blackboard.animator.runtimeAnimatorController;
        if (controller == null) return explosionAnimationLength;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
            {
                return clip.length;
            }
        }
        return explosionAnimationLength;
    }
}
#endregion#region  冲刺怪

#region  冲刺怪
public class Enemy_DashState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;
    private Vector2 dashDirection;
    private Vector2 dashStartPosition;
    private bool hasDamaged;
    private bool animationStarted;

    public Enemy_DashState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        dashDirection = (blackboard.playerTransform.position -
                        blackboard.transform.position).normalized;
        dashStartPosition = blackboard.transform.position;
        blackboard.isDashing = true;
        hasDamaged = false;
        animationStarted = false;

        // 触发冲刺动画
        if (blackboard.animator != null)
        {
            // 方法1: 使用布尔值
            if (HasAnimationParameter("IsDashing"))
            {
                blackboard.animator.SetBool("IsDashing", true);
            }
            // 方法2: 使用触发器
            else if (HasAnimationTrigger("Dash"))
            {
                blackboard.animator.SetTrigger("Dash");
            }
            // 方法3: 直接播放动画状态
            else
            {
                // 确保有 "Dash" 状态
                if (HasAnimationState("Dash"))
                {
                    blackboard.animator.Play("Dash");
                }
            }
            animationStarted = true;
        }
    }

    public void OnExit()
    {
        blackboard.isDashing = false;
        blackboard.dashTimer = blackboard.dashCooldown;

        // 重置动画状态
        if (blackboard.animator != null && animationStarted)
        {
            if (HasAnimationParameter("IsDashing"))
            {
                blackboard.animator.SetBool("IsDashing", false);
            }
        }
    }

    public void OnUpdate()
    {
        // 冲刺移动
        blackboard.transform.Translate(
            dashDirection * blackboard.chaseSpeed * 5 * Time.deltaTime,
            Space.World
        );

        // 检测是否达到冲刺距离
        float dashDistance = Vector2.Distance(dashStartPosition, blackboard.transform.position);
        if (dashDistance >= blackboard.dashDistance)
        {
            // 冲刺结束后回到Idle状态
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 检测碰撞玩家
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

                    // 禁用碰撞体后再销毁
                    Collider2D collider = blackboard.transform.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = false;

                    // 冲刺怪碰到玩家后自身销毁
                    GameObject.Destroy(blackboard.transform.gameObject);
                }
            }
        }
    }

    // 检查是否有特定名称的动画状态
    private bool HasAnimationState(string stateName)
    {
        if (blackboard.animator == null) return false;

        var controller = blackboard.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var state in controller.animationClips)
        {
            if (state.name == stateName)
                return true;
        }
        return false;
    }

    // 检查动画参数是否存在
    private bool HasAnimationParameter(string paramName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    // 检查动画触发器是否存在
    private bool HasAnimationTrigger(string triggerName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                param.name == triggerName)
            {
                return true;
            }
        }
        return false;
    }
}
#endregion

#region  射手怪
// 射手怪移动状态
public class Enemy_ShooterMoveState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;
    private float lastPositionUpdateTime;

    public Enemy_ShooterMoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        UpdateIdealPosition();
        lastPositionUpdateTime = Time.time;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // 死亡检查 - 放在最前面
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        if (blackboard.playerTransform == null) return;

        float sqrDistanceToPlayer = (blackboard.playerTransform.position - blackboard.transform.position).sqrMagnitude;
        if (sqrDistanceToPlayer > blackboard.chaseDistance * blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 每秒更新一次位置
        if (Time.time - lastPositionUpdateTime > 1f)
        {
            UpdateIdealPosition();
            lastPositionUpdateTime = Time.time;
        }

        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            blackboard.idealPosition,
            blackboard.moveSpeed * Time.deltaTime
        );

        float distanceToIdeal = Vector2.Distance(
            blackboard.transform.position,
            blackboard.idealPosition
        );

        if (distanceToIdeal < 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Shoot);
        }
    }

    private void UpdateIdealPosition()
    {
        if (blackboard.playerTransform == null) return;

        Vector2 direction = (blackboard.transform.position -
                            blackboard.playerTransform.position).normalized;

        float idealDistance = Random.Range(blackboard.minRange, blackboard.maxRange);
        blackboard.idealPosition = (Vector2)blackboard.playerTransform.position +
                                  direction * idealDistance;
    }
}

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
        // 死亡检查 - 放在最前面
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        if (blackboard.playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        // 如果玩家超出追击范围，回到Idle
        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // 如果玩家不在攻击范围内，切换到追击状态
        if (distanceToPlayer < blackboard.minRange || distanceToPlayer > blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

        // 在攻击范围内，直接攻击
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

        Vector2 direction = (blackboard.playerTransform.position -
                            blackboard.transform.position).normalized;

        GameObject projectile = GameObject.Instantiate(
            blackboard.projectilePrefab,
            blackboard.transform.position,
            Quaternion.identity
        );

        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(direction);
            proj.damage = blackboard.projectileDamage;
            proj.isPlayerBullet = false;
        }
    }

}
#endregion

#region  死亡状态
// 死亡状态
public class Enemy_DieState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;
    private float deathTimer;
    private bool deathAnimationStarted;
    private float deathAnimationLength = 1.0f; // 默认死亡动画长度

    public Enemy_DieState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        Debug.Log("敌人死亡!");
        deathTimer = 0f;
        deathAnimationStarted = false;

        // 禁用碰撞体
        Collider2D collider = blackboard.transform.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // 停止移动
        if (blackboard.transform.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 播放死亡动画
        PlayDeathAnimation();
    }

    private void PlayDeathAnimation()
    {
        // 优先使用配置的死亡触发器
        if (blackboard.animator != null && !string.IsNullOrEmpty(blackboard.dieTriggerParam))
        {
            if (HasAnimationTrigger(blackboard.dieTriggerParam))
            {
                blackboard.animator.SetTrigger(blackboard.dieTriggerParam);
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength(blackboard.dieTriggerParam);
            }
            else if (HasAnimationState(blackboard.dieTriggerParam))
            {
                blackboard.animator.Play(blackboard.dieTriggerParam);
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength(blackboard.dieTriggerParam);
            }
        }

        // 如果没有配置特定触发器，尝试通用方法
        if (!deathAnimationStarted && blackboard.animator != null)
        {
            // 尝试使用"Die"触发器
            if (HasAnimationTrigger("Die"))
            {
                blackboard.animator.SetTrigger("Die");
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength("Die");
            }
            // 尝试播放"Die"动画状态
            else if (HasAnimationState("Die"))
            {
                blackboard.animator.Play("Die");
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength("Die");
            }
            // 尝试播放"Death"动画状态
            else if (HasAnimationState("Death"))
            {
                blackboard.animator.Play("Death");
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength("Death");
            }
        }

        if (!deathAnimationStarted)
        {
            Debug.LogWarning("没有找到死亡动画，将直接销毁敌人");
            GameObject.Destroy(blackboard.transform.gameObject);
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (deathAnimationStarted)
        {
            deathTimer += Time.deltaTime;

            // 动画结束后销毁
            if (deathTimer >= deathAnimationLength)
            {
                GameObject.Destroy(blackboard.transform.gameObject);
            }
        }
    }

    // 检查动画触发器是否存在
    private bool HasAnimationTrigger(string triggerName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                param.name == triggerName)
            {
                return true;
            }
        }
        return false;
    }

    // 检查是否有特定名称的动画状态
    private bool HasAnimationState(string stateName)
    {
        if (blackboard.animator == null) return false;

        var controller = blackboard.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }
        return false;
    }

    // 获取动画长度
    private float GetAnimationLength(string stateName)
    {
        if (blackboard.animator == null) return 1.0f;

        var controller = blackboard.animator.runtimeAnimatorController;
        if (controller == null) return 1.0f;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
            {
                return clip.length;
            }
        }
        return 1.0f;
    }
}
#endregion

#region  待机状态
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

        // 设置移动动画为 false

        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, false);
        }

        // 设置默认的Idle动画
        if (blackboard.animator != null)
        {
            // 确保动画控制器已经初始化
            if (blackboard.animator.runtimeAnimatorController != null)
            {
                // 检查是否有 "IsMoving" 参数
                if (HasAnimationParameter("IsMoving"))
                {
                    blackboard.animator.SetBool("IsMoving", false);
                }

                // 检查是否有 "IsDashing" 参数
                if (HasAnimationParameter("IsDashing"))
                {
                    blackboard.animator.SetBool("IsDashing", false);
                }

                // 强制播放Idle动画
                PlayAnimationIfExists("Idle");
            }
            else
            {
                Debug.LogWarning("Animator controller is not assigned on enemy: " + blackboard.transform.name);
            }
        }
        else
        {
            Debug.LogWarning("Animator not found on enemy: " + blackboard.transform.name);
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

        if (blackboard.playerTransform == null) return;

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

        // 确保动画状态正确
        EnsureAnimationState();
    }

    // 确保动画在正确状态
    private void EnsureAnimationState()
    {
        if (blackboard.animator == null ||
            blackboard.animator.runtimeAnimatorController == null)
            return;

        // 获取当前状态信息
        var stateInfo = blackboard.animator.GetCurrentAnimatorStateInfo(0);

        // 检查是否在Idle状态
        if (!stateInfo.IsName("Idle"))
        {
            // 尝试切换到Idle状态
            PlayAnimationIfExists("Idle");
        }
    }

    // 检查动画参数是否存在
    public bool HasAnimationParameter(string paramName)
    {
        if (blackboard.animator == null) return false;

        // 遍历所有参数
        foreach (var param in blackboard.animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    // 播放指定动画（如果存在）
    private void PlayAnimationIfExists(string stateName)
    {
        if (blackboard.animator == null || blackboard.animator.runtimeAnimatorController == null)
            return;

        // 检查动画状态是否存在
        bool stateExists = false;
        RuntimeAnimatorController ac = blackboard.animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == stateName)
            {
                stateExists = true;
                break;
            }
        }

        // 如果存在则播放
        if (stateExists)
        {
            blackboard.animator.Play(stateName, 0, 0f); // 从开始播放
        }
        else
        {
            Debug.LogWarning($"Animation state '{stateName}' not found in animator controller");
        }
    }
}
#endregion

#region  移动状态
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
        float randomX = Random.Range(-5, 5);
        float randomY = Random.Range(-5, 5);
        blackboard.targetPos = new Vector2(
            blackboard.transform.position.x + randomX,
            blackboard.transform.position.y + randomY
        );

        // 设置移动动画
        if (blackboard.animator != null && HasAnimationParameter("IsMoving"))
        {
            blackboard.animator.SetBool("IsMoving", true);
        }
        // 设置移动动画
        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, true);
        }
    }

    public void OnExit() 
    {
        // 退出时设置移动动画为 false
        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, false);
        }
    }

    public void OnUpdate()
    {
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        if (blackboard.playerTransform == null) return;

        float sqrDistanceToPlayer = (blackboard.playerTransform.position - blackboard.transform.position).sqrMagnitude;
        float sqrChaseDistance = blackboard.chaseDistance * blackboard.chaseDistance;

        if (sqrDistanceToPlayer <= sqrChaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

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

    // 检查动画参数是否存在
    private bool HasAnimationParameter(string paramName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
}
#endregion

#region  追击状态
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
        // 设置移动动画
        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, true);
        }

        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            blackboard.dashTimer = Mathf.Max(0, blackboard.dashTimer);
        }

        // 设置移动动画
        if (blackboard.animator != null && HasAnimationParameter("IsMoving"))
        {
            blackboard.animator.SetBool("IsMoving", true);
        }
    }

    // 检查动画参数是否存在
    private bool HasAnimationParameter(string paramName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    public void OnExit()
    {
        // 退出时设置移动动画为 false
        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, false);
        }
    }

    public void OnUpdate()
    {
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        if (blackboard.playerTransform == null)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        Vector2 playerPosition = blackboard.playerTransform.position;
        float sqrDistanceToPlayer = (playerPosition - (Vector2)blackboard.transform.position).sqrMagnitude;
        float sqrChaseDistance = blackboard.chaseDistance * blackboard.chaseDistance;

        if (sqrDistanceToPlayer > sqrChaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        switch (blackboard.enemyType)
        {
            case EnemyBlackboard.EnemyType.Exploder:
                HandleExploder(playerPosition, sqrDistanceToPlayer);
                break;

            case EnemyBlackboard.EnemyType.Dasher:
                HandleDasher(playerPosition, sqrDistanceToPlayer);
                break;

            case EnemyBlackboard.EnemyType.Shooter:
                HandleShooter(playerPosition, sqrDistanceToPlayer);
                break;
        }
    }

    private void HandleExploder(Vector2 playerPosition, float sqrDistance)
    {
        float sqrAttackDistance = blackboard.attackDistance * blackboard.attackDistance;
        if (sqrDistance <= sqrAttackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Explode);
            return;
        }

        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleDasher(Vector2 playerPosition, float sqrDistance)
    {
        float sqrAttackDistance = blackboard.attackDistance * blackboard.attackDistance;
        if (sqrDistance <= sqrAttackDistance &&
            !blackboard.isDashing &&
            blackboard.dashTimer <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Dash);
            return;
        }

        if (blackboard.dashTimer > 0)
        {
            blackboard.dashTimer -= Time.deltaTime;
        }

        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleShooter(Vector2 playerPosition, float sqrDistanceToPlayer)
    {
        float minRange = blackboard.minRange;
        float maxRange = blackboard.maxRange;
        float distanceToPlayer = Mathf.Sqrt(sqrDistanceToPlayer); // 计算实际距离

        // 如果玩家在攻击范围内，直接射击
        if (distanceToPlayer >= minRange && distanceToPlayer <= maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.Shoot);
        }
        else
        {
            // 否则继续追击
            blackboard.transform.position = Vector2.MoveTowards(
                blackboard.transform.position,
                playerPosition,
                blackboard.chaseSpeed * Time.deltaTime
            );
        }
    }
}
#endregion

#region  怪物FSM行为类
public class Enemy_FSM : MonoBehaviour
{
    private FSM fsm;
    public EnemyBlackboard blackboard;

    void Start()
    {
        InitializeFSM();

        // 确保 animator 被赋值
        if (blackboard.animator == null)
        {
            blackboard.animator = GetComponent<Animator>();
            if (blackboard.animator == null)
            {
                Debug.LogError("Animator component not found on enemy: " + gameObject.name);
            }
        }

        // 解决怪物碰撞问题：设置怪物物理层
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            Physics2D.IgnoreLayerCollision(
                LayerMask.NameToLayer("Enemy"),
                LayerMask.NameToLayer("Enemy"),
                true
            );
        }
        else
        {
            Debug.LogWarning("'Enemy' layer not defined. Creating it.");
        }
    }

    // 检查动画触发器是否存在
    private bool HasAnimationTrigger(string triggerName)
    {
        if (blackboard.animator == null) return false;

        foreach (AnimatorControllerParameter param in blackboard.animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger &&
                param.name == triggerName)
            {
                return true;
            }
        }
        return false;
    }

    // 检查是否有特定名称的动画状态
    private bool HasAnimationState(string stateName)
    {
        if (blackboard.animator == null) return false;

        var controller = blackboard.animator.runtimeAnimatorController;
        if (controller == null) return false;

        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }
        return false;
    }

    private void InitializeFSM()
    {
        if (blackboard == null)
        {
            blackboard = new EnemyBlackboard();
        }

        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

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

        fsm = new FSM(blackboard);

        // 添加所有状态
        fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));
        fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
        fsm.AddState(MY_FSM.StateType.Dash, new Enemy_DashState(fsm));
        fsm.AddState(MY_FSM.StateType.ShooterMove, new Enemy_ShooterMoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));

        // 设置碰撞层
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            Debug.LogWarning("'Enemy' layer not defined.");
        }

        fsm.SwitchState(MY_FSM.StateType.Idle);
    }

    void Update()
    {
        if (fsm != null)
        {
            fsm.OnUpdate();
        }
        Flip();
    }

    private void Flip()
    {
        if (blackboard.playerTransform != null)
        {
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

    public void TakeDamage(float damage)
    {
        if (blackboard.currentHealth <= 0) return;

        blackboard.currentHealth -= damage;
        blackboard.currentHealth = Mathf.Max(blackboard.currentHealth, 0);

        Debug.Log($"敌人受到 {damage} 伤害! 生命: {blackboard.currentHealth}/{blackboard.maxHealth}");

        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (blackboard == null || blackboard.transform == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blackboard.chaseDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blackboard.attackDistance);

        if (Application.isPlaying && blackboard.targetPos != Vector2.zero)
        {
            if (fsm != null && fsm.curState != null && fsm.curState is Enemy_MoveState)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, blackboard.targetPos);
                Gizmos.DrawSphere(blackboard.targetPos, 0.2f);
            }
        }
    }
}
#endregion