using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//首先，需要设立枚举变量：待机/移动/攻击/死亡
public enum StateType
{
    Idle,//待机
    Move,//移动
    Attack,//攻击
    Die,//死亡
}


public class AI : MonoBehaviour
{
    public StateType curState;//当前状态类型

    void Start()
    {
        curState = StateType.Idle;//设置初始状态为待机状态
    }

    void Update()
    {
        switch (curState)
        {
            case StateType.Idle:
                OnStandBy();
                break;

            case StateType.Move:
                OnMove();
                break;

            case StateType.Attack:
                OnAttack();
                break;

            case StateType.Die:
                OnDie();
                break;

            default:
                break;
        }

    }

    void OnStandBy()//待机状态函数
    {

    }
    void OnMove()//移动状态函数
    {

    }
    void OnAttack()//攻击状态函数
    {

    }
    void OnDie()//死亡状态函数
    {

    }



}
