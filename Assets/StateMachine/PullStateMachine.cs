using System;
using System.Text;

namespace FukaMiya.Utils
{
    internal class PullStateMachine : IPullStateMachine
    {
        public State CurrentState { get; protected set; }
        public State PreviousState { get; protected set; }
        public AnyState AnyState { get; protected set; }
        private readonly StateFactory stateFactory;

        public PullStateMachine(StateFactory factory)
        {
            stateFactory = factory;
            AnyState = new AnyState();
            AnyState.SetStateMachine(this);
        }

        public void Update()
        {
            if (CurrentState == null)
            {
                throw new InvalidOperationException("CurrentState is not set. Please set the initial state using SetInitialState<T>() method.");
            }

            if (AnyState.CheckTransitionTo(-1, out var nextState) ||
                CurrentState.CheckTransitionTo(-1, out nextState))
            {
                ChangeState(nextState);
                return;
            }

            CurrentState.Update();
        }

        public void SetInitialState<T>() where T : State
        {
            CurrentState = At<T>();
            CurrentState.Enter();
        }

        public State At<T>() where T : State
        {
            if (typeof(T) == typeof(AnyState))
            {
                return AnyState;
            }

            var state = stateFactory.CreateState<T>();
            state.SetStateMachine(this);
            return state;
        }

        public string ToMermaidString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("stateDiagram-v2");
            foreach (var state in stateFactory.CachedStates)
            {
                foreach (var t in state.GetTransitions)
                {
                    var toState = t.GetToState();
                    var transitionName = string.IsNullOrEmpty(t.Name) ? (toState == null ? "AnyState" : toState.ToString()) : t.Name;
                    sb.AppendLine($"    {state} --> {transitionName}");
                }
            }
            return sb.ToString();
        }

        void ChangeState(State nextState)
        {
            CurrentState.Exit();
            PreviousState = CurrentState;
            CurrentState = nextState;
            CurrentState.Enter();
        }
    }
}
