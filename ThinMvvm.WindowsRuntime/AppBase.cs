// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace ThinMvvm.WindowsRuntime
{
    public abstract class AppBase : Application
    {
        internal static Frame RootFrame { get; private set; }

        private TransitionCollection transitions;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        protected AppBase()
        {
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Creates the root frame of the app.
        /// Override this method to change the root frame's type.
        /// </summary>
        /// <returns>A frame that will be set as the root frame of the app.</returns>
        protected virtual Frame CreateRootFrame()
        {
            // TODO: say something about cache size in the doc
            return new Frame { CacheSize = 5 };
        }

        // TODO: Say something about handling Windows.Phone.UI.Input.HardwareButtons.BackPressed for WP
        protected abstract void Initialize( LaunchActivatedEventArgs e );

        protected virtual void SaveState() { }

        protected virtual void ReloadState() { }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. 
        /// Other entry points will be used when the application is launched to 
        /// open a specific file, display search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched( LaunchActivatedEventArgs e )
        {
            if ( Debugger.IsAttached )
            {
                DebugSettings.EnableFrameRateCounter = true;
            }

            RootFrame = (Frame) Window.Current.Content;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if ( RootFrame == null )
            {
                RootFrame = CreateRootFrame();

                if ( e.PreviousExecutionState == ApplicationExecutionState.Terminated )
                {
                    ReloadState();
                }

                Window.Current.Content = RootFrame;
            }

            if ( RootFrame.Content == null )
            {
                // Removes the turnstile navigation for startup.
                if ( RootFrame.ContentTransitions != null )
                {
                    this.transitions = RootFrame.ContentTransitions;
                    RootFrame.ContentTransitions = null;
                }

                RootFrame.Navigated += RootFrame_FirstNavigated;

                Initialize( e );
            }

            Window.Current.Activate();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated( object sender, NavigationEventArgs e )
        {
            RootFrame.ContentTransitions = this.transitions ?? new TransitionCollection();
            RootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending( object sender, SuspendingEventArgs e )
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            SaveState();

            deferral.Complete();
        }
    }
}