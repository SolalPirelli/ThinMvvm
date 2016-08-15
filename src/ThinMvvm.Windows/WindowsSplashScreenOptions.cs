using System;
using Windows.UI;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// Options for a Windows splash screen.
    /// </summary>
    public sealed class WindowsSplashScreenOptions
    {
        /// <summary>
        /// Gets or sets the screen's background color.
        /// The default is light gray (#464646).
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the screen's foreground color.
        /// The default is white.
        /// </summary>
        public Color ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the URI of the screen's logo.
        /// The default is /Assets/SplashScreen.png.
        /// </summary>
        public Uri LogoUri { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsSplashScreenOptions" /> class.
        /// </summary>
        public WindowsSplashScreenOptions()
        {
            BackgroundColor = Color.FromArgb( 0xFF, 0x46, 0x46, 0x46 );
            ForegroundColor = Colors.White;
            LogoUri = new Uri( $"ms-appx:///Assets/SplashScreen.png", UriKind.Absolute );
        }
    }
}