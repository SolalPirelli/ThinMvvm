using System;
using System.Threading.Tasks;
using ThinMvvm.Infrastructure;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class ViewModelTests
    {
        private sealed class MyViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void NotTransientByDefault()
        {
            var vm = (IViewModel) new MyViewModel();

            Assert.False( vm.IsTransient );
        }

        [Fact]
        public void LoadAndSaveStateDoNothing()
        {
            var vm = new MyViewModel();
            var store = new InMemoryKeyValueStore();

            vm.SaveState( store );
            vm.LoadState( store );
        }

        [Fact]
        public void InitializeDoesNothing()
        {
            var vm = new MyViewModel();

            vm.Initialize( null );
        }

        [Fact]
        public void ExplicitlyImplementedInitializeDoesNothing()
        {
            var vm = new MyViewModel();

            ( (IViewModel) vm ).Initialize( null );
        }
        
        [Fact]
        public async Task OnNavigatedToAsyncFiresNavigatedToEvent()
        {
            var vm = (IViewModel) new MyViewModel();

            int count = 0;
            vm.NavigatedTo += ( _, __ ) => count++;

            await vm.OnNavigatedToAsync( NavigationKind.Forwards );

            Assert.Equal( 1, count );
        }

        [Fact]
        public async Task OnNavigatedFromAsyncFiresNavigatedFromEvent()
        {
            var vm = (IViewModel) new MyViewModel();

            int count = 0;
            vm.NavigatedFrom += ( _, __ ) => count++;

            await vm.OnNavigatedFromAsync( NavigationKind.Forwards );

            Assert.Equal( 1, count );
        }

        [Fact]
        public async Task CanUnregisterFromNavigatedToEvent()
        {
            var vm = (IViewModel) new MyViewModel();

            int count = 0;
            var handler = new EventHandler<EventArgs>( ( _, __ ) => count++ );
            vm.NavigatedTo += handler;
            vm.NavigatedTo -= handler;

            await vm.OnNavigatedToAsync( NavigationKind.Forwards );

            Assert.Equal( 0, count );
        }

        [Fact]
        public async Task CanUnregisterFromNavigatedFromEvent()
        {
            var vm = (IViewModel) new MyViewModel();

            int count = 0;
            var handler = new EventHandler<EventArgs>( ( _, __ ) => count++ );
            vm.NavigatedFrom += handler;
            vm.NavigatedFrom -= handler;

            await vm.OnNavigatedFromAsync( NavigationKind.Forwards );

            Assert.Equal( 0, count );
        }


        private sealed class IntViewModel : ViewModel<int>
        {
            public int Argument { get; private set; }


            public override void Initialize( int arg )
            {
                Argument = arg;
            }
        }

        [Fact]
        public void ExplicitlyImplementedInitializeCallsInitialize()
        {
            var vm = new IntViewModel();

            ( (IViewModel) vm ).Initialize( 42 );

            Assert.Equal( 42, vm.Argument );
        }


        private sealed class CountingViewModel : ViewModel<NoParameter>
        {
            public int ToCount { get; private set; }

            public int FromCount { get; private set; }


            protected override Task OnNavigatedToAsync( NavigationKind navigationKind )
            {
                ToCount++;

                return TaskEx.CompletedTask;
            }

            protected override Task OnNavigatedFromAsync( NavigationKind navigationKind )
            {
                FromCount++;

                return TaskEx.CompletedTask;
            }
        }

        [Fact]
        public async Task OnNavigatedToAsyncCallsProtectedVersion()
        {
            var vm = new CountingViewModel();

            await ( ( (IViewModel) vm ).OnNavigatedToAsync( NavigationKind.Forwards ) );

            Assert.Equal( 1, vm.ToCount );
        }

        [Fact]
        public async Task OnNavigatedFromAsyncCallsProtectedVersion()
        {
            var vm = new CountingViewModel();

            await ( ( (IViewModel) vm ).OnNavigatedFromAsync( NavigationKind.Forwards ) );

            Assert.Equal( 1, vm.FromCount );
        }
    }
}