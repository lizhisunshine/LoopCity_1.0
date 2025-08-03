using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;
using static UnityEditor.Rendering.CoreEditorDrawer<TData>;

// ���Ǻڰ�����
[Serializable]

#region  ��Һڰ�
public class PlayerBlackboard : Blackboard
{

    // ��������ֶ�
    [Header("��ǰ����")]
    public WeaponType currentWeapon;

    [Header("�������")]
    public PlayerHealth playerHealth;

    [Header("��ɫ����")]
    public float maxHealth = 100f;
    public float moveSpeed = 100f;
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
#endregion

#region  ����״̬
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
        if (blackboard.playerHealth.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }
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
#endregion

#region  �ƶ�״̬
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
        if (blackboard.playerHealth.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        // ��ȡ����
        blackboard.moveDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

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

#endregion

#region ����״̬
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

    public void OnUpdate()
    {
        if (blackboard.playerHealth.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }
    }

    private void FireBullet()
    {
        // ֻ��ħ���������ŷ����ӵ�
        if (blackboard.currentWeapon != WeaponType.MagicWand)
        {
            // ��¼����ʱ�䵫�������ӵ�
            blackboard.lastAttackTime = Time.time;
            return;
        }

        // ԭʼ�����ӵ��Ĵ��뱣�ֲ���...
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
                bulletController.direction = blackboard.aimDirection;
                bulletController.damage = blackboard.attackDamage;
                bulletController.speed = 10f;
                bulletController.isPlayerBullet = true;

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
#endregion

#region  ����״̬
// ����״̬
public class Player_DieState : IState
{
    private FSM fsm;
    private PlayerBlackboard blackboard;
    private float deathAnimationLength = 1.5f; // �����������ȣ��룩
    private float deathTimer;
    private bool animationStarted;

    public Player_DieState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as PlayerBlackboard;
    }

    public void OnEnter()
    {
        // ֹͣ�����ƶ�
        blackboard.rb.velocity = Vector2.zero;

        // ����������ײ
        if (blackboard.rb != null)
        {
            blackboard.rb.simulated = false;
        }

        // ������������
        if (blackboard.animator != null)
        {
            // ȷ��������������"Die"������
            if (HasAnimationTrigger("Die"))
            {
                blackboard.animator.SetTrigger("Die");
                animationStarted = true;

                // ��ȡ�������ȣ�������ܣ�
                deathAnimationLength = GetAnimationLength("Die");
            }
            else
            {
                Debug.LogWarning("Animator does not have 'Die' trigger");
                animationStarted = false;
            }
        }
        else
        {
            animationStarted = false;
        }

        // ������ҿ������
        if (blackboard.transform.GetComponent<Player_FSM>() != null)
        {
            blackboard.transform.GetComponent<Player_FSM>().enabled = false;
        }

        // ����������ײ��
        Collider2D[] colliders = blackboard.transform.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // ���ü�ʱ��
        deathTimer = 0f;
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        // ���¼�ʱ��
        deathTimer += Time.deltaTime;

        // ��������Ѳ�����ϣ�������Ҷ���
        if (deathTimer >= deathAnimationLength)
        {
            // ���������������Ϸ�����߼�
            GameObject.Destroy(blackboard.transform.gameObject);
        }
    }

    // ��鶯���������Ƿ���ָ���Ĵ�����
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

    // ��ȡ��������
    private float GetAnimationLength(string animationName)
    {
        if (blackboard.animator == null) return deathAnimationLength;

        RuntimeAnimatorController ac = blackboard.animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return deathAnimationLength; // Ĭ��ֵ
    }
}
#endregion

#region  ����FSM��Ϊ��
// ����״̬��������
public class Player_FSM : MonoBehaviour
{
    [Header("���״̬������")]
    public PlayerBlackboard blackboard;
    private FSM fsm;

    // ���ȱʧ���ӵ������ֶ�
    [Header("�ӵ�����")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Start()
    {
        Debug.Log("Player_FSM Start begin");

        // 1. ȷ�� PlayerHealth �������
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.Log("Adding PlayerHealth component");
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }

        // 2. ȷ���ڰ�����Ѵ���
        if (blackboard == null)
        {
            Debug.Log("Creating new PlayerBlackboard");
            blackboard = new PlayerBlackboard();
        }

        // 3. ���� transform ����
        blackboard.transform = transform;
        Debug.Log($"Transform set: {blackboard.transform != null}");

        // 4. ��ȡ Rigidbody2D ���
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("Adding Rigidbody2D component");
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        blackboard.rb = rb;
        Debug.Log($"Rigidbody2D set: {blackboard.rb != null}");

        // 5. ȷ�� firePoint ��Ч
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not set, trying to find or create one");
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                Debug.Log("Creating FirePoint child object");
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(transform);
                fpObj.transform.localPosition = new Vector3(0.5f, 0, 0); // Ĭ��λ��
                firePoint = fpObj.transform;
            }
        }
        blackboard.firePoint = firePoint;
        Debug.Log($"FirePoint set: {blackboard.firePoint != null}");

        // 6. ȷ���ӵ�Ԥ������Ч
        if (bulletPrefab == null)
        {
            Debug.LogWarning("BulletPrefab not set, trying to load default");
            bulletPrefab = Resources.Load<GameObject>("DefaultBullet");
            if (bulletPrefab == null)
            {
                Debug.LogError("Default bullet prefab not found in Resources");
                // ������ʱ�ӵ���Ϊ��
                bulletPrefab = CreateFallbackBullet();
            }
        }
        blackboard.bulletPrefab = bulletPrefab;
        Debug.Log($"BulletPrefab set: {blackboard.bulletPrefab != null}");

        // 7. ��ȡ����������
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on player");
        }
        blackboard.animator = animator;
        Debug.Log($"Animator set: {blackboard.animator != null}");

