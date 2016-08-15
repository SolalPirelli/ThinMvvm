using System;
using ThinMvvm.Applications;
using ThinMvvm.Infrastructure;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ThinMvvm.Windows.Infrastructure
{
    /// <summary>
    /// Extended splash screen for a Windows application, wrapping an operation.
    /// </summary>
    public sealed class WindowsSplashScreen
    {
        private readonly INavigationService _navigationService;
        private readonly IApplicationOperation _operation;
        private readonly SplashScreen _appSplashScreen;
        private readonly UIElement _appRoot;
        private readonly bool _shouldActivate;

        private readonly Image _image;
        private readonly ProgressRing _progressRing;
        private readonly Grid _rootGrid;


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsSplashScreen" /> class with the specified parameters.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="appSplashScreen">The app's splash screen.</param>
        /// <param name="appRoot">The app's root element.</param>
        /// <param name="shouldActivate">A value indicating whether the app should be activated.</param>
        /// <param name="options">The graphical options.</param>
        public WindowsSplashScreen( INavigationService navigationService, IApplicationOperation operation,
                                    SplashScreen appSplashScreen, UIElement appRoot, bool shouldActivate,
                                    WindowsSplashScreenOptions options )
        {
            _navigationService = navigationService;
            _operation = operation;
            _appSplashScreen = appSplashScreen;
            _appRoot = appRoot;
            _shouldActivate = shouldActivate;


            _image = new Image
            {
                Source = new BitmapImage( options.LogoUri ),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            _progressRing = new ProgressRing
            {
                IsActive = true,
                Foreground = new SolidColorBrush( options.ForegroundColor ),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            _rootGrid = new Grid
            {
                Background = new SolidColorBrush( options.BackgroundColor ),
                Children =
                {
                    _image,
                    _progressRing
                }
            };
        }


        /// <summary>
        /// Displays the splash screen and executes the operation.
        /// </summary>
        public void Show()
        {
            Window.Current.Content = _rootGrid;
            
            // Use the same handler for success and failure, all we want is to do stuff once it has loaded
            // (and the lack of a splash screen should be obvious to any developer who puts the wrong URI)
            _image.ImageFailed += OnImageLoaded;
            _image.ImageOpened += OnImageLoaded;

            // Positions are absolute since we must mimick the provided app SplashScreen, thus they need updating
            Window.Current.SizeChanged += ( _, __ ) => PositionControls();
        }
        
        /// <summary>
        /// Called when the logo image is loaded.
        /// </summary>
        private void OnImageLoaded( object sender, RoutedEventArgs e )
        {
            PositionControls();

            // Initialize on a background operation (but in the UI thread for navigations!)
            // For some reason, this cannot be done in Initialize, otherwise the app splash screen is never dismissed.
            var ignored = Window.Current.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, async () =>
            {
                await _operation.ExecuteAsync( _navigationService );
                Window.Current.Content = _appRoot;
            } );

            if( _shouldActivate )
            {
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Positions the screen's controls.
        /// </summary>
        private void PositionControls()
        {
            PositionImage( _image, _appSplashScreen.ImageLocation );
            PositionProgressRing( _progressRing, _appSplashScreen.ImageLocation );
        }

        /// <summary>
        /// Sets the logo image's position.
        /// </summary>
        private void PositionImage( Image image, Rect imageLocation )
        {
            image.Margin = new Thickness( imageLocation.Left, imageLocation.Top, 0, 0 );

            // Official Windows 10 samples do this, not sure why the scale factor is not needed on desktop...
            if( ApiInformation.IsTypePresent( "Windows.Phone.UI.Input.HardwareButtons" ) )
            {
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                image.Height = imageLocation.Height / scaleFactor;
                image.Width = imageLocation.Width / scaleFactor;
            }
            else
            {
                image.Height = imageLocation.Height;
                image.Width = imageLocation.Width;
            }
        }

        /// <summary>
        /// Sets the progress ring's position.
        /// </summary>
        private void PositionProgressRing( ProgressRing ring, Rect imageLocation )
        {
            var remainingHeight = Window.Current.Bounds.Height - imageLocation.Bottom;

            // MSDN says ProgressRing does not display if it's less than 20px
            ring.Height = Math.Max( 20, remainingHeight / 5 );
            ring.Width = ring.Height;

            ring.Margin = new Thickness( 0, 0, 0, ring.Height * 2 );
        }
    }
}