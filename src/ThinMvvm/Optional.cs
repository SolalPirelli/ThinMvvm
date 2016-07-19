using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ThinMvvm
{
    /// <summary>
    /// Represents an optional value.
    /// </summary>
    /// <typeparam name="T">The value's type.</typeparam>
    [DataContract]
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        [DataMember]
        private readonly T _value;

        /// <summary>
        /// Indicates whether there is a value.
        /// </summary>
        [DataMember]
        public bool HasValue { get; private set; }

        /// <summary>
        /// Gets the value, if there is any.
        /// This property will throw an <see cref="InvalidOperationException" /> if there is no value.
        /// </summary>
        public T Value
        {
            get
            {
                if( !HasValue )
                {
                    throw new InvalidOperationException( "Cannot get an optional's value if there is none." );
                }
                return _value;
            }
        }


        // The default constructor will set HasValue to false, which is exactly what we need

        /// <summary>
        /// Initializes a new instance of the <see cref="Optional{T}" /> class with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public Optional( T value )
        {
            _value = value;
            HasValue = true;
        }


        public static bool operator ==( Optional<T> left, Optional<T> right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( Optional<T> left, Optional<T> right )
        {
            return !( left == right );
        }

        /// <summary>
        /// Indicates whether the current object is equal to the specified object.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>A value indicating whether the current object is equal to the other object.</returns>
        public bool Equals( Optional<T> other )
        {
            if( HasValue )
            {
                return other.HasValue && EqualityComparer<T>.Default.Equals( Value, other.Value );
            }

            return !other.HasValue;
        }

        /// <summary>
        /// Indicates whether the current object is equal to the specified object.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>A value indicating whether the current object is equal to the other object.</returns>
        public override bool Equals( object obj )
        {
            var other = obj as Optional<T>?;
            return other.HasValue && Equals( other.Value );
        }

        /// <summary>
        /// Gets a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the object.</returns>
        public override int GetHashCode()
        {
            if( HasValue )
            {
                if( Value == null )
                {
                    return 1;
                }

                return Value.GetHashCode();
            }

            return 0;
        }

        /// <summary>
        /// Gets a string that represents the object.
        /// </summary>
        /// <returns>A string that represents the object.</returns>
        public override string ToString()
        {
            if( HasValue )
            {
                if( Value == null )
                {
                    return "(null)";
                }

                return "(value: " + Value.ToString() + ")";
            }

            return "(no value)";
        }
    }
}