// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        private static Frame _rootFrame;

        /// <summary>
        /// Gets the root frame of the application.
        /// </summary>
        /// <remarks>
        /// Override the <see cref="M:AppBase.GetRootFrame" /> method to provide your own frame.
        /// If your app needs to handle special launches, such as a share target, you'll need to set
        /// this property manually, then set <c>Window.Current.Content</c> to this property, 
        /// and call <c>Window.Current.Activate()</c>.
        /// </remarks>
        protected internal static Frame RootFrame
        {
            get { return _rootFrame; }
            protected set
            {
                if ( value == null )
                {
                    throw new ArgumentNullException();
                }

                if ( _rootFrame != null )
                {
                    throw new InvalidOperationException( "Cannot overwrite the root frame of the app." );
                }

                _rootFrame = value;
            }
        }


        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        protected AppBase()
        {
            Suspending += OnSuspending;
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
        /// <param name="args">Details about the launch request and process.</param>
        /// <remarks>
        /// This method can also be invoked while the app is already running.
        /// </remarks>
        protected abstract void Launch( LaunchActivatedEventArgs args );

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
        /// <param name="args">Details about the launch request and process.</param>
        protected override sealed void OnLaunched( LaunchActivatedEventArgs args )
        {
            if ( Window.Current.Content == null )
            {
                if ( args.PreviousExecutionState == ApplicationExecutionState.Terminated )
                {
                    ReloadState();
                }

                RootFrame = CreateRootFrame();
                Window.Current.Content = RootFrame;
            }

            Launch( args );
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="args">Details about the suspend request.</param>
        private void OnSuspending( object sender, SuspendingEventArgs args )
        {
            var deferral = args.SuspendingOperation.GetDeferral();
            SaveState();
            deferral.Complete();
        }
    }
}