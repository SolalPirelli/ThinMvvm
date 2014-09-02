﻿// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ThinMvvm.WindowsRuntime
{
    public sealed class WindowsRuntimeNavigationService : IWindowsRuntimeNavigationService
    {
        private readonly Dictionary<Type, Type> _views;
        // HACK: IViewModel can't be covariant because it would forbid value types as TParameters,
        //       and having a non-generic ViewModel that shouldn't be implemented is a terrible idea
        //       so we use dynamic to call OnNavigatedTo/From
        private readonly Stack<dynamic> _backStack;
        private readonly Stack<bool> _shouldIgnore;
        private bool _removeCurrentFromBackStack;

        public WindowsRuntimeNavigationService()
        {
            _views = new Dictionary<Type, Type>();
            _backStack = new Stack<dynamic>();
            _shouldIgnore = new Stack<bool>();

            AppBase.RootFrame.Navigated += RootFrame_Navigated;
        }


        public void Bind<TViewModel, TView>() where TView : Windows.UI.Xaml.Controls.Page
        {
            _views.Add( typeof( TViewModel ), typeof( TView ) );
        }

        public void NavigateTo<T>() where T : ViewModel<NoParameter>
        {
            var vm = Container.Get( typeof( T ), null );
            NavigateToPrivate( vm );
        }

        public void NavigateTo<TViewModel, TArg>( TArg arg ) where TViewModel : ViewModel<TArg>
        {
            var vm = Container.Get( typeof( TViewModel ), arg );
            NavigateToPrivate( vm );
        }

        public void NavigateBack()
        {
            if ( AppBase.RootFrame.CanGoBack )
            {
                AppBase.RootFrame.GoBack();
            }
            else
            {
                Application.Current.Exit();
            }
        }

        public void RemoveCurrentFromBackStack()
        {
            if ( _removeCurrentFromBackStack )
            {
                throw new InvalidOperationException( "RemoveCurrentFromBackStack was already called in this ViewModel." );
            }

            _removeCurrentFromBackStack = true;
        }

        public event EventHandler<NavigatedEventArgs> Navigated;
        private void OnNavigated( object viewModel, bool isForward )
        {
            var evt = Navigated;
            if ( evt != null )
            {
                evt( this, new NavigatedEventArgs( viewModel, isForward ) );
            }
        }


        private void NavigateToPrivate( object viewModel )
        {
            Type viewType;
            if ( !_views.TryGetValue( viewModel.GetType(), out viewType ) )
            {
                throw new ArgumentException( string.Format( "{0} has no registered View.", viewModel.GetType().FullName ) );
            }

            _backStack.Push( viewModel );
            AppBase.RootFrame.Navigate( _views[viewModel.GetType()] );
        }


        private void RootFrame_Navigated( object sender, NavigationEventArgs e )
        {
            var page = (Page) AppBase.RootFrame.Content;

            if ( e.NavigationMode == NavigationMode.Back )
            {
                if ( _shouldIgnore.Pop() )
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
                if ( _removeCurrentFromBackStack )
                {
                    AppBase.RootFrame.BackStack.RemoveAt( 0 );

                    var newTop = _backStack.Pop();
                    var currentTop = _backStack.Pop();
                    _backStack.Push( newTop );

                    DisposeIfNeeded( currentTop );

                    _removeCurrentFromBackStack = false;
                }

                if ( _views.Values.Contains( e.Content.GetType() ) ) // can't check for IsNavigationInitiator as it's false for the first navigation
                {
                    _shouldIgnore.Push( false );

                    if ( _backStack.Count > 0 )
                    {
                        var currentViewModel = _backStack.Peek();
                        currentViewModel.OnNavigatedTo();
                        page.DataContext = currentViewModel;
                        OnNavigated( currentViewModel, true );
                    }
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
    }
}