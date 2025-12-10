using System;
using System.Text;

namespace FukaMiya.Utils
{
    internal sealed class PushAndPullStateMachine<TEvent> : PullStateMachine, IPushAndPullStateMachine<TEvent> where TEvent : Enum
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

        public void Fire(TEvent eventId)
        {
            Fire(eventId.GetHashCode());
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