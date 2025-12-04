using System;
using System.Collections.Generic;

namespace FukaMiya.Utils
{
    public static class StateExtensions
    {
        public static ITransitionStarter To<T>(this State from) where T : State, new()
        {
            return TransitionBuilder.To(from, from.StateMachine.At<T>());
        }

        public static ITransitionStarter To(this State from, State to)
        {
            return TransitionBuilder.To(from, to);
        }

        public static ITransitionStarter To<T, C>(this State from, C context) where T : State<C>, new()
        {
            var toState = from.StateMachine.At<T, C>(context);
            return TransitionBuilder.To(from, toState);
        }

        public static ITransitionStarter To<C>(this State from, State to, C context)
        {
            if (to is State<C> stateWithContext)
            {
                stateWithContext.UpdateContext(context);
                return TransitionBuilder.To(from, to);
            }
            else
            {
                throw new InvalidOperationException($"The state {to.GetType().Name} is not of type State<{typeof(C).Name}>.");
            }
        }

        public static ITransitionStarter Back(this State from)
        {
            return TransitionBuilder.To(from, () => from.StateMachine.PreviousState);
        }
    }

    public abstract class State
    {
        public StateMachine StateMachine { get; private set; }
        public void Setup(StateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        private readonly List<Transition> transitions = new();
        public IReadOnlyList<Transition> GetTransitions => transitions.AsReadOnly();

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        protected virtual void OnUpdate() { }

        public void Enter()
        {
            OnEnter();
        }

        public void Exit()
        {
            OnExit();
        }

        public void Update()
        {
            OnUpdate();
        }

        public bool CheckTransitionTo(out State nextState)
        {
            State maxWeightToState = null;
            float maxWeight = float.MinValue;
            foreach (var transition in transitions)
            {
                if (transition.Condition == null || transition.Condition())
                {
                    var toState = transition.GetToState();
                    if (toState == null) continue;
                    if (!transition.Params.IsReentryAllowed && StateMachine.CurrentState.IsStateOf(toState.GetType())) continue;

                    if (maxWeightToState == null || transition.Params.Weight > maxWeight)
                    {
                        maxWeightToState = toState;
                        maxWeight = transition.Params.Weight;
                    }
                }
            }

            if (maxWeightToState != null)
            {
                nextState = maxWeightToState;
                return true;
            }

            nextState = null;
            return false;
        }

        public void AddTransition(Transition transition)
        {
            if (transitions.Contains(transition))
            {
                throw new InvalidOperationException("Transition already exists in this state.");
            }
            transitions.Add(transition);
        }

        public bool IsStateOf<T>() where T : State => this is T;
        public bool IsStateOf(Type type) => GetType() == type;

        public override string ToString() => GetType().Name;
    }

    public abstract class State<T> : State
    {
        public T Context { get; private set; }

        public void Setup(StateMachine stateMachine, T context)
        {
            UpdateContext(context);
            base.Setup(stateMachine);
        }

        public void UpdateContext(T context)
        {
            UnityEngine.Debug.Log($"Context updated: {context}");
            Context = context;
        }
    }

    public sealed class AnyState : State
    {
    }
}