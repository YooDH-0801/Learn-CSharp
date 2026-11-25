using Task_TextRPG.Framework;

namespace Task_TextRPG.Core.Scene
{
    internal abstract class Scene
    {
        public Scene(SceneService stateMachine)
        {
            _stateMachine = stateMachine;
        }

        protected SceneService _stateMachine;

        public SceneService StateMachine => _stateMachine;

        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void OnRender();
        public abstract void OnUpdate();
    }
}
