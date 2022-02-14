namespace Scrap.DependencyInjection;

public static class Singleton
{
    public static T Get<T>(Func<T> constructor) => SingletonAux<T>.Build(constructor);
    public static Task<T> GetAsync<T>(Func<Task<T>> constructor) => SingletonAsync<T>.Build(constructor);
    
    private static class SingletonAux<T>
    {
        private static T? _instance;
        private static readonly object Lock = new { type = typeof(T) };

        public static T Build(Func<T> constructor)
        {
            lock (Lock)
            {
                return (_instance ??= constructor());
            }
        }
    }

    private static class SingletonAsync<T>
    {
        private static T? _instance;

        public static async Task<T> Build(Func<Task<T>> constructor)
        {
            if (_instance == null)
            {
                _instance = await constructor();
            }

            return _instance;
        }
    }
}