        // 8. ������ҽ������
        blackboard.playerHealth = playerHealth;
        Debug.Log($"PlayerHealth set: {blackboard.playerHealth != null}");

        // 9. ��ʼ����ǰ����ֵ
        blackboard.currentHealth = blackboard.maxHealth;
        playerHealth.currentHealth = blackboard.maxHealth; // ȷ��PlayerHealth���ͬ��
        playerHealth.maxHealth = blackboard.maxHealth;
        Debug.Log($"Current health set: {blackboard.currentHealth}");

        // 10. ��ʼ��״̬��
        fsm = new FSM(blackboard);
        Debug.Log("FSM created");

        // ������Ϸ�����
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            gameObject.layer = playerLayer;
            Debug.Log($"Layer set to Player: {playerLayer}");
        }
        else
        {
            Debug.LogWarning("Player layer not defined");
        }

        // 11. ���״̬
        try
        {
            fsm.AddState(MY_FSM.StateType.Idle, new Player_IdleState(fsm));
            fsm.AddState(MY_FSM.StateType.Move, new Player_MoveState(fsm));
            fsm.AddState(MY_FSM.StateType.Attack, new Player_AttackState(fsm));
            fsm.AddState(MY_FSM.StateType.Die, new Player_DieState(fsm));
            Debug.Log("States added to FSM");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error adding states: {ex.Message}");
        }

        // 12. ���ó�ʼ״̬
        fsm.SwitchState(MY_FSM.StateType.Idle);
        Debug.Log("Initial state set to Idle");

        // 13. ע�������¼�
        playerHealth.onDeath.AddListener(OnPlayerDeath);
        Debug.Log("Death event listener added");

        Debug.Log("Player_FSM Start completed");
    }

    // ������ʱ�ӵ���Ϊ�󱸷���
    private GameObject CreateFallbackBullet()
    {
        GameObject bullet = new GameObject("FallbackBullet");
        bullet.AddComponent<SpriteRenderer>();
        bullet.AddComponent<CircleCollider2D>().isTrigger = true;
        BulletController bc = bullet.AddComponent<BulletController>();
        bc.damage = 10f;
        bc.speed = 10f;

        return bullet;
    }

    void OnPlayerDeath()
    {
        if (fsm.curState != fsm.states[MY_FSM.StateType.Die])
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
        }
    }

    void Update()
    {
        fsm.OnUpdate();
    }

    void FixedUpdate()
    {
        // �������
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // �����˺�ֵ�������ǹ̶�10��
            EnemyBlackboard enemyBlackboard = collision.gameObject.GetComponent<Enemy_FSM>()?.blackboard;
            if (enemyBlackboard != null)
            {
                // ���ݵ������;����˺�
                float damage = 10f; // Ĭ���˺�
                switch (enemyBlackboard.enemyType)
                {
                    case EnemyBlackboard.EnemyType.Exploder:
                        damage = enemyBlackboard.explosionDamage;
                        break;
                    case EnemyBlackboard.EnemyType.Dasher:
                        damage = enemyBlackboard.dashDamage;
                        break;
                    case EnemyBlackboard.EnemyType.Shooter:
                        damage = enemyBlackboard.projectileDamage;
                        break;
                }

                blackboard.playerHealth.TakeDamage(damage);
            }
            else
            {
                // Ĭ���˺�
                blackboard.playerHealth.TakeDamage(10f);
            }
        }
    }
}
#endregion