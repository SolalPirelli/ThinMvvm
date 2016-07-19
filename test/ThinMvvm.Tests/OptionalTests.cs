using System;
using System.IO;
using System.Runtime.Serialization.Json;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class OptionalTests
    {
        [Fact]
        public void DefaultConstructorSetsHasValue()
        {
            var optional = new Optional<int>();

            Assert.False( optional.HasValue );
        }

        [Fact]
        public void CannotGetValueIfNotPresent()
        {
            var optional = new Optional<int>();

            Assert.Throws<InvalidOperationException>( () => optional.Value );
        }

        [Fact]
        public void ConstructorSetsHasValue()
        {
            var optional = new Optional<int>( 0 );

            Assert.True( optional.HasValue );
        }

        [Fact]
        public void ConstructorSetsValue()
        {
            var optional = new Optional<int>( 42 );

            Assert.Equal( 42, optional.Value );
        }

        [Fact]
        public void CanBeDataContractSerialized()
        {
            var optional = new Optional<int>( 42 );

            var serializer = new DataContractJsonSerializer( optional.GetType() );
            var stream = new MemoryStream();

            serializer.WriteObject( stream, optional );

            stream.Seek( 0, SeekOrigin.Begin );
            var roundtrippedOptional = (Optional<int>) serializer.ReadObject( stream );

            Assert.Equal( optional, roundtrippedOptional );
        }

        [Fact]
        public void Equality()
        {
            EqualityTests.For( default( Optional<string> ) )
                .WithUnequal( new Optional<string>( null ) )
                .WithUnequal( new Optional<string>( "" ) )
                .WithUnequal( new Optional<string>( "abc" ) )
                .Test();

            EqualityTests.For( new Optional<string>( null ) )
                .WithUnequal( new Optional<string>( "" ) )
                .WithUnequal( new Optional<string>( "abc" ) )
                .Test();

            EqualityTests.For( new Optional<string>( "" ) )
                .WithUnequal( new Optional<string>( "abc" ) )
                .Test();

            EqualityTests.For( new Optional<string>( "abc" ) )
                .Test();
        }

        [Fact]
        public void ToStringReturnsWrappedValueWhenPresent()
        {
            var optional = new Optional<int>( 42 );

            Assert.Equal( "(value: 42)", optional.ToString() );
        }

        [Fact]
        public void ToStringReturnsSpecialValueWhenNoValueIsPresent()
        {
            var optional = new Optional<int>();

            Assert.Equal( "(no value)", optional.ToString() );
        }

        [Fact]
        public void ToStringReturnsSpecialValueOnNullValue()
        {
            var optional = new Optional<string>( null );

            Assert.Equal( "(null)", optional.ToString() );
        }
    }
}