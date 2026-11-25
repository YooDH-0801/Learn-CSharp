using Task_TextRPG.Core.Scene;
using Task_TextRPG.Framework;

namespace Task_TextRPG.Core
{
    internal sealed class Game : IService
    {
        public bool IsRunning { get; set; } = true;

        public void Run()
        {
            ServiceLocator.Register<Game>(this);

            var sceneService = new SceneService();
            ServiceLocator.Register<SceneService>(sceneService);

            while (IsRunning == true)
            {
                sceneService.OnRender();
                sceneService.OnUpdate();
            }

            Quit();
        }

        public void Quit()
        {
            ServiceLocator.Clear();
        }

    }
}
