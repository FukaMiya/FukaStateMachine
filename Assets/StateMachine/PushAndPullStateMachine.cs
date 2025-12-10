namespace FukaMiya.Utils
{
    internal sealed class PushAndPullStateMachine : PullStateMachine, IPushAndPullStateMachine
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

        void ChangeState(State nextState)
        {
            CurrentState.Exit();
            PreviousState = CurrentState;
            CurrentState = nextState;
            CurrentState.Enter();
        }
    }

}