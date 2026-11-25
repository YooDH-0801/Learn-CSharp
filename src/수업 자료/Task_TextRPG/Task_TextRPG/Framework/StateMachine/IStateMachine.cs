namespace Task_TextRPG.Framework
{
    internal interface IStateMachine<TState>
    {
        public TState CurrentState { get; }

        public void ChangeState(TState newState);

        public void OnRender();
        public void OnUpdate();
    }
}
