// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Implementation of <see cref="IWindowsPhoneNavigationService" />.
    /// </summary>
    public sealed class WindowsPhoneNavigationService : IWindowsPhoneNavigationService
    {
        private const char UriParametersPrefix = '?';
        private const char UriParametersDelimiter = '&';
        private const char UriParameterKeyValueSeparator = '=';
        private const string UniqueParameter = "ThinMvvm.WindowsPhone.UniqueId";

        private readonly Dictionary<Type, Uri> _views;
        // HACK: IViewModel can't be covariant because it would forbid value types as TArgs,
        //       and having a non-generic IViewModel that shouldn't be implemented is a terrible idea
        //       so we use dynamic to call OnNavigatedTo/From
        private readonly Stack<dynamic> _backStack;
        private readonly Stack<bool> _ignored;

        private bool _removeCurrentFromBackstack;


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPhoneNavigationService" /> class.
        /// </summary>
        public WindowsPhoneNavigationService()
        {
            _views = new Dictionary<Type, Uri>();
            _backStack = new Stack<dynamic>();
            _ignored = new Stack<bool>();

            AppBase.RootFrame.Navigated += Frame_Navigated;
        }


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
                AppBase.RootFrame.GoBack();
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

        /// <summary>
        /// Occurs when the service navigates to a page, forwards or backwards.
        /// </summary>
        public event EventHandler<NavigatedEventArgs> Navigated;
        /// <summary>
        /// Fires the <see cref="Navigated" /> event.
        /// </summary>
        private void OnNavigated( object viewModel, bool isForwards )
        {
            var evt = Navigated;
            if ( evt != null )
            {
                evt( this, new NavigatedEventArgs( viewModel, isForwards ) );
            }
        }


        /// <summary>
        /// Binds the specified View URI to the specified ViewModel type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <param name="viewUri">The View URI. It needs to be relative to the app root (e.g. /MyApp;Component/Views/MyView.xaml).</param>
        public void Bind<TViewModel>( string viewUri )
        {
            _views.Add( typeof( TViewModel ), new Uri( viewUri, UriKind.Relative ) );
        }


        /// <summary>
        /// Navigates to the specified ViewModel.
        /// </summary>
        private void NavigateToPrivate( object viewModel )
        {
            var viewModelType = viewModel.GetType();
            OnNavigated( viewModel, true );
            _backStack.Push( viewModel );
            AppBase.RootFrame.Navigate( MakeUnique( _views[viewModelType] ) );
        }

        /// <summary>
        /// Occurs when the frame has navigated, either because the user requested it or because the program did.
        /// </summary>
        private void Frame_Navigated( object sender, NavigationEventArgs e )
        {
            var page = (PhoneApplicationPage) AppBase.RootFrame.Content;

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
                    OnNavigated( currentViewModel, false );
                }
            }
            else if ( e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.New )
            {
                if ( _removeCurrentFromBackstack )
                {
                    AppBase.RootFrame.RemoveBackEntry();

                    var newTop = _backStack.Pop();
                    var currentTop = _backStack.Pop();
                    _backStack.Push( newTop );

                    DisposeIfNeeded( currentTop );

                    _removeCurrentFromBackstack = false;
                }

                if ( e.Uri.ToString().Contains( UniqueParameter ) ) // can't check for IsNavigationInitiator as it's false for the first navigation
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
                        OnNavigated( currentViewModel, false );
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
            string uniqueValue = Guid.NewGuid().ToString();
            char separator = uri.ToString().Contains( UriParametersPrefix ) ? UriParametersDelimiter
                                                                            : UriParametersPrefix;
            string uniqueUri = uri.ToString() + separator + UniqueParameter + UriParameterKeyValueSeparator + uniqueValue;
            return new Uri( uniqueUri, UriKind.RelativeOrAbsolute );
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
    }
}