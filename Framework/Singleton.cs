namespace Hedra.Framework
{
    public class Singleton<T> where T : class, new()
    {
        protected Singleton()
        {   
        }
        
        private static T _instance;       
        public static T Instance => _instance ?? (_instance = new T());
    }
}