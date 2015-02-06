// Copyright (c) 2014-15 Solal Pirelli
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
        // HACK: IViewModel can't be covariant because it would forbid value types as TParameters,
        //       and having a non-generic ViewModel that shouldn't be implemented is a terrible idea
        //       so we use dynamic to call OnNavigatedTo/From
        private readonly Stack<dynamic> _backStack;
        private readonly Stack<bool> _shouldIgnore;

        private bool _removeCurrentFromBackStack;


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPhoneNavigationService" /> class.
        /// </summary>
        public WindowsPhoneNavigationService()
        {
            _views = new Dictionary<Type, Uri>();
            _backStack = new Stack<dynamic>();
            _shouldIgnore = new Stack<bool>();

            AppBase.RootFrame.Navigated += Frame_Navigated;
        }


        /// <summary>
        /// Navigates to a ViewModel of the specified type.
        /// </summary>
        public void NavigateTo<T>()
            where T : ViewModel<NoParameter>
        {
            var vm = Container.Get( typeof( T ), null );
            NavigateToPrivate( vm );
        }

        /// <summary>
        /// Navigates to a ViewModel of the specified type.
        /// </summary>
        public void NavigateTo<TViewModel, TParameter>( TParameter arg )
            where TViewModel : ViewModel<TParameter>
        {
            var vm = Container.Get( typeof( TViewModel ), arg );
            NavigateToPrivate( vm );
        }

        /// <summary>
        /// Goes back to the previous ViewModel.
        /// </summary>
        public void NavigateBack()
        {
            if ( _backStack.Count == 0 )
            {
                Application.Current.Terminate();
            }
            else
            {
                AppBase.RootFrame.GoBack();
            }
        }

        /// <summary>
        /// Removes the current ViewModel from the back stack.
        /// </summary>
        public void RemoveCurrentFromBackStack()
        {
            if ( _removeCurrentFromBackStack )
            {
                throw new InvalidOperationException( "RemoveCurrentFromBackStack was already called in this ViewModel." );
            }

            _removeCurrentFromBackStack = true;
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
            if ( viewUri == null )
            {
                throw new ArgumentNullException( "viewUri" );
            }

            _views.Add( typeof( TViewModel ), new Uri( viewUri, UriKind.Relative ) );
        }


        /// <summary>
        /// Navigates to the specified ViewModel.
        /// </summary>
        private void NavigateToPrivate( object viewModel )
        {
            Uri viewUri;
            if ( !_views.TryGetValue( viewModel.GetType(), out viewUri ) )
            {
                throw new ArgumentException( string.Format( "{0} has no registered View.", viewModel.GetType().FullName ) );
            }

            _backStack.Push( viewModel );

            // BeginInvoke is a possible fix for an odd and misleading WP error message that might occur
            // when two navigations are executed at the same time:
            // http://social.msdn.microsoft.com/Forums/windowsapps/en-US/c68b397e-e16f-4474-b813-1ca03eef6932/navigation-is-not-allowed-when-the-task-is-not-in-the-foreground-error-while-debugging
            AppBase.RootFrame.Dispatcher.BeginInvoke( () => AppBase.RootFrame.Navigate( MakeUnique( viewUri ) ) );
        }

        /// <summary>
        /// Occurs when the frame has navigated, either because the user requested it or because the program did.
        /// </summary>
        private void Frame_Navigated( object sender, NavigationEventArgs e )
        {
            var page = (PhoneApplicationPage) AppBase.RootFrame.Content;

            if ( e.NavigationMode == NavigationMode.Back )
            {
                if ( _shouldIgnore.Pop() )
                {
                    return;
                }

                var currentTop = _backStack.Pop();
                currentTop.OnNavigatedFrom();
                DisposeIfNeeded( currentTop );

                if ( _backStack.Count > 0 )
                {
                    var newTop = _backStack.Peek();
                    newTop.OnNavigatedTo();
                    page.DataContext = newTop;
                    OnNavigated( newTop, false );
                }
            }
            else if ( e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.New )
            {
                if ( _removeCurrentFromBackStack )
                {
                    AppBase.RootFrame.RemoveBackEntry();

                    var newTop = _backStack.Pop();
                    var currentTop = _backStack.Pop();
                    _backStack.Push( newTop );

                    DisposeIfNeeded( currentTop );

                    _removeCurrentFromBackStack = false;
                }

                if ( e.Uri.ToString().Contains( UniqueParameter ) ) // can't check for IsNavigationInitiator as it's false for the first navigation
                {
                    _shouldIgnore.Push( false );
                    var currentViewModel = _backStack.Peek();
                    currentViewModel.OnNavigatedTo();
                    page.DataContext = currentViewModel;
                    OnNavigated( currentViewModel, true );
                }
                else
                {
                    _shouldIgnore.Push( true );
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
    }
}