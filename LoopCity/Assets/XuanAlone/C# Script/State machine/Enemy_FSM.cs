using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyBlackboard : Blackboard
{
    // ��ӹ�������ö��
    public enum EnemyType { Exploder, Dasher, Shooter }
    public EnemyType enemyType;

    // ��ը��ר������
    public float explosionRadius = 1f;
    public float explosionDamage = 20f;

    // ��̹�ר������
    public float dashDistance = 1.5f;
    public float dashCooldown = 1.5f;
    public float dashDamage = 10f;
    [HideInInspector] public float dashTimer; // �����ȴ��ʱ
    [HideInInspector] public bool isDashing; // �Ƿ����ڳ��

    // ���ֹ�ר������
    [Header("���ֹ�����")]
    public float minRange = 3.5f; // ��С��������
    public float maxRange = 4.5f; // ��󹥻�����
    public float projectileDamage = 5f; // �ӵ��˺�
    public float attackInterval = 2f; // �������
    public GameObject projectilePrefab; // �ӵ�Ԥ����

    [HideInInspector] public float attackTimer; // ������ʱ��
    [HideInInspector] public Vector2 idealPosition; // ����λ��

    [Header("��������")]
    public float maxHealth = 100f;
    public float idleTime = 2f;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f; // ׷���ٶ�
    public float chaseDistance = 5f; // ��ʼ׷���ľ���
    public float attackDistance = 1.5f; // ��������

    [Header("����")]
    public Transform transform;
    public Transform playerTransform; // �������

    [Header("����ʱ����")]
    public float currentHealth;
    public Vector2 targetPos;
}

public class Enemy_ExplodeState : IState
{
    private FSM fsm;
    private EnemyBlackboard blackboard;

    public Enemy_ExplodeState(FSM fsm)
    {
        // ȷ������� fsm ��Ϊ null
        if (fsm == null)
        {
            Debug.LogError("FSM is null in Enemy_ExplodeState constructor!");
            return;
        }
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
        // ���ת���Ƿ�ɹ�
        if (blackboard == null)
        {
            Debug.LogError("Failed to cast blackboard to EnemyBlackboard in Enemy_ExplodeState");
        }
    }

    public void OnEnter()
    {
        // ���ű�ը����/��Ч
        Debug.Log("Exploding!");

        // ��ⱬը��Χ�ڵ����
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            blackboard.transform.position,
            blackboard.explosionRadius
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // �ڱ�ը��״̬��
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(blackboard.explosionDamage);
            }
        }

        // ���ٵ���
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
        // ���ó�̷���ָ����ң�
        dashDirection = (blackboard.playerTransform.position -
                        blackboard.transform.position).normalized;
        dashStartPosition = blackboard.transform.position;
        blackboard.isDashing = true;
        hasDamaged = false;
    }

    public void OnExit()
    {
        blackboard.isDashing = false;
        blackboard.dashTimer = blackboard.dashCooldown; // ������ȴ��ʱ��
    }

    public void OnUpdate()
    {
        // ����ƶ�
        blackboard.transform.Translate(
            dashDirection * blackboard.chaseSpeed * 5 * Time.deltaTime,
            Space.World
        );
        // �����ֵ̹���ȴ
        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            if (blackboard.dashTimer > 0)
            {
                blackboard.dashTimer -= Time.deltaTime;
            }
        }
        // ����Ƿ�ﵽ��̾���
        float dashDistance = Vector2.Distance(dashStartPosition, blackboard.transform.position);
        if (dashDistance >= blackboard.dashDistance)
        {
            // ��̽�����ص�Idle״̬
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // �����ײ��ң�ֻ�ڳ�̹����м��һ�Σ�
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

                    // ��̹�������Һ���������
                    GameObject.Destroy(blackboard.transform.gameObject);
                }
            }
        }
    }
}

// ���ֹ��ƶ�״̬
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
        // ��������λ�ã����λ�ú��˵�maxRange����
        Vector2 direction = (blackboard.transform.position -
                            blackboard.playerTransform.position).normalized;

        // ��minRange��maxRange֮�����ѡ��һ������
        float idealDistance = Random.Range(blackboard.minRange, blackboard.maxRange);

        blackboard.idealPosition = (Vector2)blackboard.playerTransform.position +
                                  direction * idealDistance;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // ����Ƿ�����
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // �������Ƿ񳬳�׷����Χ
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // ������λ���ƶ�
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            blackboard.idealPosition,
            blackboard.moveSpeed * Time.deltaTime
        );

        // ����Ƿ񵽴�����λ��
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

