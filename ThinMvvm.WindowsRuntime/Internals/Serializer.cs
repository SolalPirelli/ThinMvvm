// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ThinMvvm.WindowsRuntime.Internals
{
    /// <summary>
    /// Serializes classes to and from strings, for storage in Windows Runtime settings.
    /// </summary>
    internal static class Serializer
    {
        // DO NOT CHANGE
        private static readonly Encoding Encoding = Encoding.UTF8;


        /// <summary>
        /// Serializes the specified object into a string.
        /// </summary>
        public static string Serialize( object value )
        {
            if ( value == null )
            {
                return null;
            }

            using ( var stream = new MemoryStream() )
            {
                new DataContractJsonSerializer( value.GetType() ).WriteObject( stream, value );
                return Encoding.GetString( stream.ToArray(), 0, (int) stream.Length );
            }
        }

        /// <summary>
        /// Deserializes the specified object type from a string.
        /// </summary>
        public static T Deserialize<T>( string serialized )
        {
            if ( serialized == null )
            {
                return default( T );
            }

            byte[] serializedBytes = Encoding.GetBytes( serialized );
            using ( var stream = new MemoryStream( serializedBytes ) )
            {
                return (T) new DataContractJsonSerializer( typeof( T ) ).ReadObject( stream );
            }
        }
    }
}