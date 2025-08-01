using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

// ���Ǻڰ�����
[Serializable]
public class PlayerBlackboard : Blackboard
{
    [Header("��ɫ����")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float attackDamage = 20f;
    public float attackCooldown = 0.5f;

    [Header("����")]
    public Transform transform;
    public Rigidbody2D rb;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("����ʱ����")]
    public float currentHealth;
    public Vector2 moveDirection;
    public Vector2 aimDirection;
    public float lastAttackTime;

    [Header("��������")]
    public Animator animator;
    public float lastHorizontal = 1f; // Ĭ�ϳ���
    [Header("�������")]
    public float lastValidHorizontal = 1f; // Ĭ�ϳ���
    public bool facingRight = true; // ��ǰ����

    // ����������������׼����
    public void UpdateAimDirection()
    {
        // ��ȡ���λ�ò�ת��Ϊ��������
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // ���㳯�����ķ���
        aimDirection = (mousePosition - transform.position).normalized;
    }
}

// ����״̬
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
        // ʹ�������Ч����
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
        // ��ȡ����
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // ������׼���򣨹ؼ��޸�����Idle״̬ҲҪ���£�
        blackboard.UpdateAimDirection();

        // ������ƶ����룬�л����ƶ�״̬
        if (blackboard.moveDirection.magnitude > 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Move);
        }

        // ����������
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        // ����������
        if (Input.GetMouseButton(0))
        {
            // ��鹥����ȴ
            if (Time.time - blackboard.lastAttackTime > blackboard.attackCooldown)
            {
                fsm.SwitchState(MY_FSM.StateType.Attack);
            }
        }
    }
}

// �ƶ�״̬
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
        // ������� ================================
        if (blackboard.moveDirection.x != 0)
        {
            // ����ˮƽ����
            blackboard.facingRight = blackboard.moveDirection.x > 0;
            blackboard.lastValidHorizontal = blackboard.facingRight ? 1f : -1f;
        }

        // ���ö�������
        if (blackboard.animator)
        {
            // �����ƶ�ʱʹ�������Чˮƽ����
            float animHorizontal = blackboard.moveDirection.x != 0 ?
                blackboard.moveDirection.x :
                blackboard.lastValidHorizontal;

            blackboard.animator.SetFloat("Horizontal", animHorizontal);
            blackboard.animator.SetFloat("Vertical", blackboard.moveDirection.y);
            blackboard.animator.SetFloat("Speed", blackboard.moveDirection.magnitude);
        }
        // ��ȡ����
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // ������׼����
        blackboard.UpdateAimDirection();

        // Ӧ���ƶ�
        blackboard.rb.velocity = blackboard.moveDirection * blackboard.moveSpeed;

        // ���û���ƶ����룬�л�������״̬
        if (blackboard.moveDirection.magnitude < 0.1f)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
        }

        // ����������
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

// ����״̬
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
        // ʹ��ʵʱ���������׼����
        if (blackboard.animator)
        {
            // ����ˮƽ�������������Ҳ���Ϊ1�������Ϊ-1
            float horizontal = blackboard.aimDirection.x > 0 ? 1f : -1f;
            blackboard.animator.SetFloat("Horizontal", horizontal);
            blackboard.animator.SetTrigger("Attack");
        }

        // �����ӵ�
        FireBullet();

        // ��¼����ʱ��
        blackboard.lastAttackTime = Time.time;

        // �������ص���һ��״̬
        if (fsm.prevState != null)
        {
            fsm.SwitchState(fsm.prevStateType);
        }
        else
        {
            // Ĭ�Ϸ��ش���״̬
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
                // ȷ����ʵ�������������÷���
                bulletController.direction = blackboard.aimDirection;
                bulletController.damage = blackboard.attackDamage;
                bulletController.speed = 10f; // ȷ�������ٶ�ֵ
                bulletController.isPlayerBullet = true;

                // ����Ӧ�÷��򣨿�ѡ��
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

// ����״̬
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
        // �������� - ֹͣ�ƶ������ÿ��Ƶ�
        blackboard.rb.velocity = Vector2.zero;
        Debug.Log("Player died!");

        // ����������������������Ϸ�����߼���
    }

    public void OnExit() { }

    public void OnUpdate() { }
}


// ����״̬��������
public class Player_FSM : MonoBehaviour
{
    public PlayerBlackboard blackboard;
    private FSM fsm;

    [Header("�ӵ�����")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Start()
    {
        blackboard.animator = GetComponent<Animator>();

        // ��ʼ���ڰ�
        if (blackboard == null) blackboard = new PlayerBlackboard();

        // ��������
        blackboard.transform = transform;
        blackboard.rb = GetComponent<Rigidbody2D>();
        blackboard.firePoint = firePoint;
        blackboard.bulletPrefab = bulletPrefab;
        blackboard.currentHealth = blackboard.maxHealth;

        // ��ʼ��״̬��
        fsm = new FSM(blackboard);
        // �ؼ��޸ģ�������ײ��
        gameObject.layer = LayerMask.NameToLayer("Player");

        // ���״̬
        fsm.AddState(MY_FSM.StateType.Idle, new Player_IdleState(fsm));
        fsm.AddState(MY_FSM.StateType.Move, new Player_MoveState(fsm));
        fsm.AddState(MY_FSM.StateType.Attack, new Player_AttackState(fsm));
        fsm.AddState(MY_FSM.StateType.Die, new Player_DieState(fsm));

        // ��ʼ״̬
        fsm.SwitchState(MY_FSM.StateType.Idle);
    }

    void Update()
    {
        fsm.OnUpdate();
        UpdateHealth();

        // ���ԣ���ʾ��ǰ״̬
        if (fsm.curState is Player_IdleState) Debug.Log("State: Idle");
        else if (fsm.curState is Player_MoveState) Debug.Log("State: Move");
        else if (fsm.curState is Player_AttackState) Debug.Log("State: Attack");
    }

    void FixedUpdate()
    {
        // �������
    }

    // ������ײ�˺�
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(10f);
        }
    }

    // ��������ֵ
    void UpdateHealth()
    {
        if (blackboard.currentHealth <= 0 && fsm.curState != fsm.states[MY_FSM.StateType.Die])
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    // �ܵ��˺�
    public void TakeDamage(float damage)
    {
        blackboard.currentHealth -= damage;
        blackboard.currentHealth = Mathf.Clamp(blackboard.currentHealth, 0, blackboard.maxHealth);
        Debug.Log($"Player took {damage} damage! Health: {blackboard.currentHealth}/{blackboard.maxHealth}");
    }

    // ���Ի���
    
}