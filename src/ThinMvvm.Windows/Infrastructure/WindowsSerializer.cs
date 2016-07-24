using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Foundation;

namespace ThinMvvm.Windows.Infrastructure
{
    /// <summary>
    /// Serializes classes to and from strings, for storage in Windows settings.
    /// </summary>
    /// <remarks>
    /// Serialization uses the DataContractJsonSerializer, because it can serialize most things.
    /// The XmlSerializer is recommended for performance, but it can't even serialize DateTimeOffset.
    /// </remarks>
    public static class WindowsSerializer
    {
        /// <summary>
        /// Indicates whether the specified type is natively supported by Windows' settings storage.
        /// </summary>
        /// <param name="type">The type.</param>
        public static bool IsTypeNativelySupported( Type type )
        {
            // See https://msdn.microsoft.com/en-us/windows/uwp/app-settings/store-and-retrieve-app-data
            // - UWP does not support signed bytes.
            // - UWP's DateTime maps to .NET's DateTimeOffset.
            return type == typeof( byte )
                || type == typeof( short ) || type == typeof( ushort )
                || type == typeof( int ) || type == typeof( uint )
                || type == typeof( long ) || type == typeof( ulong )
                || type == typeof( float ) || type == typeof( double )
                || type == typeof( bool )
                || type == typeof( char )
                || type == typeof( string )
                || type == typeof( DateTimeOffset )
                || type == typeof( TimeSpan )
                || type == typeof( Guid )
                || type == typeof( Point )
                || type == typeof( Size )
                || type == typeof( Rect );
        }


        // This method MUST be generic for .NET Native to generate the serialization code automatically.
        /// <summary>
        /// Serializes the specified value into a string.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        public static string Serialize<T>( T value )
        {
            using( var stream = new MemoryStream() )
            {
                new DataContractJsonSerializer( typeof( T ) ).WriteObject( stream, value );
                return Encoding.UTF8.GetString( stream.ToArray(), 0, (int) stream.Length );
            }
        }

        /// <summary>
        /// Deserializes the specified object type from a string.
        /// </summary>
        public static T Deserialize<T>( string serialized )
        {
            return (T) Deserialize( typeof( T ), serialized );
        }

        /// <summary>
        /// Deserializes the specified object type from a string.
        /// </summary>
        public static object Deserialize( Type type, string serialized )
        {
            if( serialized == null )
            {
                return null;
            }

            var serializedBytes = Encoding.UTF8.GetBytes( serialized );
            using( var stream = new MemoryStream( serializedBytes ) )
            {
                return new DataContractJsonSerializer( type ).ReadObject( stream );
            }
        }
    }
}