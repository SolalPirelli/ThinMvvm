using System;
using ThinMvvm.Infrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class ViewBinderTests
    {
        private sealed class MyViewModel : ViewModel<NoParameter> { }

        private sealed class OtherViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void CannotRegisterViewTwice()
        {
            var binder = new ViewBinder<object>();

            binder.Bind<MyViewModel, int>();

            Assert.Throws<InvalidOperationException>( () => binder.Bind<OtherViewModel, int>() );
        }

        [Fact]
        public void CannotRegisterViewModelTwice()
        {
            var binder = new ViewBinder<object>();

            binder.Bind<MyViewModel, int>();

            Assert.Throws<InvalidOperationException>( () => binder.Bind<MyViewModel, double>() );
        }

        [Fact]
        public void CannotGetViewTypeForNullViewModelType()
        {
            var registry = (IViewRegistry) new ViewBinder<object>();

            Assert.Throws<ArgumentNullException>( () => registry.GetViewType( null ) );
        }

        [Fact]
        public void CannotGetViewModelTypeForNullViewType()
        {
            var registry = (IViewRegistry) new ViewBinder<object>();

            Assert.Throws<ArgumentNullException>( () => registry.GetViewModelType( null ) );
        }

        [Fact]
        public void CannotGetUnregisteredViewType()
        {
            var registry = (IViewRegistry) new ViewBinder<object>();

            Assert.Throws<InvalidOperationException>( () => registry.GetViewType( typeof( MyViewModel ) ) );
        }

        [Fact]
        public void CannotGetUnregisteredViewModelType()
        {
            var registry = (IViewRegistry) new ViewBinder<object>();

            Assert.Throws<InvalidOperationException>( () => registry.GetViewModelType( typeof( int ) ) );
        }

        [Fact]
        public void BoundTypesAreInRegistry()
        {
            var binder = new ViewBinder<object>();

            binder.Bind<MyViewModel, int>();

            Assert.Equal( typeof( int ), ( (IViewRegistry) binder ).GetViewType( typeof( MyViewModel ) ) );
            Assert.Equal( typeof( MyViewModel ), ( (IViewRegistry) binder ).GetViewModelType( typeof( int ) ) );
        }
    }
}