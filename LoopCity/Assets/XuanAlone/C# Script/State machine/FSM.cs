using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MY_FSM
{
    //���ȣ���Ҫ����ö�ٱ���������/�ƶ�/����/����
    public enum StateType
    {
        Idle,//����
        Move,//�ƶ�
        Attack,//����
        Die,//����
        Chase, // ׷��
        Exxplode,
        Dash,
        Shoot,
        ShooterMove,
        Explode,

    }

    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        //void OnCheck();
        //void OnFixUpdate();
    }

    [Serializable]
    public class Blackboard
    {
        //�˴��洢�������ݣ�������չʾ�����ݣ������õ�����
    }
    public class FSM
    {
        public IState curState;
        public Dictionary<StateType, IState> states;
        public Blackboard blackboard;

        // ��� prevState ֧��
        public IState prevState;
        public StateType prevStateType;
        internal StateType curStateType;

        public FSM(Blackboard blackboard)
        {
            this.states = new Dictionary<StateType, IState>();
            this.blackboard = blackboard;
        }

        public void AddState(StateType stateType, IState state)
        {
            if (states.ContainsKey(stateType))
            {
                Debug.Log("[AddState] >>>>>>>>>> map has contain key:" + stateType);
                return;
            }
            states.Add(stateType, state);
        }

        public void SwitchState(StateType stateType)
        {
            if (!states.ContainsKey(stateType))
            {
                Debug.Log("[SwitchState] >>>>>>>>>> not contain key:" + stateType);
                return;
            }

            // ��¼ǰһ��״̬
            if (curState != null)
            {
                prevState = curState;
                foreach (var kvp in states)
                {
                    if (kvp.Value == curState)
                    {
                        prevStateType = kvp.Key;
                        break;
                    }
                }
            }

            if (curState != null)
            {
                curState.OnExit();
            }
            curState = states[stateType];
            curState.OnEnter();
        }

        public void OnUpdate()
        {
            curState?.OnUpdate();
        }

        internal void OnCheck()
        {
            throw new NotImplementedException();
        }

        internal void OnFixUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
