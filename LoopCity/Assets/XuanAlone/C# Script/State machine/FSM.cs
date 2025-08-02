using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MY_FSM
{
    //首先，需要设立枚举变量：待机/移动/攻击/死亡
    public enum StateType
    {
        Idle,//待机
        Move,//移动
        Attack,//攻击
        Die,//死亡
        Chase, // 追踪
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
        //此处存储共享数据，或向外展示的数据，可配置的数据
    }
    public class FSM
    {
        public IState curState;
        public Dictionary<StateType, IState> states;
        public Blackboard blackboard;

        // 添加 prevState 支持
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

            // 记录前一个状态
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
