using System;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Tests for <see cref="NavigatedEventArgs" />.
    /// </summary>
    public sealed class NavigatedEventArgsTests
    {
        private sealed class MyViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void CannotCreateWithInvalidKind()
        {
            Assert.Throws<ArgumentException>( () => new NavigatedEventArgs( (NavigationKind) ( -1 ), new MyViewModel() ) );
        }

        [Fact]
        public void CannotCreateWithNullTarget()
        {
            Assert.Throws<ArgumentNullException>( () => new NavigatedEventArgs( NavigationKind.Forwards, null ) );
        }

        [Fact]
        public void ConstructorSetsKind()
        {
            var args = new NavigatedEventArgs( NavigationKind.Backwards, new MyViewModel() );

            Assert.Equal( NavigationKind.Backwards, args.Kind );
        }

        [Fact]
        public void ConstructorSetsTarget()
        {
            var target = new MyViewModel();
            var args = new NavigatedEventArgs( NavigationKind.Forwards, target );

            Assert.Equal( args.Target, target );
        }
    }
}