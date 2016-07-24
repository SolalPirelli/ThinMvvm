using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ThinMvvm
{
    /// <summary>
    /// Optional value.
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


        /// <summary>
        /// Indicates whether the two instances of <see cref="Optional{T}" /> are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are equal.</returns>
        public static bool operator ==( Optional<T> left, Optional<T> right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Indicates whether the two instances of <see cref="Optional{T}" /> are unequal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A value indicating whether the instances are unequal.</returns>
        public static bool operator !=( Optional<T> left, Optional<T> right )
        {
            return !( left == right );
        }

        /// <summary>
        /// Indicates whether the <see cref="Optional{T}" /> is equal to the specified <see cref="Optional{T}" />.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals( Optional<T> other )
        {
            if( HasValue )
            {
                return other.HasValue && EqualityComparer<T>.Default.Equals( Value, other.Value );
            }

            return !other.HasValue;
        }

        /// <summary>
        /// Indicates whether the <see cref="Optional{T}" /> is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            var other = obj as Optional<T>?;
            return other.HasValue && Equals( other.Value );
        }

        /// <summary>
        /// Returns the hash code of the object.
        /// </summary>
        /// <returns>The object's hash code.</returns>
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
        /// Returns the string representation of the object.
        /// </summary>
        /// <returns>The object's string representation.</returns>
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