using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace ThinMvvm.Tests.TestInfrastructure
{
    public static class EqualityTests
    {
        public static EqualityTests<T> For<T>( T value )
            where T : IEquatable<T>
        {
            return new EqualityTests<T>( value );
        }
    }

    public sealed class EqualityTests<T>
        where T : IEquatable<T>
    {
        private readonly T _value;
        private readonly List<T> _equalValues;
        private readonly List<T> _unequalValues;


        internal EqualityTests( T value )
        {
            _value = value;
            _equalValues = new List<T>();
            _unequalValues = new List<T>();
        }


        public EqualityTests<T> WithEqual( T item )
        {
            _equalValues.Add( item );
            return this;
        }

        public EqualityTests<T> WithUnequal( T item )
        {
            _unequalValues.Add( item );
            return this;
        }

        public void Test( bool includeOperators = true )
        {
            var equalsOp = typeof( T ).GetTypeInfo().GetDeclaredMethod( "op_Equality" );
            var unequalsOp = typeof( T ).GetTypeInfo().GetDeclaredMethod( "op_Inequality" );

            if( includeOperators )
            {
                Assert.False( equalsOp == null,
                    "The == op must be declared." );
                Assert.False( unequalsOp == null,
                    "The != op must be declared." );
            }

            Func<T, T, bool> eq = ( a, b ) =>
            {
                var eqResult = (bool) equalsOp.Invoke( null, new object[] { a, b } );
                var uneqResult = (bool) unequalsOp.Invoke( null, new object[] { a, b } );

                Assert.True( eqResult != uneqResult,
                    "== and != must not return the same result." );

                return eqResult;
            };


            Assert.False( _value.Equals( null ),
                "Equals must return false on null." );

            if( includeOperators )
            {
                Assert.True( eq( default( T ), default( T ) ),
                    "default == default must return true." );
            }


            if( !typeof( T ).GetTypeInfo().IsValueType )
            {
                Assert.False( _value.Equals( (T) (object) null ),
                    "IEquatable.Equals must return false on null." );

                if( includeOperators )
                {
                    Assert.False( eq( _value, (T) (object) null ),
                        "<non-null> == null must return false." );

                    Assert.False( eq( (T) (object) null, _value ),
                        "null == <non-null> must return false." );
                }
            }

            Assert.True( _value.Equals( _value ),
                "Equals must be reflexive." );

            if( includeOperators )
            {
                Assert.True( eq( _value, _value ),
                    "== must be reflexive." );
            }

            Assert.True( _value.GetHashCode() == _value.GetHashCode(),
                "GetHashCode must be reflexive." );


            foreach( var item in _equalValues )
            {
                Assert.True( _value.Equals( (object) item ),
                    "Equals must return true for equal items." );

                Assert.True( item.Equals( (object) item ),
                    "Equals must return true for equal items." );

                Assert.True( _value.Equals( item ),
                    "IEquatable.Equals must return true for equal items." );

                Assert.True( item.Equals( _value ),
                    "IEquatable.Equals must return true for equal items." );

                if( includeOperators )
                {
                    Assert.True( eq( _value, item ),
                        "== must return true for equal items." );

                    Assert.True( eq( item, _value ),
                        "== must return true for equal items." );
                }

                Assert.True( _value.GetHashCode() == item.GetHashCode(),
                    "GetHashCode must return the same value for equal objects." );
            }

            foreach( var item in _unequalValues )
            {
                Assert.False( _value.Equals( (object) item ),
                    "Equals must return false for unequal items." );

                Assert.False( item.Equals( (object) _value ),
                    "Equals must return false for unequal items." );

                Assert.False( _value.Equals( item ),
                    "Equals must return false for unequal items." );

                Assert.False( item.Equals( _value ),
                    "Equals must return false for unequal items." );

                if( includeOperators )
                {
                    Assert.False( eq( _value, item ),
                        "== must return false for unequal items." );

                    Assert.False( eq( item, _value ),
                        "== must return false for unequal items." );
                }
            }
        }
    }
}