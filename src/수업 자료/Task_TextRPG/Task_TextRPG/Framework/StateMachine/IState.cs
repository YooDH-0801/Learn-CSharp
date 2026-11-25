namespace Task_TextRPG.Framework
{
    internal interface IState<TStateMachine>
    {
        public TStateMachine StateMachine { get; }

        public void OnEnter();
        public void OnExit();
        public void OnRender();

        public void OnUpdate();
    }
}
