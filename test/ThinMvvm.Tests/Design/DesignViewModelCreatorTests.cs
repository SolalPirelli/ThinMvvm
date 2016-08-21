using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinMvvm.DependencyInjection;
using ThinMvvm.Design;
using ThinMvvm.Logging;
using Xunit;

namespace ThinMvvm.Tests.Design
{
    public sealed class DesignViewModelCreatorTests
    {
        private interface IService { }

        private sealed class ServiceImplementation : IService { }

        private sealed class TestViewModelCreator : DesignViewModelCreator
        {
            public new TViewModel Create<TViewModel>()
                where TViewModel : ViewModel<NoParameter>
                => base.Create<TViewModel>();

            public new TViewModel Create<TViewModel, TArg>( TArg arg )
                where TViewModel : ViewModel<TArg>
                => base.Create<TViewModel, TArg>( arg );

            protected override void ConfigureServices( ServiceCollection services )
            {
                base.ConfigureServices( services );

                services.AddSingleton<IService, ServiceImplementation>();
            }
        }


        private sealed class SimpleViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void CanCreateSimpleViewModel()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<SimpleViewModel>();

            Assert.NotNull( vm );
        }



        private sealed class SimpleParameterizedViewModel : ViewModel<int>
        {
            public int Arg { get; private set; }


            public override void Initialize( int arg )
            {
                Arg = arg;
            }
        }

        [Fact]
        public void CanCreateSimpleParameterizedViewModel()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<SimpleParameterizedViewModel, int>( 42 );

            Assert.Equal( 42, vm.Arg );
        }


        private sealed class DependentViewModel : ViewModel<NoParameter>
        {
            public IService Dependency { get; }


            public DependentViewModel( IService dependency )
            {
                Dependency = dependency;
            }
        }

        [Fact]
        public void CanCreateDependentViewModel()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<DependentViewModel>();

            Assert.NotNull( vm.Dependency );
        }


        private sealed class ViewModelUsingNavigationService : ViewModel<NoParameter>
        {
            private readonly INavigationService _navigationService;


            public ViewModelUsingNavigationService( INavigationService navigationService )
            {
                _navigationService = navigationService;
            }


            public void Test()
            {
                EventHandler<NavigatedEventArgs> navigated = ( _, __ ) => Assert.True( false, "Fail." );

                _navigationService.Navigated += navigated;
                Assert.False( _navigationService.CanNavigateBack );
                _navigationService.NavigateTo<SimpleViewModel>();
                _navigationService.NavigateTo<SimpleParameterizedViewModel, int>( 42 );
                _navigationService.NavigateBack();
                _navigationService.Reset();
                Assert.False( _navigationService.RestorePreviousState() );
                _navigationService.Navigated -= navigated;
            }
        }

        [Fact]
        public void FakeNavigationServiceDoesNothing()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<ViewModelUsingNavigationService>();

            vm.Test();
        }


        private sealed class ViewModelUsingKeyValueStore : ViewModel<NoParameter>
        {
            private readonly IKeyValueStore _keyValueStore;


            public ViewModelUsingKeyValueStore( IKeyValueStore keyValueStore )
            {
                _keyValueStore = keyValueStore;
            }


            public void Test()
            {
                _keyValueStore.Set( "X", 42 );

                Assert.Equal( 42, _keyValueStore.Get<int>( "X" ).Value );

                _keyValueStore.Delete( "X" );

                Assert.False( _keyValueStore.Get<int>( "X" ).HasValue );

                _keyValueStore.Set( "Y", 42 );

                _keyValueStore.Clear();

                Assert.False( _keyValueStore.Get<int>( "Y" ).HasValue );
            }
        }

        [Fact]
        public void FakeKeyValueStoreProvidesBasicFunctionality()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<ViewModelUsingKeyValueStore>();

            vm.Test();
        }


        private sealed class ViewModelUsingDataStore : ViewModel<NoParameter>
        {
            private readonly IDataStore _dataStore;


            public ViewModelUsingDataStore( IDataStore dataStore )
            {
                _dataStore = dataStore;
            }


            public async Task TestAsync()
            {
                await _dataStore.StoreAsync( "X", 42 );

                Assert.Equal( 42, ( await _dataStore.LoadAsync<int>( "X" ) ).Value );

                await _dataStore.DeleteAsync( "X" );

                Assert.False( ( await _dataStore.LoadAsync<int>( "X" ) ).HasValue );
            }
        }

        [Fact]
        public Task FakeDataStoreProvidesBasicFunctionality()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<ViewModelUsingDataStore>();

            return vm.TestAsync();
        }


        private sealed class ViewModelUsingLogger : ViewModel<NoParameter>
        {
            private readonly ILogger _logger;


            public ViewModelUsingLogger( ILogger logger )
            {
                _logger = logger;
            }


            public void Test()
            {
                _logger.LogNavigation( "test", true );
                _logger.LogEvent( "test", "event", null );
                _logger.LogError( "error", null );
            }
        }

        [Fact]
        public void FakeLoggerDoesNothing()
        {
            var creator = new TestViewModelCreator();

            var vm = creator.Create<ViewModelUsingLogger>();

            vm.Test();
        }
    }
}