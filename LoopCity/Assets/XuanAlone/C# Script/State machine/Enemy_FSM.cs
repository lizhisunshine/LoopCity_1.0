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
    [Header("��������")]
    public string moveAnimParam = "IsMoving";
    public string explodeTriggerParam = "Explode";
    public string dieTriggerParam = "Die"; // ��������������
    public bool hasExplodeAnimation = true;
    public bool hasDieAnimation = true;   // ��������������־

    [Header("��������")]
    public Animator animator;

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
    private bool hasExploded;
    private bool isDeathExplosion; // ����Ƿ�Ϊ������ը
    private float explosionTimer;
    private float explosionAnimationLength = 0.5f; // Ĭ�ϱ�ը��������

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
        Debug.Log("������ը����!");
        hasExploded = false;
        explosionTimer = 0f;

        // ֻ���ű�ը����������������
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

        // ������ײ��
        Collider2D collider = blackboard.transform.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // ֹͣ�ƶ�
        if (blackboard.transform.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // ����ִ�б�ը�˺�������������
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

        // �����������ը�����Ϊ�ѱ�ը����ִ���˺���
        if (isDeathExplosion && !hasExploded && explosionTimer > 0.1f)
        {
            hasExploded = true;
        }

        // ����������Ϻ�����
        if (explosionTimer >= explosionAnimationLength)
        {
            GameObject.Destroy(blackboard.transform.gameObject);
        }
    }


    // ��鶯���������Ƿ����
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

    // ����Ƿ����ض����ƵĶ���״̬
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

    // ��ȡ��������
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
#endregion#region  ��̹�

#region  ��̹�
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

        // ������̶���
        if (blackboard.animator != null)
        {
            // ����1: ʹ�ò���ֵ
            if (HasAnimationParameter("IsDashing"))
            {
                blackboard.animator.SetBool("IsDashing", true);
            }
            // ����2: ʹ�ô�����
            else if (HasAnimationTrigger("Dash"))
            {
                blackboard.animator.SetTrigger("Dash");
            }
            // ����3: ֱ�Ӳ��Ŷ���״̬
            else
            {
                // ȷ���� "Dash" ״̬
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

        // ���ö���״̬
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
        // ����ƶ�
        blackboard.transform.Translate(
            dashDirection * blackboard.chaseSpeed * 5 * Time.deltaTime,
            Space.World
        );

        // ����Ƿ�ﵽ��̾���
        float dashDistance = Vector2.Distance(dashStartPosition, blackboard.transform.position);
        if (dashDistance >= blackboard.dashDistance)
        {
            // ��̽�����ص�Idle״̬
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // �����ײ���
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

                    // ������ײ���������
                    Collider2D collider = blackboard.transform.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = false;

                    // ��̹�������Һ���������
                    GameObject.Destroy(blackboard.transform.gameObject);
                }
            }
        }
    }

    // ����Ƿ����ض����ƵĶ���״̬
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

    // ��鶯�������Ƿ����
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

    // ��鶯���������Ƿ����
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
        // ������� - ������ǰ��
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
        // ������� - ������ǰ��
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

        // �����ҳ���׷����Χ���ص�Idle
        if (distanceToPlayer > blackboard.chaseDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Idle);
            return;
        }

        // �����Ҳ��ڹ�����Χ�ڣ��л���׷��״̬
        if (distanceToPlayer < blackboard.minRange || distanceToPlayer > blackboard.maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.Chase);
            return;
        }

        // �ڹ�����Χ�ڣ�ֱ�ӹ���
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
    private float deathTimer;
    private bool deathAnimationStarted;
    private float deathAnimationLength = 1.0f; // Ĭ��������������

    public Enemy_DieState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackboard;
    }

    public void OnEnter()
    {
        Debug.Log("��������!");
        deathTimer = 0f;
        deathAnimationStarted = false;

        // ������ײ��
        Collider2D collider = blackboard.transform.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // ֹͣ�ƶ�
        if (blackboard.transform.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // ������������
        PlayDeathAnimation();
    }

    private void PlayDeathAnimation()
    {
        // ����ʹ�����õ�����������
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

        // ���û�������ض�������������ͨ�÷���
        if (!deathAnimationStarted && blackboard.animator != null)
        {
            // ����ʹ��"Die"������
            if (HasAnimationTrigger("Die"))
            {
                blackboard.animator.SetTrigger("Die");
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength("Die");
            }
            // ���Բ���"Die"����״̬
            else if (HasAnimationState("Die"))
            {
                blackboard.animator.Play("Die");
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength("Die");
            }
            // ���Բ���"Death"����״̬
            else if (HasAnimationState("Death"))
            {
                blackboard.animator.Play("Death");
                deathAnimationStarted = true;
                deathAnimationLength = GetAnimationLength("Death");
            }
        }

        if (!deathAnimationStarted)
        {
            Debug.LogWarning("û���ҵ�������������ֱ�����ٵ���");
            GameObject.Destroy(blackboard.transform.gameObject);
        }
    }

    public void OnExit() { }

    public void OnUpdate()
    {
        if (deathAnimationStarted)
        {
            deathTimer += Time.deltaTime;

            // ��������������
            if (deathTimer >= deathAnimationLength)
            {
                GameObject.Destroy(blackboard.transform.gameObject);
            }
        }
    }

    // ��鶯���������Ƿ����
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

    // ����Ƿ����ض����ƵĶ���״̬
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

    // ��ȡ��������
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

        // �����ƶ�����Ϊ false

        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, false);
        }

        // ����Ĭ�ϵ�Idle����
        if (blackboard.animator != null)
        {
            // ȷ�������������Ѿ���ʼ��
            if (blackboard.animator.runtimeAnimatorController != null)
            {
                // ����Ƿ��� "IsMoving" ����
                if (HasAnimationParameter("IsMoving"))
                {
                    blackboard.animator.SetBool("IsMoving", false);
                }

                // ����Ƿ��� "IsDashing" ����
                if (HasAnimationParameter("IsDashing"))
                {
                    blackboard.animator.SetBool("IsDashing", false);
                }

                // ǿ�Ʋ���Idle����
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
        // ����Ƿ�����
        if (blackboard.currentHealth <= 0)
        {
            fsm.SwitchState(MY_FSM.StateType.Die);
            return;
        }

        if (blackboard.playerTransform == null) return;

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

        // ȷ������״̬��ȷ
        EnsureAnimationState();
    }

    // ȷ����������ȷ״̬
    private void EnsureAnimationState()
    {
        if (blackboard.animator == null ||
            blackboard.animator.runtimeAnimatorController == null)
            return;

        // ��ȡ��ǰ״̬��Ϣ
        var stateInfo = blackboard.animator.GetCurrentAnimatorStateInfo(0);

        // ����Ƿ���Idle״̬
        if (!stateInfo.IsName("Idle"))
        {
            // �����л���Idle״̬
            PlayAnimationIfExists("Idle");
        }
    }

    // ��鶯�������Ƿ����
    public bool HasAnimationParameter(string paramName)
    {
        if (blackboard.animator == null) return false;

        // �������в���
        foreach (var param in blackboard.animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    // ����ָ��������������ڣ�
    private void PlayAnimationIfExists(string stateName)
    {
        if (blackboard.animator == null || blackboard.animator.runtimeAnimatorController == null)
            return;

        // ��鶯��״̬�Ƿ����
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

        // ��������򲥷�
        if (stateExists)
        {
            blackboard.animator.Play(stateName, 0, 0f); // �ӿ�ʼ����
        }
        else
        {
            Debug.LogWarning($"Animation state '{stateName}' not found in animator controller");
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

        // �����ƶ�����
        if (blackboard.animator != null && HasAnimationParameter("IsMoving"))
        {
            blackboard.animator.SetBool("IsMoving", true);
        }
        // �����ƶ�����
        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, true);
        }
    }

    public void OnExit() 
    {
        // �˳�ʱ�����ƶ�����Ϊ false
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

    // ��鶯�������Ƿ����
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
        // �����ƶ�����
        if (blackboard.animator != null &&
            HasAnimationParameter(blackboard.moveAnimParam))
        {
            blackboard.animator.SetBool(blackboard.moveAnimParam, true);
        }

        if (blackboard.enemyType == EnemyBlackboard.EnemyType.Dasher)
        {
            blackboard.dashTimer = Mathf.Max(0, blackboard.dashTimer);
        }

        // �����ƶ�����
        if (blackboard.animator != null && HasAnimationParameter("IsMoving"))
        {
            blackboard.animator.SetBool("IsMoving", true);
        }
    }

    // ��鶯�������Ƿ����
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
        // �˳�ʱ�����ƶ�����Ϊ false
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
        float distanceToPlayer = Mathf.Sqrt(sqrDistanceToPlayer); // ����ʵ�ʾ���

        // �������ڹ�����Χ�ڣ�ֱ�����
        if (distanceToPlayer >= minRange && distanceToPlayer <= maxRange)
        {
            fsm.SwitchState(MY_FSM.StateType.Shoot);
        }
        else
        {
            // �������׷��
            blackboard.transform.position = Vector2.MoveTowards(
                blackboard.transform.position,
                playerPosition,
                blackboard.chaseSpeed * Time.deltaTime
            );
        }
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

        // ȷ�� animator ����ֵ
        if (blackboard.animator == null)
        {
            blackboard.animator = GetComponent<Animator>();
            if (blackboard.animator == null)
            {
                Debug.LogError("Animator component not found on enemy: " + gameObject.name);
            }
        }

        // ���������ײ���⣺���ù��������
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

    // ��鶯���������Ƿ����
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

    // ����Ƿ����ض����ƵĶ���״̬
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

        Debug.Log($"�����ܵ� {damage} �˺�! ����: {blackboard.currentHealth}/{blackboard.maxHealth}");

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