// ���ֹֹ���״̬
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
        // ����Ƿ�����
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // �������Ƿ񳬳�׷����Χ
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // ����Ƿ���Ҫ����λ��
        if (distanceToPlayer < blackboard.minRange || distanceToPlayer > blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.ShooterMove);
            return;
        }

        // ������ʱ
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

        // �����������
        Vector2 direction = (blackboard.playerTransform.position -
                            blackboard.transform.position).normalized;

        // �����ӵ�
        GameObject projectile = GameObject.Instantiate(
            blackboard.projectilePrefab,
            blackboard.transform.position,
            Quaternion.identity
        );

        // �����ӵ�����
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(direction);
            proj.damage = blackboard.projectileDamage;
            proj.isPlayerBullet = false; // ���ǵ����ӵ�
        }
    }
}


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
        // ��������������Ч��
        Debug.Log("Enemy died!");

        // ���ٵ��˶���
        GameObject.Destroy(blackboard.transform.gameObject, 0.5f);
    }

    public void OnExit() { }

    public void OnUpdate() { }
}

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
        // ����Ƿ�����
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // �����Ҿ���
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        // ��������׷����Χ�ڣ��л���׷��״̬
        if (distanceToPlayer <= blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

        // ������ʱ
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

        // ��ҳ���׷����Χ
        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // ���ݹ�������ִ�в�ͬ��Ϊ
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
        // ���ﱬը����
        if (distance <= blackboard.attackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Explode);
            return;
        }

        // ����������ƶ�
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleDasher(float distance)
    {
        // �����̷�Χ�Ҳ�����ȴ��
        if (distance <= blackboard.attackDistance &&
            !blackboard.isDashing &&
            blackboard.dashTimer <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Dash);
            return;
        }

        // �����ȴ��ʱ
        if (blackboard.dashTimer > 0)
        {
            blackboard.dashTimer -= Time.deltaTime;
        }

        // ������ƶ�
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            blackboard.playerTransform.position,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleShooter(Vector2 playerPosition, float distance)
    {
        // ���������Χ
        if (distance >= blackboard.minRange && distance <= blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.Shoot);
            return;
        }

        // ��������λ�ã����λ�ú��˵�maxRange����
        Vector2 direction = (blackboard.transform.position -
                            (Vector3)playerPosition).normalized;

        Vector2 targetPosition = playerPosition + direction *
                                ((blackboard.minRange + blackboard.maxRange) / 2);

        // ������λ���ƶ�
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            targetPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }
}


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
        // �������Ŀ��λ��
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
        // ����Ƿ�����
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // �����Ҿ���
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            blackboard.playerTransform.position
        );

        // ��������׷����Χ�ڣ��л���׷��״̬
        if (distanceToPlayer <= blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

        // �ƶ��߼�
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
        // ����׷��״̬ʱ���ó�̼�ʱ������Գ�̹֣�
        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            blackboard.dashTimer = 0;
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // ����Ƿ�����
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // ��ȡ���λ��
        Vector2 playerPosition = blackboard.playerTransform.position;

        // �����Ҿ���
        float distanceToPlayer = Vector2.Distance(
            blackboard.transform.position,
            playerPosition
        );

        // �����ҳ���׷����Χ�����ش���״̬
        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // ���ݹ�������ִ�в�ͬ��Ϊ
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
        // ������뱬ը���룬�л�����ը״̬
        if (distance <= blackboard.attackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Explode);
            return;
        }

        // ������ƶ�
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleDasher(Vector2 playerPosition, float distance)
    {
        // ��������̷�Χ����ȴ�������л������״̬
        if (distance <= blackboard.attackDistance &&
            !blackboard.isDashing &&
            blackboard.dashTimer <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Dash);
            return;
        }

        // ���³����ȴ��ʱ
        if (blackboard.dashTimer > 0)
        {
            blackboard.dashTimer -= Time.deltaTime;
        }

        // ������ƶ�
        blackboard.transform.position = Vector2.MoveTowards(
            blackboard.transform.position,
            playerPosition,
            blackboard.chaseSpeed * Time.deltaTime
        );
    }

    private void HandleShooter()
    {
        // ���ֹ�ֱ���л��������ƶ�״̬
        fsm.SwitchState(MY_FSM.StateType.ShooterMove);
    }
}

public class Enemy_FSM : MonoBehaviour
{
    private FSM fsm;
    public EnemyBlackboard blackboard;

