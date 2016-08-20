using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ThinMvvm.Windows.Infrastructure
{
    /// <summary>
    /// Extended splash screen for a Windows application.
    /// </summary>
    public sealed class WindowsSplashScreen : Grid
    {
        private readonly SplashScreen _appSplashScreen;
        private readonly Image _image;
        private readonly ProgressRing _progressRing;


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsSplashScreen" /> class with the specified parameters.
        /// </summary>
        /// <param name="appSplashScreen">The app's splash screen.</param>
        /// <param name="graphics">The splash screen graphics.</param>
        public WindowsSplashScreen( SplashScreen appSplashScreen, WindowsSplashScreenGraphics graphics )
        {
            _appSplashScreen = appSplashScreen;

            _image = new Image
            {
                Source = new BitmapImage( graphics.LogoUri ),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            _progressRing = new ProgressRing
            {
                IsActive = true,
                Foreground = new SolidColorBrush( graphics.ForegroundColor ),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            Background = new SolidColorBrush( graphics.BackgroundColor );
            Children.Add( _image );
            Children.Add( _progressRing );
        }


        /// <summary>
        /// Displays the splash screen.
        /// </summary>
        /// <param name="shouldActivate">A value indicating whether the window should be activated once the splash screen is displayed.</param>
        public void Show( bool shouldActivate )
        {
            RoutedEventHandler imageLoaded = async ( _, __ ) =>
            {
                PositionControls();

                if( shouldActivate )
                {
                    // HACK: Without this, the normal splash screen will disappear before the controls
                    //       have been positioned, causing minor flickering.
                    await Task.Delay( TimeSpan.FromMilliseconds( 5 ) );

                    Window.Current.Activate();
                }
            };

            // Ignore failure, the lack of a splash screen should be obvious to any developer who puts the wrong URI
            _image.ImageFailed += new ExceptionRoutedEventHandler( imageLoaded );
            _image.ImageOpened += imageLoaded;

            // Positions are absolute since we must mimick the provided app SplashScreen, thus they need updating
            Window.Current.SizeChanged += ( _, __ ) => PositionControls();

            Window.Current.Content = this;
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