using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MY_FSM;
using System;
using Random = UnityEngine.Random;


[Serializable]
public class EnemyBlackbroad : Blackboard
{
    public float idleTime;
    public float moveSpeed;
    public Transform transform;

    public Vector2 targetPos;
}

//待机逻辑
#region 待机逻辑
public class Enemy_IdleState : IState
{
    private float idleTimer;
    private FSM fsm;
    private EnemyBlackbroad blackboard;
    public Enemy_IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackbroad;
    }

    public void OnEnter()
    {
        idleTimer = 0;
    }

    public void OnExit()
    {

    }


    public void OnUpdate()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer > blackboard.idleTime)
        {
            this.fsm.SwitchState((MY_FSM.StateType)StateType.Move);
        }
    }
}
#endregion

//移动逻辑
#region 移动逻辑
public class Player_MoveState : IState
{
    private FSM fsm;
    private EnemyBlackbroad blackboard;
    private Vector2 targetPos;
    public Player_MoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackboard = fsm.blackboard as EnemyBlackbroad;
    }

    public void OnEnter()
    {
        float randomX = Random.Range(-10, 10);
        float randomY = Random.Range(-10, 10);
        //从黑板中获取当前位置
        blackboard.targetPos = new Vector2(blackboard.transform.position.x + randomX, blackboard.transform.position.y);
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {
        if (Vector2.Distance(blackboard.transform.position, blackboard.targetPos) < 0.1f)
        {
            fsm.SwitchState((MY_FSM.StateType)StateType.Idle);
        }
        else
        {
            blackboard.transform.position = Vector2.MoveTowards(blackboard.transform.position, targetPos, blackboard.moveSpeed * Time.deltaTime);
        }
    }
}
#endregion
public class Enemy_FSM : MonoBehaviour
{
    private FSM fsm;
    public EnemyBlackbroad blackboard;

    void Start()
    {
        fsm = new FSM(blackboard);
        fsm.AddState((MY_FSM.StateType)StateType.Idle, new Enemy_IdleState(fsm));
        fsm.AddState((MY_FSM.StateType)StateType.Move, new Enemy_IdleState(fsm));
        fsm.SwitchState((MY_FSM.StateType)StateType.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnCheck();
        fsm.OnUpdate();
        Flip();
    }

    void Flip()
    {
        if (blackboard.targetPos != Vector2.zero)
        {
            if (blackboard.targetPos.x > transform.position.x)
            {
                transform.localPosition = new Vector2(-1, 1);
            }
            else
            {
                transform.localPosition = new Vector2(1, 1);
            }
        }
    }

    public void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }
}
