namespace Task_TextRPG.Framework
{
    internal static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> ServiceDict = new Dictionary<Type, IService>();
        
        public static void Register<T>(T service) where T : class, IService
        {
            Type type = typeof(T);

            if (ServiceDict.ContainsKey(type) == true)
            {
                Debug.LogError("이미 존재하는 타입이라 등록할 수 없습니다.");
                return;
            }

            ServiceDict.Add(type, service);
        }

        public static void Unregister<T>(T service) where T : class, IService
        {
            Type type = typeof(T);

            if (ServiceDict.ContainsKey(type) == false)
            {
                Debug.LogError("해당 타입의 키가 존재하지 않아 등록 해제할 수 없습니다.");
                return;
            }

            if (ServiceDict[type] != service)
            {
                Debug.LogError("해당 타입을 해제하려면 해당 객체에서 등록 해제 시도해주세요.");
                return;
            }

            ServiceDict.Remove(type);
        }

        public static T? Get<T>() where T : class, IService
        {
            Type type = typeof(T);

            if (ServiceDict.ContainsKey(type) == false)
            {
                Debug.LogError("해당 타입의 키가 존재하지 않아 가져올 수 없습니다.");
                return null;
            }

            return ServiceDict[type] as T;
        }

        public static void Clear()
        {
            ServiceDict.Clear();
        }
    }
}
