using System;
using System.Collections.Generic;
using System.Text;

namespace FukaMiya.Utils
{
    public sealed class StateMachine
    {
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }

        private readonly Dictionary<Type, State> states = new();
        public AnyState AnyState { get; }

        public StateMachine()
        {
            AnyState = new AnyState();
            AnyState.Setup(this);
            states[typeof(AnyState)] = AnyState;
        }

        public void Update()
        {
            if (CurrentState == null)
            {
                throw new InvalidOperationException("CurrentState is not set. Please set the initial state using SetInitialState<T>() method.");
            }

            if (AnyState.CheckTransitionTo(out var nextState) ||
                CurrentState.CheckTransitionTo(out nextState))
            {
                ChangeState(nextState);
                return;
            }

            CurrentState.Update();
        }

        void ChangeState(State nextState)
        {
            CurrentState.Exit();
            PreviousState = CurrentState;
            CurrentState = nextState;
            CurrentState.Enter();
        }

        public void SetInitialState<T>() where T : State, new()
        {
            CurrentState = At<T>();
            CurrentState.Enter();
        }

        public State At<T>() where T : State, new()
        {
            if (states.TryGetValue(typeof(T), out var state))
            {
                return state;
            }

            state = CreateStateInstance<T>();
            states[typeof(T)] = state;
            return state;
        }

        public State At<T, C>(C context) where T : State<C>, new()
        {
            if (states.TryGetValue(typeof(T), out var state))
            {
                if (state is State<C> typedState)
                {
                    typedState.UpdateContext(context);
                }
                return state;
            }

            state = CreateStateInstance<T, C>(context);
            states[typeof(T)] = state;
            return state;
        }

        State CreateStateInstance<T>() where T : State, new()
        {
            T instance = new ();
            instance.Setup(this);
            return instance;
        }

        State CreateStateInstance<T, C>(C context) where T : State<C>, new()
        {
            T instance = new ();
            instance.Setup(this, context);
            return instance;
        }

        public string ToMermaidString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("stateDiagram-v2");
            foreach (var state in states.Values)
            {
                foreach (var t in state.GetTransitions)
                {
                    var toState = t.GetToState();
                    sb.AppendLine($"    {state} --> {(toState == null ? "AnyState" : toState.ToString())}");
                }
            }
            return sb.ToString();
        }
    }
}
