using System;
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
        public void IEquatableEqualsReturnsTrueWhenBothHaveSameValue()
        {
            var optional1 = new Optional<int>( 42 );
            var optional2 = new Optional<int>( 42 );

            Assert.True( optional1.Equals( optional2 ) );
        }

        [Fact]
        public void IEquatableEqualsReturnsTrueWhenBothHaveNullValue()
        {
            var optional1 = new Optional<string>( null );
            var optional2 = new Optional<string>( null );

            Assert.True( optional1.Equals( optional2 ) );
        }

        [Fact]
        public void IEquatableEqualsReturnsTrueWhenBothLackValues()
        {
            var optional1 = new Optional<int>();
            var optional2 = new Optional<int>();

            Assert.True( optional1.Equals( optional2 ) );
        }

        [Fact]
        public void IEquatableEqualsReturnsFalseWhenOnlyFirstLacksValue()
        {
            var optional1 = new Optional<int>();
            var optional2 = new Optional<int>( 0 );

            Assert.False( optional1.Equals( optional2 ) );
        }

        [Fact]
        public void IEquatableEqualsReturnsFalseWhenOnlySecondLacksValue()
        {
            var optional1 = new Optional<int>( 0 );
            var optional2 = new Optional<int>();

            Assert.False( optional1.Equals( optional2 ) );
        }

        [Fact]
        public void IEquatableEqualsReturnsFalseWhenBothHaveDifferentValues()
        {
            var optional1 = new Optional<int>( 0 );
            var optional2 = new Optional<int>( 1 );

            Assert.False( optional1.Equals( optional2 ) );
        }

        [Fact]
        public void EqualsReturnsTrueWhenBothHaveSameValue()
        {
            var optional1 = new Optional<int>( 42 );
            var optional2 = new Optional<int>( 42 );

            Assert.True( optional1.Equals( (object) optional2 ) );
        }

        [Fact]
        public void EqualsReturnsTrueWhenBothHaveNullValue()
        {
            var optional1 = new Optional<string>( null );
            var optional2 = new Optional<string>( null );

            Assert.True( optional1.Equals( (object) optional2 ) );
        }

        [Fact]
        public void EqualsReturnsTrueWhenBothLackValues()
        {
            var optional1 = new Optional<int>();
            var optional2 = new Optional<int>();

            Assert.True( optional1.Equals( (object) optional2 ) );
        }

        [Fact]
        public void EqualsReturnsFalseWhenOnlyFirstLacksValue()
        {
            var optional1 = new Optional<int>();
            var optional2 = new Optional<int>( 0 );

            Assert.False( optional1.Equals( (object) optional2 ) );
        }

        [Fact]
        public void EqualsReturnsFalseWhenOnlySecondLacksValue()
        {
            var optional1 = new Optional<int>( 0 );
            var optional2 = new Optional<int>();

            Assert.False( optional1.Equals( (object) optional2 ) );
        }

        [Fact]
        public void EqualsReturnsFalseWhenBothHaveDifferentValues()
        {
            var optional1 = new Optional<int>( 0 );
            var optional2 = new Optional<int>( 1 );

            Assert.False( optional1.Equals( (object) optional2 ) );
        }

        [Fact]
        public void EqualsReturnsFalseWhenOtherIsOfDifferentType()
        {
            var optional = new Optional<int>( 0 );

            Assert.False( optional.Equals( "abc" ) );
        }

        [Fact]
        public void EqualsReturnsFalseWhenGivenNull()
        {
            var optional = new Optional<int>();

            Assert.False( optional.Equals( null ) );
        }

        [Fact]
        public void HashCodeIsSameWhenBothLackValues()
        {
            var optional1 = new Optional<int>();
            var optional2 = new Optional<int>();

            Assert.Equal( optional1.GetHashCode(), optional2.GetHashCode() );
        }

        [Fact]
        public void HashCodeIsSameWhenBothHaveSameValue()
        {
            var optional1 = new Optional<int>( 42 );
            var optional2 = new Optional<int>( 42 );

            Assert.Equal( optional1.GetHashCode(), optional2.GetHashCode() );
        }

        [Fact]
        public void HashCodeIsSameWhenBothHaveNullValue()
        {
            var optional1 = new Optional<string>( null );
            var optional2 = new Optional<string>( null );

            Assert.Equal( optional1.GetHashCode(), optional2.GetHashCode() );
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