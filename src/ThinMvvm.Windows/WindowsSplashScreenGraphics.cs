using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Xaml;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// Graphical options for a Windows splash screen.
    /// </summary>
    public sealed class WindowsSplashScreenGraphics
    {
        private static readonly Color AccentColor = (Color) Application.Current.Resources["SystemAccentColor"];
        private static readonly Uri DefaultLogoUri = new Uri( $"ms-appx:///Assets/SplashScreen.png", UriKind.Absolute );


        /// <summary>
        /// Gets the screen's background color.
        /// The default is the system accent color.
        /// </summary>
        public Color BackgroundColor { get; }

        /// <summary>
        /// Gets the screen's foreground color.
        /// The default is black or white, depending on which is more readable on the system accent color.
        /// </summary>
        public Color ForegroundColor { get; }

        /// <summary>
        /// Gets the URI of the screen's logo.
        /// The default is /Assets/SplashScreen.png.
        /// </summary>
        public Uri LogoUri { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsSplashScreenGraphics" /> class.
        /// </summary>
        public WindowsSplashScreenGraphics( Color backgroundColor, Color foregroundColor, Uri logoUri )
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
            LogoUri = logoUri;
        }


        /// <summary>
        /// Loads options from the package's application manifest.
        /// </summary>
        /// <returns>Options loaded from the application manifest.</returns>
        /// <remarks>
        /// From http://stackoverflow.com/a/38945458/3311770
        /// </remarks>
        public static WindowsSplashScreenGraphics LoadFromAppManifest()
        {
            try
            {
                var manifest = XDocument.Load( "AppxManifest.xml" );
                var ns = XNamespace.Get( "http://schemas.microsoft.com/appx/manifest/uap/windows10" );

                var splashScreen = manifest.Descendants( ns + "VisualElements" ).First()
                                           .Descendants( ns + "SplashScreen" ).First();

                var colorText = splashScreen.Attribute( "BackgroundColor" ).Value;
                
                var logoPath = splashScreen.Attribute( "Image" ).Value
                                           .Replace( '\\', '/' );

                var backgroundColor = ParseColor( colorText ) ?? AccentColor;
                var foregroundColor = GetForegroundColorFromBackground( backgroundColor );
                var logoUri = new Uri( "ms-appx:///" + logoPath, UriKind.Absolute );

                return new WindowsSplashScreenGraphics( backgroundColor, foregroundColor, logoUri );
            }
            catch
            {
                var foregroundColor = GetForegroundColorFromBackground( AccentColor );
                return new WindowsSplashScreenGraphics( AccentColor, foregroundColor, DefaultLogoUri );
            }
        }

        /// <summary>
        /// Attempts to parse the specified text as a color.
        /// Returns null on failure.
        /// </summary>
        private static Color? ParseColor( string colorText )
        {
            if( string.IsNullOrWhiteSpace( colorText ) )
            {
                return null;
            }

            if( colorText == "transparent" )
            {
                return AccentColor;
            }

            if( colorText.StartsWith( "#" ) )
            {
                if( colorText.Length == 7 ) // #RRGGBB
                {
                    byte r = byte.Parse( colorText.Substring( 1, 2 ), NumberStyles.HexNumber );
                    byte g = byte.Parse( colorText.Substring( 3, 2 ), NumberStyles.HexNumber );
                    byte b = byte.Parse( colorText.Substring( 5, 2 ), NumberStyles.HexNumber );

                    return Color.FromArgb( 0xFF, r, g, b );
                }

                return null;
            }

            var prop = typeof( Colors ).GetTypeInfo()
                                       .DeclaredProperties
                                       .FirstOrDefault( f => colorText.Equals( f.Name, StringComparison.OrdinalIgnoreCase ) );

            if( prop == null )
            {
                return null;
            }

            return (Color) prop.GetValue( null );
        }

        /// <summary>
        /// Gets a readable foreground color from the specified background color.
        /// </summary>
        /// <remarks>
        /// From http://stackoverflow.com/a/3943023/3311770
        /// </remarks>
        private static Color GetForegroundColorFromBackground( Color c )
        {
            if( c.R * 0.299 + c.G * 0.587 + c.B * 0.114 > 186 )
            {
                return Colors.Black;
            }

            return Colors.White;
        }
    }
}