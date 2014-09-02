// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Logging.Tests
{
    [TestClass]
    public class LoggerTests
    {
        private class TestNavigationService : INavigationService
        {
            public object CurrentViewModel { get { return _viewModels.Peek(); } }

            private Stack<object> _viewModels = new Stack<object>();

            public void NavigateTo<T>() where T : ViewModel<NoParameter>
            {
                _viewModels.Push( Activator.CreateInstance<T>() );
                OnNavigated( CurrentViewModel, true );
            }

            public void NavigateTo<TViewModel, TParameter>( TParameter arg ) where TViewModel : ViewModel<TParameter>
            {
                throw new NotSupportedException();
            }

            public void NavigateBack()
            {
                _viewModels.Pop();
                OnNavigated( CurrentViewModel, false );
            }

            public void RemoveCurrentFromBackStack()
            {
                throw new NotSupportedException();
            }

            public event EventHandler<NavigatedEventArgs> Navigated;
            private void OnNavigated( object viewModel, bool isForwards )
            {
                var evt = Navigated;
                if ( evt != null )
                {
                    evt( this, new NavigatedEventArgs( viewModel, isForwards ) );
                }
            }
        }

        private class TestLogger : Logger
        {
            public List<Tuple<string, LoggedSpecialAction>> Actions { get; private set; }

            public List<Tuple<string, string, string>> Commands { get; private set; }


            public TestLogger( INavigationService navigationService )
                : base( navigationService )
            {
                Actions = new List<Tuple<string, LoggedSpecialAction>>();
                Commands = new List<Tuple<string, string, string>>();
            }

            protected override void LogAction( string viewModelId, LoggedSpecialAction action )
            {
                Actions.Add( Tuple.Create( viewModelId, action ) );
            }

            protected override void LogCommand( string viewModelId, string eventId, string label )
            {
                Commands.Add( Tuple.Create( viewModelId, eventId, label ) );
            }
        }

        [LogId( "1" )]
        private class TestViewModel1 : DataViewModel<NoParameter>
        {
            [LogId( "C1" )]
            public Command Command1
            {
                get { return this.GetCommand( () => { } ); }
            }

            [LogId( "C2" )]
            public Command Command2
            {
                get { return this.GetCommand( () => { } ); }
            }
        }

        [LogId( "2" )]
        private class TestViewModel2 : ViewModel<NoParameter>
        {
            public Tuple<string> SomeValue { get; set; }

            [LogId( "C3" )]
            public Command Command3
            {
                get { return this.GetCommand( () => { } ); }
            }

            [LogId( "C4" )]
            [LogParameter( "SomeValue.Item1" )]
            public Command Command4
            {
                get { return this.GetCommand( () => { } ); }
            }

            [LogId( "C5" )]
            [LogParameter( "$Param" )]
            public Command<string> Command5
            {
                get { return this.GetCommand<string>( _ => { } ); }
            }

            [LogId( "C6" )]
            [LogParameter( "$Param" )]
            [LogValueConverter( typeof( TestLogValueConverter ) )]
            public Command<bool> Command6
            {
                get { return this.GetCommand<bool>( _ => { } ); }
            }

            [LogId( "Refresh" )]
            public AsyncCommand RefreshCommand
            {
                get { return this.GetAsyncCommand( () => Task.FromResult( 0 ) ); }
            }

            private sealed class TestLogValueConverter : ILogValueConverter
            {
                public string Convert( object value )
                {
                    return (bool) value ? "Yes" : "No";
                }
            }
        }


        [TestMethod]
        public void NavigationIsLogged()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", LoggedSpecialAction.ForwardsNavigation ) }, logger.Actions );
        }

        [TestMethod]
        public void NavigationsAreLogged()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            nav.NavigateTo<TestViewModel2>();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", LoggedSpecialAction.ForwardsNavigation ),
                                               Tuple.Create( "2", LoggedSpecialAction.ForwardsNavigation ) }, logger.Actions );
        }

        [TestMethod]
        public void BackwardsNavigationIsLogged()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            nav.NavigateTo<TestViewModel2>();
            nav.NavigateBack();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", LoggedSpecialAction.ForwardsNavigation ),
                                               Tuple.Create( "2", LoggedSpecialAction.ForwardsNavigation ),
                                               Tuple.Create( "1", LoggedSpecialAction.BackwardsNavigation ) }, logger.Actions );
        }

        [TestMethod]
        public void CommandIsLogged()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            ( (TestViewModel1) nav.CurrentViewModel ).Command1.Execute();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", "C1", (string) null ) }, logger.Commands );
        }

        [TestMethod]
        public void CommandsAreLogged()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            ( (TestViewModel1) nav.CurrentViewModel ).Command1.Execute();
            ( (TestViewModel1) nav.CurrentViewModel ).Command2.Execute();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", "C1", (string) null ), 
                                               Tuple.Create( "1", "C2", (string) null ) }, logger.Commands );
        }

        [TestMethod]
        public void CommandIsLoggedAfterViewModelChange()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            nav.NavigateTo<TestViewModel2>();
            ( (TestViewModel2) nav.CurrentViewModel ).Command3.Execute();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "2", "C3", (string) null ) }, logger.Commands );
        }

        [TestMethod]
        public void CommandIsLoggedAfterBackwardsViewModelChange()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            nav.NavigateTo<TestViewModel2>();
            nav.NavigateBack();
            ( (TestViewModel1) nav.CurrentViewModel ).Command1.Execute();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", "C1", (string) null ) }, logger.Commands );
        }

        [TestMethod]
        public async Task RefreshCommandIsSpecialAction()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            await ( (TestViewModel1) nav.CurrentViewModel ).RefreshCommand.ExecuteAsync();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", LoggedSpecialAction.ForwardsNavigation ),
                                               Tuple.Create( "1", LoggedSpecialAction.Refresh ) }, logger.Actions );
        }

        [TestMethod]
        public void CommandLoggingRequestIsHonored()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();
            var vm2 = new TestViewModel2();

            nav.NavigateTo<TestViewModel1>();
            Messenger.Send( new CommandLoggingRequest( vm2 ) );
            vm2.Command3.Execute();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", "C3", (string) null ) }, logger.Commands );
        }

        [TestMethod]
        public void EventLogRequestIsHonored()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            Messenger.Send( new EventLogRequest( "XYZ", "123" ) );

            CollectionAssert.AreEqual( new[] { Tuple.Create( "1", "XYZ", "123" ) }, logger.Commands );
        }

        [TestMethod]
        public void EventLogRequestWithScreenIdIsHonored()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel1>();
            Messenger.Send( new EventLogRequest( "XYZ", "123", "ABC" ) );

            CollectionAssert.AreEqual( new[] { Tuple.Create( "ABC", "XYZ", "123" ) }, logger.Commands );
        }

        [TestMethod]
        public void LogParametersRelativeToViewModelAreHonored()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel2>();
            ( (TestViewModel2) nav.CurrentViewModel ).SomeValue = Tuple.Create( "a b c" );
            ( (TestViewModel2) nav.CurrentViewModel ).Command4.Execute();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "2", "C4", "a b c" ) }, logger.Commands );
        }

        [TestMethod]
        public void LogParametersRelativeToCommandParameterAreHonored()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel2>();
            ( (TestViewModel2) nav.CurrentViewModel ).Command5.Execute( "x y z" );

            CollectionAssert.AreEqual( new[] { Tuple.Create( "2", "C5", "x y z" ) }, logger.Commands );
        }

        [TestMethod]
        public void LogValueConvertersAreHonored()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();


            nav.NavigateTo<TestViewModel2>();
            ( (TestViewModel2) nav.CurrentViewModel ).Command6.Execute( true );
            ( (TestViewModel2) nav.CurrentViewModel ).Command6.Execute( false );

            CollectionAssert.AreEqual( new[] { Tuple.Create( "2", "C6", "Yes" ), 
                                               Tuple.Create( "2", "C6", "No" ) }, logger.Commands );
        }

        [TestMethod]
        public async Task CustomRefreshCommandIsNotSpecial()
        {
            var nav = new TestNavigationService();
            var logger = new TestLogger( nav );
            logger.Start();

            nav.NavigateTo<TestViewModel2>();
            await ( (TestViewModel2) nav.CurrentViewModel ).RefreshCommand.ExecuteAsync();

            CollectionAssert.AreEqual( new[] { Tuple.Create( "2", LoggedSpecialAction.ForwardsNavigation ) }, logger.Actions );
            CollectionAssert.AreEqual( new[] { Tuple.Create( "2", "Refresh", (string) null ) }, logger.Commands );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Constructor_ErrorOnNullNavigationService()
        {
            new TestLogger( null );
        }
    }
}