using System;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class NavigatedEventArgsTests
    {
        private sealed class MyViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void CannotCreateWithInvalidKind()
        {
            Assert.Throws<ArgumentException>( () => new NavigatedEventArgs( new MyViewModel(), (NavigationKind) ( -1 ) ) );
        }

        [Fact]
        public void CannotCreateWithNullTarget()
        {
            Assert.Throws<ArgumentNullException>( () => new NavigatedEventArgs( null, NavigationKind.Forwards ) );
        }

        [Fact]
        public void ConstructorSetsKind()
        {
            var args = new NavigatedEventArgs( new MyViewModel(), NavigationKind.Backwards );

            Assert.Equal( NavigationKind.Backwards, args.Kind );
        }

        [Fact]
        public void ConstructorSetsTarget()
        {
            var target = new MyViewModel();
            var args = new NavigatedEventArgs( target, NavigationKind.Forwards );

            Assert.Equal( args.Target, target );
        }
    }
}