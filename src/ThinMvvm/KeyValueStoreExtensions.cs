namespace ThinMvvm
{
    public static class KeyValueStoreExtensions
    {
        public static T Get<T>( this IKeyValueStore store, string key, T defaultValue )
        {
            var stored = store.Get<T>( key );
            if( stored.HasValue )
            {
                return stored.Value;
            }

            return defaultValue;
        }
    }
}