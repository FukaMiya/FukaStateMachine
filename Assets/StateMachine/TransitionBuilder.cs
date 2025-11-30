using System;

namespace FukaMiya.Utils
{
    public interface ITransitionInitializer
    {
        public ITransitionInitializer From<T>() where T : State, new();
        public ITransitionStarter To<T>() where T : State, new();
        public ITransitionStarter To(State toState);
    }

    public sealed class TransitionInitializer : ITransitionInitializer
    {
        private readonly StateMachine stateMachine;
        private readonly State fromState;

        public TransitionInitializer(StateMachine stateMachine, State fromState)
        {
            this.stateMachine = stateMachine;
            this.fromState = fromState;
        }

        public ITransitionInitializer From<T>() where T : State, new()
        {
            var newFromState = stateMachine.At<T>();
            return new TransitionInitializer(stateMachine, newFromState);
        }

        public ITransitionStarter To<T>() where T : State, new()
        {
            var toState = stateMachine.At<T>();
            return TransitionBuilder.To(fromState, toState);
        }

        public ITransitionStarter To(State toState)
        {
            return TransitionBuilder.To(fromState, toState);
        }
    }

    public interface ITransitionStarter : ITransitionParameterSetter
    {
        public ITransitionChain When(StateCondition condition);
        public Transition Always();
    }

    public interface ITransitionChain : ITransitionParameterSetter
    {
        public ITransitionChain And(StateCondition condition);
        public ITransitionChain Or(StateCondition condition);
        public Transition Build();
    }

    public interface ITransitionFinalizer : ITransitionParameterSetter
    {
        public Transition Build();
    }

    public interface ITransitionParameterSetter
    {
        public ITransitionFinalizer SetAllowReentry(bool allowReentry);
        public ITransitionFinalizer SetWeight(float weight);
    }

    public sealed class TransitionBuilder : ITransitionStarter, ITransitionChain, ITransitionFinalizer
    {
        private State fromState;
        private State fixedToState;
        private Func<State> stateProvider;
        private StateCondition condition;

        private readonly TransitionParams transitionParams = new();

        public static ITransitionStarter To(State fromState, State toState)
        {
            var instance = new TransitionBuilder
            {
                fromState = fromState,
                fixedToState = toState
            };
            return instance;
        }

        public static ITransitionStarter To(State fromState, Func<State> toStateProvider)
        {
            var instance = new TransitionBuilder
            {
                fromState = fromState,
                stateProvider = toStateProvider
            };
            return instance;
        }

        public ITransitionChain When(StateCondition condition)
        {
            this.condition = condition;
            return this;
        }

        public ITransitionChain And(StateCondition condition)
        {
            var current = this.condition;
            this.condition = () => current() && condition();
            return this;
        }

        public ITransitionChain Or(StateCondition condition)
        {
            var current = this.condition;
            this.condition = () => current() || condition();
            return this;
        }

        public ITransitionFinalizer SetAllowReentry(bool allowReentry)
        {
            transitionParams.IsReentryAllowed = allowReentry;
            return this;
        }

        public ITransitionFinalizer SetWeight(float weight)
        {
            transitionParams.Weight = weight;
            return this;
        }

        public Transition Always()
        {
            return Build();
        }

        public Transition Build()
        {
            if (fixedToState == null && stateProvider == null)
            {
                throw new InvalidOperationException("Either fixedToState or stateProvider must be set.");
            }

            Transition transition;
            if (fixedToState != null)
            {
                transition = new Transition(fixedToState);
            }
            else
            {
                transition = new Transition(stateProvider);
            }
            transition.SetCondition(condition);
            transition.SetParams(transitionParams);
            fromState.AddTransition(transition);
            return transition;
        }
    }

    public delegate bool StateCondition();

    public sealed class Transition : IEquatable<Transition>
    {
        private readonly State to;
        private readonly Func<State> stateProvider;
        public StateCondition Condition { get; private set; }

        public TransitionParams Params { get; private set;}

        public Transition(State to)
        {
            this.to = to;
        }

        public Transition(Func<State> stateProvider)
        {
            this.stateProvider = stateProvider;
        }

        public void SetCondition(StateCondition condition)
        {
            Condition = condition;
        }

        public void SetParams(TransitionParams transitionParams)
        {
            Params = transitionParams;
        }

        public State GetToState()
        {
            return stateProvider != null ? stateProvider() : to;
        }

        public bool Equals(Transition other)
        {
            if (other == null) return false;
            return to == other.to && Condition == other.Condition;
        }
    }
    
    public sealed class TransitionParams
    {
        public float Weight { get; set; } = 1f;
        public bool IsReentryAllowed { get; set; } = false;
    }

    public static class Condition
    {
        public static StateCondition Any(params StateCondition[] conditions)
        {
            return () =>
            {
                foreach (var condition in conditions)
                {
                    if (condition()) return true;
                }
                return false;
            };
        }

        public static StateCondition All(params StateCondition[] conditions)
        {
            return () =>
            {
                foreach (var condition in conditions)
                {
                    if (!condition()) return false;
                }
                return true;
            };
        }

        public static StateCondition Not(StateCondition condition)
        {
            return () => !condition();
        }
        
        // ラムダ式を明示的に変換したい場合用（基本不要だが互換性のため）
        public static StateCondition Is(Func<bool> predicate)
        {
            return new StateCondition(predicate);
        }
    }
}