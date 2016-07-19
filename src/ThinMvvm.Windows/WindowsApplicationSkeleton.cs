using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    public sealed class WindowsApplicationSkeleton
    {
        public UIElement Root { get; }

        public Frame NavigationFrame { get; }


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