using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// Skeleton of a Windows application.
    /// </summary>
    public sealed class WindowsApplicationSkeleton
    {
        /// <summary>
        /// The root element, i.e. the content of the main window.
        /// </summary>
        public UIElement Root { get; }

        /// <summary>
        /// The frame to use for navigation.
        /// </summary>
        public Frame NavigationFrame { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsApplicationSkeleton" /> class
        /// with the specified root element and navigation frame.
        /// </summary>
        /// <param name="root">The root element.</param>
        /// <param name="navigationFrame">The navigation frame.</param>
        public WindowsApplicationSkeleton( UIElement root, Frame navigationFrame )
        {
            if( root == null )
            {
                throw new ArgumentNullException( nameof( root ) );
            }
            if( navigationFrame == null )
            {
                throw new ArgumentNullException( nameof( navigationFrame ) );
            }

            Root = root;
            NavigationFrame = navigationFrame;
        }
    }
}