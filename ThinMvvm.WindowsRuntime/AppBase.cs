// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace ThinMvvm.WindowsRuntime
{
    /// <summary>
    /// Base application class that abstracts away some concepts and handles boilerplate 
    /// such as root frame creation.
    /// </summary>
    /// <remarks>
    /// On Windows Phone, do not forget to handle the Windows.Phone.UI.Input.HardwareButtons.BackPressed 
    /// event to handle back button presses (e.g. by going back using the navigation service).
    /// </remarks>
    public abstract class AppBase : Application
    {
        // Holds the frame's transitions, since they must be removed prior to the first navigation and re-added afterwards
        private TransitionCollection _frametransitions;


        /// <summary>
        /// Gets the root frame of the application.
        /// </summary>
        internal static Frame RootFrame { get; private set; }


        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        protected AppBase()
        {
            Suspending += OnSuspending;
            RootFrame = CreateRootFrame();
        }


        /// <summary>
        /// Creates the root frame of the app.
        /// Override this method to change the root frame's type, or to use different parameters for e.g. the cache size.
        /// By default, returns a <see cref="Frame" /> with a cache size of 5.
        /// </summary>
        /// <returns>A frame that will be set as the root frame of the app.</returns>
        protected virtual Frame CreateRootFrame()
        {
            return new Frame { CacheSize = 5 };
        }

        /// <summary>
        /// Launches the app.
        /// This is only invoked when the end user launched the app normally;
        /// other endpoints (e.g. opening files or displaying search results)
        /// should be handled separately by overriding the associated methods.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected abstract void Launch( LaunchActivatedEventArgs e );

        /// <summary>
        /// Save state when the application is about to be set to the background (but will not always be terminated).
        /// </summary>
        protected virtual void SaveState() { }

        /// <summary>
        /// Reload state, after the application was terminated.
        /// </summary>
        protected virtual void ReloadState() { }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. 
        /// Other entry points will be used when the application is launched to 
        /// open a specific file, display search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override sealed void OnLaunched( LaunchActivatedEventArgs e )
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if ( Window.Current.Content == null )
            {
                if ( e.PreviousExecutionState == ApplicationExecutionState.Terminated )
                {
                    ReloadState();
                }

                Window.Current.Content = RootFrame;
            }

            RootFrame = (Frame) Window.Current.Content;

            if ( RootFrame.Content == null )
            {
                // Removes the turnstile navigation for startup.
                if ( RootFrame.ContentTransitions != null )
                {
                    _frametransitions = RootFrame.ContentTransitions;
                    RootFrame.ContentTransitions = null;
                }

                RootFrame.Navigated += RootFrame_FirstNavigated;

                Launch( e );
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
            RootFrame.ContentTransitions = _frametransitions ?? new TransitionCollection();
            RootFrame.Navigated -= RootFrame_FirstNavigated;
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