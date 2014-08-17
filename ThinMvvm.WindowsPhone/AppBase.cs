// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Base application class that abstracts away some concepts and handles boilerplate 
    /// such as root visual creation and exception handling.
    /// </summary>
    public abstract class AppBase : Application
    {
        private const string FirstRunKey = "ThinMvvm.WindowsPhone.FirstRun";


        /// <summary>
        /// Infrastructure.
        /// Gets the root frame of the application.
        /// </summary>
        internal static PhoneApplicationFrame RootFrame { get; private set; }


        /// <summary>
        /// Gets the app's language.
        /// </summary>
        /// <remarks>
        /// Usually, this will come from the app resources.
        /// </remarks>
        protected abstract string Language { get; }

        /// <summary>
        /// Gets the app's flow direction, as a string.
        /// </summary>
        /// <remarks>
        /// Usually, this will come from the app resources.
        /// </remarks>
        protected abstract string FlowDirection { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AppBase" /> class.
        /// </summary>
        protected AppBase()
        {
            UnhandledException += OnUnhandledException;
            ApplicationLifetimeObjects.Add( new PhoneApplicationService() );

            RootFrame = CreateRootFrame();
            if ( RootFrame == null )
            {
                throw new InvalidOperationException( "RootFrame cannot be null." );
            }

            RootFrame.Navigating += OnAppOpening;
            RootFrame.NavigationFailed += OnNavigationFailed;

            InitializeLanguage();
        }


        /// <summary>
        /// Creates the root frame of the app.
        /// Override this method to change the root frame's type, e.g. if you need one that supports transitions.
        /// </summary>
        /// <returns>A frame that will be set as the root frame of the app.</returns>
        protected virtual PhoneApplicationFrame CreateRootFrame()
        {
            return new PhoneApplicationFrame();
        }

        /// <summary>
        /// Called when the app starts.
        /// </summary>
        /// <param name="dependencies">The app dependencies.</param>
        /// <param name="arguments">The app arguments.</param>
        protected abstract void Start( AppDependencies dependencies, AppArguments arguments );


        /// <summary>
        /// Checks whether this is the first time the application is run.
        /// Only executions in which this method was called are counted.
        /// </summary>
        /// <remarks>
        /// This method accesses the isolated storage, which Microsoft defines as "resource-intensive".
        /// Therefore, implementors should avoid using this method unless they absolutely need it.
        /// </remarks>
        protected bool IsFirstRun()
        {
            // Do first run stuff now, message boxes (a common use case) can't be displayed before
            bool dummy;
            if ( !IsolatedStorageSettings.ApplicationSettings.TryGetValue( FirstRunKey, out dummy ) )
            {
                IsolatedStorageSettings.ApplicationSettings.Add( FirstRunKey, false );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Occurs before the very first navigation, set by the "Navigation Page" in WMAppManifest, to cancel it.
        /// </summary>
        private void OnAppOpening( object sender, NavigatingCancelEventArgs e )
        {
            e.Cancel = true;
            RootVisual = RootFrame;
            RootFrame.Navigating -= OnAppOpening;

            var deps = (AppDependencies) Container.Get( typeof( AppDependencies ), null );
            var args = new AppArguments( e.Uri );
            // Overlapping navigations aren't allowed, schedule the new navigation for later
            RootFrame.Dispatcher.BeginInvoke( () => Start( deps, args ) );
        }

        /// <summary>
        /// Occurs when a navigation fails.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnNavigationFailed( object sender, NavigationFailedEventArgs e )
        {
            if ( e == null )
            {
                throw new ArgumentNullException( "e" );
            }

            if ( !e.Handled && Debugger.IsAttached )
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Occurs when an unhandled exception is raised.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnUnhandledException( object sender, ApplicationUnhandledExceptionEventArgs e )
        {
            if ( e == null )
            {
                throw new ArgumentNullException( "e" );
            }

            if ( !e.Handled && Debugger.IsAttached )
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Sets the root frame's language and flow direction from the resources.
        /// </summary>
        private void InitializeLanguage()
        {
            try
            {
                RootFrame.Language = XmlLanguage.GetLanguage( Language );
                RootFrame.FlowDirection = (FlowDirection) Enum.Parse( typeof( FlowDirection ), FlowDirection );
            }
            catch
            {
                if ( Debugger.IsAttached )
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        /// <summary>
        /// Settings used when a debugger is attached.
        /// </summary>
        protected static class DebugSettings
        {
            /// <summary>
            /// Gets or sets a value indicating whether frame rate counters are enabled or disabled.
            /// </summary>
            public static bool EnableFrameRateCounter
            {
                get { return Application.Current.Host.Settings.EnableFrameRateCounter; }
                set { DoWhenDebuggerAttached( () => Application.Current.Host.Settings.EnableFrameRateCounter = value ); }
            }

            /// <summary>
            /// Gets or sets a value indicating whether areas of the app that are being redrawn in each frame should be displayed specially.
            /// </summary>
            public static bool EnableRedrawRegions
            {
                get { return Application.Current.Host.Settings.EnableRedrawRegions; }
                set { DoWhenDebuggerAttached( () => Application.Current.Host.Settings.EnableRedrawRegions = value ); }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the areas of a page that are handed off to the GPU should be displayed with a colored overlay.
            /// </summary>
            public static bool EnableCacheVisualization
            {
                get { return Application.Current.Host.Settings.EnableCacheVisualization; }
                set { DoWhenDebuggerAttached( () => Application.Current.Host.Settings.EnableCacheVisualization = value ); }
            }

            /// <summary>
            /// Gets or sets the idle detection mode for the user.
            /// </summary>
            public static IdleDetectionMode UserIdleDetectionMode
            {
                get { return PhoneApplicationService.Current.UserIdleDetectionMode; }
                set { DoWhenDebuggerAttached( () => PhoneApplicationService.Current.UserIdleDetectionMode = value ); }
            }


            /// <summary>
            /// Executes the specified action if a debugger is attached.
            /// </summary>
            private static void DoWhenDebuggerAttached( Action action )
            {
                if ( Debugger.IsAttached )
                {
                    action();
                }
            }
        }
    }
}