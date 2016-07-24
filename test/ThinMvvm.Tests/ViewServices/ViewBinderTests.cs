using System;
using ThinMvvm.ViewServices;
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
            var registry = new ViewBinder<object>().BuildRegistry();

            Assert.Throws<ArgumentNullException>( () => registry.GetViewType( null ) );
        }

        [Fact]
        public void CannotGetViewModelTypeForNullViewType()
        {
            var registry = new ViewBinder<object>().BuildRegistry();

            Assert.Throws<ArgumentNullException>( () => registry.GetViewModelType( null ) );
        }

        [Fact]
        public void CannotGetUnregisteredViewType()
        {
            var registry =  new ViewBinder<object>().BuildRegistry();

            Assert.Throws<InvalidOperationException>( () => registry.GetViewType( typeof( MyViewModel ) ) );
        }

        [Fact]
        public void CannotGetUnregisteredViewModelType()
        {
            var registry = new ViewBinder<object>().BuildRegistry();

            Assert.Throws<InvalidOperationException>( () => registry.GetViewModelType( typeof( int ) ) );
        }

        [Fact]
        public void BoundTypesAreInRegistry()
        {
            var binder = new ViewBinder<object>();
            binder.Bind<MyViewModel, int>();

            var registry = binder.BuildRegistry();

            Assert.Equal( typeof( int ), registry.GetViewType( typeof( MyViewModel ) ) );
            Assert.Equal( typeof( MyViewModel ), registry.GetViewModelType( typeof( int ) ) );
        }
    }
}