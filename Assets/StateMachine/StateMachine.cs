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

    public sealed class StateFactory
    {
        private readonly Dictionary<Type, Func<State>> factories;
        private readonly Dictionary<Type, State> stateCache = new();
        public IReadOnlyList<State> CachedStates => new List<State>(stateCache.Values);
        public bool IsAutoCreateEnabled { get; set; } = true;

        public StateFactory()
        {
            factories = new Dictionary<Type, Func<State>>();
        }
        public StateFactory(Dictionary<Type, Func<State>> factories)
        {
            this.factories = factories;
        }

        public void Register<T>(Func<State> factory) where T : State
        {
            factories[typeof(T)] = factory;
        }

        public State CreateState<T>() where T : State
        {
            var stateType = typeof(T);
            if (stateCache.TryGetValue(stateType, out var cachedState))
            {
                return cachedState;
            }

            if (!factories.ContainsKey(stateType))
            {
                if (IsAutoCreateEnabled)
                {
                    State autoCreatedState;
                    try
                    {
                        autoCreatedState = Activator.CreateInstance(stateType) as State;
                    }
                    catch (MissingMethodException)
                    {
                        throw new InvalidOperationException($"Failed to auto-create {stateType.Name}. It requires a parameterless constructor.");
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"An error occurred while creating {stateType.Name}: {ex.Message}", ex);
                    }

                    stateCache[stateType] = autoCreatedState;
                    return autoCreatedState;
                }
                else
                {
                    throw new InvalidOperationException($"State of type {stateType.Name} is not registered in the StateFactory.");   
                }
            }

            var newState = factories[stateType]() ?? throw new InvalidOperationException($"Factory for state type {stateType.Name} returned null.");
            stateCache[stateType] = newState;
            return newState;
        }

        public void ClearCache() => stateCache.Clear();
    }
}
