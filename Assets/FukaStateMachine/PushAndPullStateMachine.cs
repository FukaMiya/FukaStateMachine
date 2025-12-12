using System;

namespace FukaStateMachine
{
    internal sealed class PushAndPullStateMachine : PullStateMachine, IPushAndPullStateMachine, IEnumTypeHolder
    {
        public PushAndPullStateMachine(StateFactory factory) : base(factory)
        {
            AnyState = new AnyState();
            AnyState.SetStateMachine(this);
        }

        public void Fire(int eventId)
        {
            if (AnyState.CheckTransitionTo(eventId, out var nextState) ||
                CurrentState.CheckTransitionTo(eventId, out nextState))
            {
                ChangeState(nextState);
                return;
            }
        }

        public Type EnumType { get; private set; }
        public void SetEnumType(Type enumType)
        {
            EnumType = enumType;
        }
    }

}