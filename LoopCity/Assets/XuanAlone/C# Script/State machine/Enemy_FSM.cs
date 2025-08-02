using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

[Serializable]

#region  ���˺ڰ�
public class EnemyBlackboard : Blackboard
{
    public enum EnemyType { Exploder, Dasher, Shooter }
    public EnemyType enemyType;

    // ��ը��ר������
    public float explosionRadius = 1f;
    public float explosionDamage = 20f;

    // ��̹�ר������
    public float dashDistance = 1.5f;
    public float dashCooldown = 1.5f;
    public float dashDamage = 10f;
    [HideInInspector] public float dashTimer;
    [HideInInspector] public bool isDashing;

    // ���ֹ�ר������
    [Header("���ֹ�����")]
    public float minRange = 3.5f;
    public float maxRange = 4.5f;
    public float projectileDamage = 5f;
    public float attackInterval = 2f;
    public GameObject projectilePrefab;

    [HideInInspector] public float attackTimer;
    [HideInInspector] public Vector2 idealPosition;

    [Header("��������")]
    public float maxHealth = 100f;
    public float idleTime = 2f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float chaseDistance = 5f;
    public float attackDistance = 1.5f;

    [Header("����")]
    public Transform transform;
    public Transform playerTransform;

    [Header("����ʱ����")]
    public float currentHealth;
    public Vector2 targetPos;
}

#endregion

#region  ��ը��
public class Enemy_ExplodeState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

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
        Debug.Log("Exploding!");

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

        GameObject.Destroy(blackboard.transform.gameObject);
    }

    public void OnExit() { }
    public void OnUpdate() { }
}
#endregion

#region  ��̹�
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
        dashDirection = (blackboard.playerTransform.position -
                        blackboard.transform.position).normalized;
        dashStartPosition = blackboard.transform.position;
        blackboard.isDashing = true;
        hasDamaged = false;
    }

    public void OnExit()
    {
        blackboard.isDashing = false;
        blackboard.dashTimer = blackboard.dashCooldown;
    }

    public void OnUpdate()
    {
        blackboard.transform.Translate(
            dashDirection * blackboard.chaseSpeed * 5 * Time.deltaTime,
            Space.World
        );

        float dashDistance = Vector2.Distance(dashStartPosition, blackboard.transform.position);
        if (dashDistance >= blackboard.dashDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

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
                    GameObject.Destroy(blackboard.transform.gameObject);
                }
            }
        }
    }
}

#endregion

#region  ���ֹ�
// ���ֹ��ƶ�״̬
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

        // ÿ�����һ��λ��
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

        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        if (distanceToPlayer < blackboard.minRange || distanceToPlayer > blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.ShooterMove);
            return;
        }

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

#region  ����״̬
// ����״̬
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
        Debug.Log("Enemy died!");
        GameObject.Destroy(blackboard.transform.gameObject, 0.5f);
    }

    public void OnExit() { }
    public void OnUpdate() { }
}
#endregion

#region  ����״̬
// ����״̬
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

        idleTimer += Time.deltaTime;
        if (idleTimer > blackboard.idleTime)
        {
            fsm.SwitchState(MY_FSM.StateType.Move);
        }
    }
}

#endregion

#region  �ƶ�״̬
// �ƶ�״̬�����ߣ�
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
    }

    public void OnExit() { }

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
}
#endregion

#region  ׷��״̬
// ׷��״̬������ӣ�
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
        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            blackboard.dashTimer = Mathf.Max(0, blackboard.dashTimer);
        }
    }

    public void OnExit() { }

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
                HandleShooter();
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

    private void HandleShooter()
    {
        fsm.SwitchState(MY_FSM.StateType.ShooterMove);
    }
}

#endregion

#region  ����FSM��Ϊ��
public class Enemy_FSM : MonoBehaviour
{
    private FSM fsm;
    public EnemyBlackboard blackboard;

    void Start()
    {
        InitializeFSM();
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

        // �������״̬
        fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));
        fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
        fsm.AddState(MY_FSM.StateType.Dash, new Enemy_DashState(fsm));
        fsm.AddState(MY_FSM.StateType.ShooterMove, new Enemy_ShooterMoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));

        // ������ײ��
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

        Debug.Log($"Enemy took {damage} damage! Health: {blackboard.currentHealth}/{blackboard.maxHealth}");

        if (blackboard.currentHealth <= 0)
        {
            if (fsm != null)
            {
                fsm.SwitchState(MY_FSM.StateType.Die);
            }
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