// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Text;
using ThinMvvm.WindowsRuntime.Internals;
using Windows.Storage;

namespace ThinMvvm.WindowsRuntime
{
    /// <summary>
    /// <see cref="ISettingsStorage" /> implementation for the Windows Runtime.
    /// </summary>
    public sealed class WindowsRuntimeSettingsStorage : ISettingsStorage
    {
        // HACK: Since the maximum size of settings is 8 KB, we need to split larger ones.
        //       Unfortunately, we can't just split strings after a certain number of bytes 
        //       since it could split multi-byte characters in two. (the JsonDataContractSerializer does not escape non-ASCII chars)
        //       The maximum character length is 4 bytes.
        private const int MaximumSettingLength = 2048;

        private readonly ApplicationDataContainer _storage = ApplicationData.Current.LocalSettings;
        private readonly ApplicationDataContainer _sizeStorage = ApplicationData.Current.LocalSettings.CreateContainer( "Sizes", ApplicationDataCreateDisposition.Always );

        /// <summary>
        /// Gets a value indicating whether the setting with the specified key is defined.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <returns>A value indicating whether the setting with the specified key is defined.</returns>
        public bool IsDefined( string key )
        {
            return _sizeStorage.Values.ContainsKey( key );
        }

        /// <summary>
        /// Gets the value of the setting with the specified key, as an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the setting value.</typeparam>
        /// <param name="key">The setting key.</param>
        /// <returns>The setting value.</returns>
        public T Get<T>( string key )
        {
            if ( IsDefined( key ) )
            {
                int chunkCount = (int) _sizeStorage.Values[key];

                if ( chunkCount == 0 )
                {
                    return default( T );
                }

                var serializedValueBuilder = new StringBuilder();
                for ( int n = 0; n < chunkCount; n++ )
                {
                    serializedValueBuilder.Append( _storage.Values[GetChunkKey( key, n )] );
                }
                return Serializer.Deserialize<T>( serializedValueBuilder.ToString() );
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Sets the value of the setting with the specified key.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The value.</param>
        public void Set( string key, object value )
        {
            string serializedValue = Serializer.Serialize( value );
            var chunks = SplitInChunks( serializedValue, MaximumSettingLength );

            for ( int n = 0; n < chunks.Length; n++ )
            {
                _storage.Values[GetChunkKey( key, n )] = chunks[n];
            }
            _sizeStorage.Values[key] = chunks.Length;
        }


        /// <summary>
        /// Splits the specified string in chunks of the specified size.
        /// </summary>
        private static string[] SplitInChunks( string str, int chunkSize )
        {
            if ( str == null )
            {
                return new string[0];
            }

            int chunkCount = (int) Math.Ceiling( str.Length / (double) chunkSize );
            string[] chunks = new string[chunkCount];
            for ( int n = 0; n < chunkCount; n++ )
            {
                int begin = n * chunkSize;
                chunks[n] = str.Substring( begin, Math.Min( chunkSize, str.Length - begin ) );
            }
            return chunks;
        }

        /// <summary>
        /// Gets the setting key for the chunk of the specified index and original key.
        /// </summary>
        /// <remarks>
        /// Note that all settings will be stored as chunks using this syntax, even the ones that only need one chunk.
        /// </remarks>
        private static string GetChunkKey( string key, int chunkIndex )
        {
            return key + "_" + chunkIndex;
        }
    }
}