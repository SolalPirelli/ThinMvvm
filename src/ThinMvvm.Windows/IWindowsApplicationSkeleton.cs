using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// Represents the skeleton of a Windows application.
    /// </summary>
    public interface IWindowsApplicationSkeleton
    {
        /// <summary>
        /// Gets the frame to use for navigation.
        /// </summary>
        Frame NavigationFrame { get; }
    }
}