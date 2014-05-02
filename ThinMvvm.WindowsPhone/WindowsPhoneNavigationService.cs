// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using ThinMvvm.Logging;

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Implementation of IWindowsPhoneNavigationService.
    /// </summary>
    public sealed class WindowsPhoneNavigationService : IWindowsPhoneNavigationService
    {
        private const char UriParametersPrefix = '?';
        private const char UriParametersDelimiter = '&';
        private const string UniqueParameter = "mvvm_unique_id";

        private static PhoneApplicationFrame Frame
        {
            get { return (PhoneApplicationFrame) Application.Current.RootVisual; }
        }

        private readonly Logger _logger;
        private readonly Dictionary<Type, Uri> _views;
        // HACK: IViewModel can't be covariant to be used with value types
        //       and having a non-generic IViewModel that shouldn't be implemented is a terrible idea
        //       so we use dynamic to call OnNavigatedTo/From
        private readonly Stack<dynamic> _backStack;
        private readonly Stack<bool> _ignored;

        private bool _removeCurrentFromBackstack;


        /// <summary>
        /// Creates a new WindowsPhoneNavigationService.
        /// </summary>
        public WindowsPhoneNavigationService( Logger logger )
        {
            _logger = logger;
            _views = new Dictionary<Type, Uri>();
            _backStack = new Stack<dynamic>();
            _ignored = new Stack<bool>();

            Frame.Navigated += Frame_Navigated;
        }


        /// <summary>
        /// Navigates to the specified ViewModel.
        /// </summary>
        private void NavigateToPrivate( object viewModel )
        {
            var viewModelType = viewModel.GetType();
            _logger.LogNavigation( viewModel, true );
            _backStack.Push( viewModel );
            Frame.Navigate( MakeUnique( _views[viewModelType] ) );
        }

        /// <summary>
        /// Occurs when the frame has navigated, either because the user requested it or because the program did.
        /// </summary>
        private void Frame_Navigated( object sender, NavigationEventArgs e )
        {
            var page = (PhoneApplicationPage) Frame.Content;

            // need to check IsNavigationInitiator to avoid doing stuff when the user
            // long-presses the Back button to multitask
            if ( e.NavigationMode == NavigationMode.Back && e.IsNavigationInitiator )
            {
                if ( _ignored.Pop() )
                {
                    return;
                }

                if ( _backStack.Count > 0 )
                {
                    var currentTop = _backStack.Pop();
                    currentTop.OnNavigatedFrom();
                    DisposeIfNeeded( currentTop );
                }
                if ( _backStack.Count > 0 )
                {
                    var currentViewModel = _backStack.Peek();
                    currentViewModel.OnNavigatedTo();
                    page.DataContext = currentViewModel;
                    _logger.LogNavigation( currentViewModel, false );
                }
            }
            else if ( e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.New )
            {
                if ( _removeCurrentFromBackstack )
                {
                    Frame.RemoveBackEntry();

                    var newTop = _backStack.Pop();
                    var currentTop = _backStack.Pop();
                    _backStack.Push( newTop );

                    DisposeIfNeeded( currentTop );

                    _removeCurrentFromBackstack = false;
                }

                if ( e.IsNavigationInitiator )
                {
                    // Ignore pages we don't know about
                    if ( !_views.Any( p => UriEquals( p.Value, e.Uri ) ) )
                    {
                        _ignored.Push( true );
                        return;
                    }
                    _ignored.Push( false );


                    if ( _backStack.Count > 0 )
                    {
                        var currentViewModel = _backStack.Peek();
                        currentViewModel.OnNavigatedTo();
                        page.DataContext = currentViewModel;
                        _logger.LogNavigation( currentViewModel, false );
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of the specified object, if it is an IDisposable.
        /// </summary>
        private static void DisposeIfNeeded( object obj )
        {
            var disposable = obj as IDisposable;
            if ( disposable != null )
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Makes a unique URI out of the specified URI.
        /// </summary>
        /// <remarks>
        /// This allows same-page navigations.
        /// </remarks>
        private static Uri MakeUnique( Uri uri )
        {
            string uniqueParameterValue = Guid.NewGuid().ToString();
            char separator = uri.ToString().Contains( UriParametersPrefix ) ? UriParametersDelimiter
                                                                            : UriParametersPrefix;
            return new Uri( uri.ToString() + separator + UniqueParameter + uniqueParameterValue, UriKind.RelativeOrAbsolute );
        }

        /// <summary>
        /// Indicates whether the two specified URIs are considered to be equal.
        /// </summary>
        private static bool UriEquals( Uri uri1, Uri uri2 )
        {
            return GetUriPath( uri1.ToString() ) == GetUriPath( uri2.ToString() );
        }

        /// <summary>
        /// Gets the path section of the specified URI.
        /// </summary>
        private static string GetUriPath( string uri )
        {
            if ( uri.Contains( UriParametersPrefix ) )
            {
                return uri.ToString().Substring( 0, uri.ToString().IndexOf( UriParametersPrefix ) );
            }
            return uri;
        }

        #region INavigationService implementation
        /// <summary>
        /// Navigates to a ViewModel of the specified type.
        /// </summary>
        public void NavigateTo<T>()
            where T : IViewModel<NoParameter>
        {
            var vm = Container.Get( typeof( T ), null );
            NavigateToPrivate( vm );
        }

        /// <summary>
        /// Navigates to a ViewModel of the specified type.
        /// </summary>
        public void NavigateTo<TViewModel, TArg>( TArg arg )
            where TViewModel : IViewModel<TArg>
        {
            var vm = Container.Get( typeof( TViewModel ), arg );
            NavigateToPrivate( vm );
        }

        /// <summary>
        /// Goes back to the previous ViewModel.
        /// </summary>
        public void NavigateBack()
        {
            if ( _backStack.Count > 0 )
            {
                Frame.GoBack();
            }
            else
            {
                Application.Current.Terminate();
            }
        }

        /// <summary>
        /// Pops the ViewModel back-stack, removing the current one so that going backwards will not go to it.
        /// </summary>
        public void PopBackStack()
        {
            _removeCurrentFromBackstack = true;
        }
        #endregion

        #region IWindowsPhoneNavigationService implementation
        /// <summary>
        /// Adds a ViewModel to View URI link.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel's type.</typeparam>
        /// <param name="viewUri">The View URI. Needs to be relative to the app root (e.g. /MyApp;Component/Views/MyView.xaml).</param>
        public void Bind<TViewModel>( string viewUri )
        {
            _views.Add( typeof( TViewModel ), new Uri( viewUri, UriKind.Relative ) );
        }
        #endregion
    }
}