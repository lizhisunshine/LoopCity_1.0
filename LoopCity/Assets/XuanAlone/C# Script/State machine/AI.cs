using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//���ȣ���Ҫ����ö�ٱ���������/�ƶ�/����/����
public enum StateType
{
    Idle,//����
    Move,//�ƶ�
    Attack,//����
    Die,//����
}


public class AI : MonoBehaviour
{
    public StateType curState;//��ǰ״̬����

    void Start()
    {
        curState = StateType.Idle;//���ó�ʼ״̬Ϊ����״̬
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

    void OnStandBy()//����״̬����
    {

    }
    void OnMove()//�ƶ�״̬����
    {

    }
    void OnAttack()//����״̬����
    {

    }
    void OnDie()//����״̬����
    {

    }



}
