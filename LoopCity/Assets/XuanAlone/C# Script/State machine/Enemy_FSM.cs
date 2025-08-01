using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyBlackboard : Blackboard
{
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

    public void OnEnter() { }

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

        // �������ڹ�����Χ�ڣ��л�������״̬����ѡ��
        /*
        if (distanceToPlayer <= blackboard.attackDistance)
        {
            fsm.SwitchState(MY_FSM.StateType.Attack);
            return;
        }
        */

        // ������ƶ�
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
        // ��ʼ���ڰ�����
        if (blackboard == null)
            blackboard = new EnemyBlackboard();

        blackboard.transform = transform;
        blackboard.currentHealth = blackboard.maxHealth;

        // �ؼ��޸ģ�������ײ��
        gameObject.layer = LayerMask.NameToLayer("Enemy");


        // �������
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            blackboard.playerTransform = player.transform;

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
        fsm.OnUpdate();
        Flip();
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