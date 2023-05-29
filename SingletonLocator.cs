namespace BAStudio.SingletonLocator
{
    public static class SingletonLocator<T> where T : class
    {
        private static T instance;
        public static event System.Action<T> OnInstanceSet;

        public static T Instance
        {
            get
            {
                return instance;
            }

            set
            {
                if (value != null && instance != null) throw new System.Exception(string.Format("SingletonLocator<{0}>.Instance is already set.", typeof(T).Name));

                instance = value;
                OnInstanceSet?.Invoke(value);
            }
        }
    }

    public static class LockedSingletonLocator<T> where T : class
    {
        private static readonly object _lock = new object();
        private static T instance;
        public static event System.Action<T> OnInstanceSet;


        public static T Instance
        {
            get
            {
                lock (_lock) return instance;
            }

            set
            {
                lock (_lock)
                {
                    if (value != null && instance != null) throw new System.Exception(string.Format("SingletonLocator<{0}>.Instance is already set.", typeof(T).Name));

                    instance = value;
                    OnInstanceSet?.Invoke(value);
                }
            }
        }
    }
}