    void Start()
    {
        // ʹ��ר�ó�ʼ������
        InitializeFSM();

        // ��ʼ���ڰ�����
        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // �������
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            blackboard.playerTransform = player.transform;
        else
            Debug.LogError("Player not found!");

        // ��ʼ��״̬��
        fsm = new FSM(blackboard);

        // ��ӹ���״̬
        fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));

        // ����ض�����״̬
        fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
        fsm.AddState(MY_FSM.StateType.Dash, new Enemy_DashState(fsm));
        fsm.AddState(MY_FSM.StateType.ShooterMove, new Enemy_ShooterMoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));

        // ��ʼ״̬
        fsm.SwitchState(MY_FSM.StateType.Idle);

        // ������ײ��
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            Debug.LogWarning("'Enemy' layer not defined. Creating it.");
            // ���������Ӵ����Զ�������
        }
        // �����״̬
        fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
        fsm.AddState(MY_FSM.StateType.Dash, new Enemy_DashState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));
        // �����״̬
        fsm.AddState(MY_FSM.StateType.ShooterMove, new Enemy_ShooterMoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Shoot, new Enemy_ShootState(fsm));

        // ��ʼ���ڰ�����
        if (blackboard == null)
            blackboard = new EnemyBlackboard();

        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // �ؼ��޸ģ�������ײ��
        gameObject.layer = LayerMask.NameToLayer("Enemy");


        

        // ��ʼ��״̬��
        fsm = new FSM(blackboard);

        // ���״̬
        fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));

        // ��ʼ״̬
        fsm.SwitchState(MY_FSM.StateType.Idle);
    }

    void Update()
    {
        // ��ӿ�ֵ���
        if (fsm != null)
        {
            fsm.OnUpdate();
        }
        else
        {
            // �������³�ʼ��״̬��
            Debug.LogWarning("FSM is null in Update, attempting to reinitialize.");
            InitializeFSM();
        }
        Flip();
    }
    // ��ʼ��״̬����ר�÷���
    private void InitializeFSM()
    {
        // ȷ���ڰ����
        if (blackboard == null)
        {
            blackboard = new EnemyBlackboard();
            Debug.Log("EnemyBlackboard created in InitializeFSM.");
        }

        // ���ùؼ�����
        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // �������
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

        // ��ʼ��״̬��
        fsm = new FSM(blackboard);

        // ���״̬ - ȷ������״̬���ܰ�ȫ��ʼ��
        try
        {
            fsm.AddState(MY_FSM.StateType.Idle, new Enemy_IdleState(fsm));
            fsm.AddState(MY_FSM.StateType.Move, new Enemy_MoveState(fsm));
            fsm.AddState(MY_FSM.StateType.Chase, new Enemy_ChaseState(fsm));
            fsm.AddState(MY_FSM.StateType.Die, new Enemy_DieState(fsm));
            fsm.AddState(MY_FSM.StateType.Explode, new Enemy_ExplodeState(fsm));
            // �������״̬...

            fsm.SwitchState(MY_FSM.StateType.Idle);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing FSM states: {ex.Message}");
        }
    }
    // ��ת���˳���
    void Flip()
    {
        if (blackboard.playerTransform != null)
        {
            // �������λ�þ�������
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

    // �ܵ��˺�
    public void TakeDamage(float damage)
    {
        if (blackboard.currentHealth <= 0) return; // ������

        blackboard.currentHealth -= damage;
        blackboard.currentHealth = Mathf.Max(blackboard.currentHealth, 0);

        Debug.Log($"Enemy took {damage} damage! Health: {blackboard.currentHealth}/{blackboard.maxHealth}");

        // ���Ѫ�����㣬�л�������״̬
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    // ���Ի���
    void OnDrawGizmosSelected()
    {
        // ��� blackboard �Ƿ�Ϊ��
        if (blackboard == null)
            return;

        // ��� blackboard �������Ƿ����
        if (blackboard.transform == null)
            return;

        // ����׷����Χ��ȷ������Ч�� chaseDistance��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blackboard.chaseDistance);

        // ���ƹ�����Χ��ȷ������Ч�� attackDistance��
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blackboard.attackDistance);

        // ���Ƶ�ǰĿ��λ�� - ֻ������ʱ������ЧĿ��ʱ����
        if (Application.isPlaying && blackboard.targetPos != Vector2.zero)
        {
            // ��� fsm �͵�ǰ״̬
            if (fsm != null && fsm.curState != null)
            {
                // ��鵱ǰ״̬�Ƿ�Ϊ�ƶ�״̬
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