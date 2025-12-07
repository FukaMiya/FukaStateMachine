using System;
using System.Collections.Generic;
using System.Text;

namespace FukaMiya.Utils
{
    public sealed class StateMachine
    {
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }

        private readonly StateFactory stateFactory;
        public AnyState AnyState { get; }

        public StateMachine(StateFactory factory)
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

            if (AnyState.CheckTransitionTo(out var nextState) ||
                CurrentState.CheckTransitionTo(out nextState))
            {
                ChangeState(nextState);
                return;
            }

            CurrentState.Update();
        }

        public void SetInitialState<T>() where T : State, new()
        {
            CurrentState = At<T>();
            CurrentState.Enter();
        }

        public State At<T>() where T : State, new()
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
                    sb.AppendLine($"    {state} --> {(toState == null ? "AnyState" : toState.ToString())}");
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

    public sealed class StateFactory
    {
        private readonly Func<Type, State> factoryMethod;
        private readonly Dictionary<Type, State> stateCache = new();
        public IReadOnlyList<State> CachedStates => new List<State>(stateCache.Values);

        public StateFactory(Func<Type, State> factoryMethod)
        {
            this.factoryMethod = factoryMethod;
        }

        public State CreateState<T>() where T : State, new() => CreateState(typeof(T));
        public State CreateState(Type stateType)
        {
            if (stateCache.TryGetValue(stateType, out var cachedState))
            {
                return cachedState;
            }

            var newState = factoryMethod(stateType);
            stateCache[stateType] = newState;
            return newState;
        }

        public void ClearCache() => stateCache.Clear();
    }
}
