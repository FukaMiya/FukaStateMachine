using System;
using System.Collections.Generic;

namespace FukaMiya.Utils
{
    public sealed class TransitionBuilder
    {
        private  readonly StateMachine StateMachine;
        private State fromState;
        private State toState;
        private ICondition condition;

        public TransitionBuilder(StateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public TransitionBuilder From<T>() where T : State, new()
        {
            fromState = StateMachine.At<T>();
            return this;
        }
        public TransitionBuilder From(State from)
        {
            fromState = from;
            return this;
        }

        public TransitionBuilder To<T>() where T : State, new()
        {
            toState = StateMachine.At<T>();
            return this;
        }
        public TransitionBuilder To(State to)
        {
            toState = to;
            return this;
        }

        public TransitionBuilder When(Func<bool> condition) => When(new FuncCondition(condition));
        public TransitionBuilder When(ICondition condition)
        {
            this.condition = condition;
            return this;
        }
        
        public TransitionBuilder And(Func<bool> condition) => And(new FuncCondition(condition));
        public TransitionBuilder And(ICondition condition)
        {
            this.condition = this.condition == null ? condition : new AndCondition(this.condition, condition);
            return this;
        }

        public TransitionBuilder Or(Func<bool> condition) => Or(new FuncCondition(condition));
        public TransitionBuilder Or(ICondition condition)
        {
            this.condition = this.condition == null ? condition : new OrCondition(this.condition, condition);
            return this;
        }

        public Transition Build()
        {
            var transition = new Transition(fromState, toState);
            transition.AddConditions(condition);
            fromState.AddTransition(transition);
            return transition;
        }

        public static implicit operator Transition(TransitionBuilder builder)
        {
            return builder.Build();
        }
    }

    public interface ICondition
    {
        bool Evaluate();
    }

    public class FuncCondition : ICondition
    {
        private readonly Func<bool> func;
        public FuncCondition(Func<bool> func) => this.func = func;
        public bool Evaluate() => func();
    }

    public class AndCondition : ICondition
    {
        private readonly ICondition a;
        private readonly ICondition b;
        public AndCondition(ICondition a, ICondition b) { this.a = a; this.b = b; }
        public bool Evaluate() => a.Evaluate() && b.Evaluate();
    }

    public class OrCondition : ICondition
    {
        private readonly ICondition a;
        private readonly ICondition b;
        public OrCondition(ICondition a, ICondition b) { this.a = a; this.b = b; }
        public bool Evaluate() => a.Evaluate() || b.Evaluate();
    }

    public sealed class Transition : IEquatable<Transition>
    {
        public State From { get; }
        public State To { get; }
        public float Weight { get; }
        public ICondition Condition { get; private set; }

        public Transition(State from, State to, float weight = 1f)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        public Transition AddConditions(ICondition condition)
        {
            Condition = condition;
            return this;
        }

        public bool Equals(Transition other)
        {
            if (other is null) return false;
            return From == other.From && To == other.To;
        }
    }

    public static class Condition
    {
        public static ICondition Any(Func<bool> a, Func<bool> b)
        {
            return new OrCondition(new FuncCondition(a), new FuncCondition(b));
        }

        public static ICondition Any(params Func<bool>[] conditions)
        {
            return new FuncCondition(() =>
            {
                foreach (var condition in conditions)
                {
                    if (condition()) return true;
                }
                return false;
            });
        }

        public static ICondition All(Func<bool> a, Func<bool> b)
        {
            return new AndCondition(new FuncCondition(a), new FuncCondition(b));
        }

        public static ICondition All(params Func<bool>[] conditions)
        {
            return new FuncCondition(() =>
            {
                foreach (var condition in conditions)
                {
                    if (!condition()) return false;
                }
                return true;
            });
        }

        public static ICondition Not(Func<bool> condition)
        {
            return new FuncCondition(() => !condition());
        }
    }
}