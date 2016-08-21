using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThinMvvm.Applications;
using ThinMvvm.Applications.Infrastructure;
using ThinMvvm.DependencyInjection;
using ThinMvvm.DependencyInjection.Infrastructure;
using ThinMvvm.Infrastructure;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Infrastructure
{
    public sealed class NavigationServiceBaseTests
    {
        private abstract class View
        {
            public IViewModel ViewModel { get; set; }
        }

        private sealed class SimpleView : View { }

        private sealed class MyNavigationService : NavigationServiceBase<View, int>
        {
            private readonly List<object> _args;


            public List<View> Views { get; }

            public Dictionary<string, InMemoryKeyValueStore> Stores { get; set; }

            public bool ThrowWhenSettingNavigationState { get; set; }


            public int NavigationState { get; set; }


            public MyNavigationService( ObjectCreator viewModelCreator, ViewRegistry views, TimeSpan savedStateExpirationTime )
                : base( viewModelCreator, views, savedStateExpirationTime )
            {
                Views = new List<View>();
                _args = new List<object>();

                Stores = new Dictionary<string, InMemoryKeyValueStore>();
            }


            public new void StoreState() => base.StoreState();

            public new Type GetParameterType( Type viewType ) => base.GetParameterType( viewType );


            protected override int BackStackDepth => Views.Count - 1;

            protected override View CurrentView => Views.LastOrDefault();

            protected override void NavigateTo<TArg>( Type viewType, TArg arg )
            {
                BeginNavigationAsync( NavigationKind.Forwards ).Wait();
                Views.Add( (View) Activator.CreateInstance( viewType ) );
                _args.Add( arg );
                EndNavigationAsync( NavigationKind.Forwards, arg ).Wait();
            }

            public override void NavigateBack()
            {
                BeginNavigationAsync( NavigationKind.Backwards ).Wait();
                Views.RemoveAt( Views.Count - 1 );
                _args.RemoveAt( _args.Count - 1 );
                EndNavigationAsync( NavigationKind.Backwards, _args.Last() ).Wait();
            }

            public override void Reset()
            {
                Views.Clear();
                _args.Clear();
            }

            protected override void PopBackStack()
            {
                Views.RemoveAt( Views.Count - 2 );
                _args.RemoveAt( _args.Count - 2 );
            }

            protected override int GetNavigationState()
            {
                return NavigationState;
            }

            protected override void SetNavigationState( int state )
            {
                if( ThrowWhenSettingNavigationState )
                {
                    throw new Exception( "Voluntary failure." );
                }

                NavigationState = state;
            }

            protected override IViewModel GetViewModel( View view )
            {
                return view.ViewModel;
            }

            protected override void SetViewModel( View view, IViewModel viewModel )
            {
                view.ViewModel = viewModel;
            }

            protected override IKeyValueStore GetStateStore( string name )
            {
                InMemoryKeyValueStore store;
                if( !Stores.TryGetValue( name, out store ) )
                {
                    store = new InMemoryKeyValueStore();
                    Stores[name] = store;
                }

                return store;
            }
        }

        [Fact]
        public void InitialState()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.MaxValue );

            Assert.False( ns.CanNavigateBack );
        }


        private sealed class SimpleViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void CanNavigateToSimpleViewModel()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<SimpleViewModel>();


            Assert.Equal( 1, ns.Views.Count );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[0].ViewModel );
        }

        [Fact]
        public void CanNavigateToSimpleViewModelMultipleTimes()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<SimpleViewModel>();
            ns.NavigateTo<SimpleViewModel>();
            ns.NavigateTo<SimpleViewModel>();


            Assert.Equal( 3, ns.Views.Count );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[0].ViewModel );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[1].ViewModel );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[2].ViewModel );
        }


        private sealed class SimpleView2 : View { }

        private sealed class SimpleViewModel2 : ViewModel<NoParameter> { }

        [Fact]
        public void CanNavigateToDifferentViewModels()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();
            binder.Bind<SimpleViewModel2, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<SimpleViewModel>();
            ns.NavigateTo<SimpleViewModel2>();
            ns.NavigateTo<SimpleViewModel>();


            Assert.Equal( 3, ns.Views.Count );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[0].ViewModel );
            Assert.IsType( typeof( SimpleViewModel2 ), ns.Views[1].ViewModel );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[2].ViewModel );
        }


        private sealed class ParameterizedViewModel : ViewModel<int>
        {
            public int Arg { get; private set; }

            public override void Initialize( int arg )
            {
                Arg = arg;
            }
        }

        [Fact]
        public void ViewModelsAreCreatedWithArguments()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<ParameterizedViewModel, SimpleView>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<ParameterizedViewModel, int>( 42 );


            Assert.Equal( 1, ns.Views.Count );
            Assert.Equal( 42, ( (ParameterizedViewModel) ns.Views[0].ViewModel ).Arg );
        }


        private interface IService { }

        private sealed class ServiceImplementation : IService { }

        private sealed class DependentViewModel : ViewModel<NoParameter>
        {
            public IService Dependency { get; }


            public DependentViewModel( IService dependency )
            {
                Dependency = dependency;
            }
        }

        [Fact]
        public void ViewModelsAreCreatedWithDependencies()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, ServiceImplementation>();

            var binder = new ViewBinder<View>();
            binder.Bind<DependentViewModel, SimpleView>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<DependentViewModel>();


            Assert.Equal( 1, ns.Views.Count );
            Assert.IsType( typeof( ServiceImplementation ), ( (DependentViewModel) ns.Views[0].ViewModel ).Dependency );
        }


        private sealed class TransientViewModel : ViewModel<NoParameter>
        {
            protected override bool IsTransient => true;
        }

        [Fact]
        public void TransientViewModelIsNotInBackStack()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();
            binder.Bind<TransientViewModel, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<TransientViewModel>();
            ns.NavigateTo<SimpleViewModel>();


            Assert.Equal( 1, ns.Views.Count );
            Assert.IsType( typeof( SimpleViewModel ), ns.Views[0].ViewModel );
        }


        private sealed class ViewModelTestingCanNavigateBack : ViewModel<NoParameter>
        {
            private readonly INavigationService _navigationService;


            protected override bool IsTransient => true;


            public ViewModelTestingCanNavigateBack( INavigationService navigationService )
            {
                _navigationService = navigationService;
            }


            protected override Task OnNavigatedFromAsync( NavigationKind navigationKind )
            {
                Assert.False( _navigationService.CanNavigateBack );

                return TaskEx.CompletedTask;
            }
        }

        [Fact]
        public void CanNavigateBackIsCorrectDuringNavigationFromTransientViewModel()
        {
            MyNavigationService ns = null;
            var services = new ServiceCollection();
            services.AddSingleton<INavigationService>( () => ns );

            var binder = new ViewBinder<View>();
            binder.Bind<ViewModelTestingCanNavigateBack, SimpleView>();
            binder.Bind<SimpleViewModel, SimpleView2>();

            ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );


            ns.NavigateTo<ViewModelTestingCanNavigateBack>();
            ns.NavigateTo<SimpleViewModel>();
        }


        private sealed class TrackingViewModel : ViewModel<NoParameter>
        {
            public List<string> Events { get; } = new List<string>();

            public override void Initialize( NoParameter arg )
            {
                Events.Add( nameof( Initialize ) );
            }

            protected override Task OnNavigatedToAsync( NavigationKind navigationKind )
            {
                Events.Add( nameof( OnNavigatedToAsync ) + ":" + navigationKind );

                return TaskEx.CompletedTask;
            }

            protected override Task OnNavigatedFromAsync( NavigationKind navigationKind )
            {
                Events.Add( nameof( OnNavigatedFromAsync ) + ":" + navigationKind );

                return TaskEx.CompletedTask;
            }

            public override void LoadState( IKeyValueStore store )
            {
                Events.Add( nameof( LoadState ) );
            }

            public override void SaveState( IKeyValueStore store )
            {
                Events.Add( nameof( SaveState ) );
            }
        }

        private sealed class StoreCheckingViewModel : ViewModel<NoParameter>
        {
            public override void SaveState( IKeyValueStore store )
            {
                Assert.Equal( 0, store.Get( "Value", 0 ) );
            }
        }

        [Fact]
        public void NavigationForwardsToViewModelRespectsContract()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );

            var hitCount = 0;
            ns.Navigated += ( _, e ) =>
            {
                hitCount++;
                Assert.Equal( new[]
                {
                    nameof( IViewModel.Initialize )
                }, ( (TrackingViewModel) e.Target ).Events );
            };


            ns.NavigateTo<TrackingViewModel>();


            Assert.Equal( 1, hitCount );
            Assert.Equal( new[]
            {
                nameof( IViewModel.Initialize ),
                nameof( IViewModel.OnNavigatedToAsync ) + ":" + NavigationKind.Forwards
            }, ( (TrackingViewModel) ns.Views[0].ViewModel ).Events );
        }

        [Fact]
        public void NavigationForwardsFromViewModelRespectsContract()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();
            binder.Bind<SimpleViewModel, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<TrackingViewModel>();

            var vm = (TrackingViewModel) ns.Views[0].ViewModel;
            vm.Events.Clear();

            var hitCount = 0;
            ns.Navigated += ( _, e ) =>
            {
                hitCount++;
                Assert.Equal( new[]
                {
                    nameof( IViewModel.SaveState ),
                    nameof( IViewModel.OnNavigatedFromAsync ) + ":" + NavigationKind.Forwards
                }, vm.Events );
            };


            ns.NavigateTo<SimpleViewModel>();


            Assert.Equal( 1, hitCount );
            Assert.Equal( 2, vm.Events.Count ); // should not have changed
        }

        [Fact]
        public void NavigationForwardsFromViewModelClearsStoreBeforeSavingState()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();
            binder.Bind<SimpleViewModel, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<TrackingViewModel>();

            var store = new InMemoryKeyValueStore();
            store.Set( "Value", 42 );
            ns.Stores["0"] = store;


            ns.NavigateTo<SimpleViewModel>();
        }

        [Fact]
        public void NavigationBackwardsToViewModelRespectsContract()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();
            binder.Bind<SimpleViewModel, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<TrackingViewModel>();
            ns.NavigateTo<SimpleViewModel>();

            var vm = (TrackingViewModel) ns.Views[0].ViewModel;
            vm.Events.Clear();

            var hitCount = 0;
            ns.Navigated += ( _, e ) =>
            {
                hitCount++;
                Assert.Equal( 0, ( (TrackingViewModel) e.Target ).Events.Count );
            };


            ns.NavigateBack();


            Assert.Equal( 1, hitCount );
            Assert.Equal( new[]
            {
                nameof( IViewModel.OnNavigatedToAsync ) + ":" + NavigationKind.Backwards
            }, ( (TrackingViewModel) ns.Views[0].ViewModel ).Events );
        }

        [Fact]
        public void NavigationBackwardsFromViewModelRespectsContract()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();
            binder.Bind<TrackingViewModel, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<SimpleViewModel>();
            ns.NavigateTo<TrackingViewModel>();

            var vm = (TrackingViewModel) ns.Views[1].ViewModel;
            vm.Events.Clear();
            var hitCount = 0;
            ns.Navigated += ( _, e ) =>
            {
                hitCount++;
                Assert.Equal( new[]
                {
                    nameof( IViewModel.OnNavigatedFromAsync ) + ":" + NavigationKind.Backwards
                }, vm.Events );
            };


            ns.NavigateBack();


            Assert.Equal( 1, hitCount );
            Assert.Equal( 1, vm.Events.Count ); // should not have changed
        }

        [Fact]
        public void NavigationBackwardsToNewViewModelRespectsContract()
        {
            var services = new ServiceCollection();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();
            binder.Bind<SimpleViewModel, SimpleView2>();

            var ns = new MyNavigationService( services.BuildCreator(), binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<TrackingViewModel>();
            ns.NavigateTo<SimpleViewModel>();

            ns.Views[0].ViewModel = null;

            var hitCount = 0;
            ns.Navigated += ( _, e ) =>
            {
                hitCount++;
                Assert.Equal( new[]
                {
                    nameof( IViewModel.Initialize ),
                    nameof( IViewModel.LoadState )
                }, ( (TrackingViewModel) e.Target ).Events );
            };


            ns.NavigateBack();


            Assert.Equal( 1, hitCount );
            Assert.Equal( new[]
            {
                nameof( IViewModel.Initialize ),
                nameof( IViewModel.LoadState ),
                nameof( IViewModel.OnNavigatedToAsync ) + ":" + NavigationKind.Backwards
            }, ( (TrackingViewModel) ns.Views[0].ViewModel ).Events );
        }


        [Fact]
        public void StoreStateWorksWhenCurrentViewIsNull()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.MaxValue );

            ns.NavigationState = 42;


            ns.StoreState();


            var store = ns.Stores["SuspensionStore"];
            var date = store.Get( "SuspendDate", DateTimeOffset.MinValue );

            Assert.Equal( 42, store.Get( "NavigationState", 0 ) );
            Assert.True( ( DateTimeOffset.Now - date ) < TimeSpan.FromSeconds( 1 ) );
        }

        [Fact]
        public void StoreStateWorksWhenCurrentViewModelIsNull()
        {
            var creator = new ServiceCollection().BuildCreator();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();

            var ns = new MyNavigationService( creator, binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<SimpleViewModel>();
            ns.NavigationState = 42;
            ns.Views[0].ViewModel = null;


            ns.StoreState();


            var store = ns.Stores["SuspensionStore"];
            var date = store.Get( "SuspendDate", DateTimeOffset.MinValue );

            Assert.Equal( 42, store.Get( "NavigationState", 0 ) );
            Assert.True( ( DateTimeOffset.Now - date ) < TimeSpan.FromSeconds( 1 ) );
        }

        [Fact]
        public void StoreStateSavesCurrentViewModelState()
        {
            var creator = new ServiceCollection().BuildCreator();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();

            var ns = new MyNavigationService( creator, binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<TrackingViewModel>();

            ns.NavigationState = 42;

            var store = new InMemoryKeyValueStore();
            store.Set( "Value", 42 );
            ns.Stores["0"] = store;


            ns.StoreState();
        }

        [Fact]
        public void StoreStateClearsStoreBeforeSavingState()
        {
            var creator = new ServiceCollection().BuildCreator();

            var binder = new ViewBinder<View>();
            binder.Bind<TrackingViewModel, SimpleView>();

            var ns = new MyNavigationService( creator, binder.BuildRegistry(), TimeSpan.MaxValue );

            ns.NavigateTo<TrackingViewModel>();
            ns.NavigationState = 42;
            var vm = (TrackingViewModel) ns.Views[0].ViewModel;
            vm.Events.Clear();


            ns.StoreState();


            Assert.Equal( new[]
            {
                nameof( IViewModel.SaveState )
            }, vm.Events );
        }


        [Fact]
        public void RestorePreviousStateWorks()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.MaxValue );

            var store = new InMemoryKeyValueStore();
            store.Set( "NavigationState", 42 );
            store.Set( "SuspendDate", DateTimeOffset.Now );
            ns.Stores["SuspensionStore"] = store;


            ns.RestorePreviousState();


            Assert.Equal( 42, ns.NavigationState );
        }

        [Fact]
        public void RestorePreviousStateClearsTheStoreWhenRestoringFails()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.MaxValue );
            ns.ThrowWhenSettingNavigationState = true;

            var store = new InMemoryKeyValueStore();
            store.Set( "NavigationState", 42 );
            store.Set( "SuspendDate", DateTimeOffset.Now );
            ns.Stores["SuspensionStore"] = store;


            ns.RestorePreviousState();


            Assert.Equal( 0, store.Get( "NavigationState", 0 ) );
            Assert.Equal( DateTimeOffset.MaxValue, store.Get( "SuspendDate", DateTimeOffset.MaxValue ) );
        }


        [Fact]
        public void RestorePreviousStateFailsWhenStateIsMissing()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.MaxValue );

            var store = new InMemoryKeyValueStore();
            store.Set( "SuspendDate", DateTimeOffset.Now );
            ns.Stores["SuspensionStore"] = store;


            ns.RestorePreviousState();


            Assert.Equal( 0, ns.NavigationState );
        }

        [Fact]
        public void RestorePreviousStateFailsWhenDateIsMissing()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.MaxValue );

            var store = new InMemoryKeyValueStore();
            store.Set( "NavigationState", 42 );
            ns.Stores["SuspensionStore"] = store;


            ns.RestorePreviousState();


            Assert.Equal( 0, ns.NavigationState );
        }

        [Fact]
        public void RestorePreviousStateFailsWhenStateIsTooOld()
        {
            var creator = new ServiceCollection().BuildCreator();
            var registry = new ViewBinder<View>().BuildRegistry();
            var ns = new MyNavigationService( creator, registry, TimeSpan.FromDays( 1 ) );

            var store = new InMemoryKeyValueStore();
            store.Set( "NavigationState", 42 );
            store.Set( "SuspendDate", DateTimeOffset.Now.AddDays( -2 ) );
            ns.Stores["SuspensionStore"] = store;


            ns.RestorePreviousState();


            Assert.Equal( 0, ns.NavigationState );
        }



        [Fact]
        public void GetParameterTypeWorksWithNoParameter()
        {
            var creator = new ServiceCollection().BuildCreator();

            var binder = new ViewBinder<View>();
            binder.Bind<SimpleViewModel, SimpleView>();

            var ns = new MyNavigationService( creator, binder.BuildRegistry(), TimeSpan.MaxValue );


            Assert.Equal( typeof( NoParameter ), ns.GetParameterType( typeof( SimpleView ) ) );
        }


        private sealed class IntViewModel : ViewModel<int> { }

        [Fact]
        public void GetParameterTypeWorksWithIntParameter()
        {
            var creator = new ServiceCollection().BuildCreator();

            var binder = new ViewBinder<View>();
            binder.Bind<IntViewModel, SimpleView>();

            var ns = new MyNavigationService( creator, binder.BuildRegistry(), TimeSpan.MaxValue );


            Assert.Equal( typeof( int ), ns.GetParameterType( typeof( SimpleView ) ) );
        }


        private abstract class DerivedViewModelBase<TParameter> : ViewModel<TParameter> { }

        private sealed class DerivedViewModel : DerivedViewModelBase<string> { }

        [Fact]
        public void GetParameterTypeWorksWithViewModelDerivedFromCustomGenericBase()
        {
            var creator = new ServiceCollection().BuildCreator();

            var binder = new ViewBinder<View>();
            binder.Bind<DerivedViewModel, SimpleView>();

            var ns = new MyNavigationService( creator, binder.BuildRegistry(), TimeSpan.MaxValue );


            Assert.Equal( typeof( string ), ns.GetParameterType( typeof( SimpleView ) ) );
        }
    }
}