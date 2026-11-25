using Task_TextRPG.Framework;

namespace Task_TextRPG.Core.Scene
{
    internal sealed class SceneService : IService
    {
        private Scene? _currentScene;
        public Scene? CurrentScene => _currentScene;

        public void ChangeScene(Scene newScene)
        {
            if (_currentScene != null)
                _currentScene.OnExit();

            _currentScene = newScene;
            _currentScene.OnEnter();
        }

        public void OnRender()
        {
            _currentScene?.OnRender();
        }

        public void OnUpdate()
        {
            _currentScene?.OnUpdate();
        }
    }
}
