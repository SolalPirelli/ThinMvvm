// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Contains dependencies required by an application's entry point.
    /// If you need additional dependencies, implement this class and add dependencies as constructor arguments,
    /// then register your implementation with the <see cref="Container" />.
    /// </summary>
    public class AppDependencies
    {
        /// <summary>
        /// Gets a navigation service for the app.
        /// </summary>
        public IWindowsPhoneNavigationService NavigationService { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AppDependencies" /> class.
        /// </summary>
        /// <param name="navigationService">A navigation service for the app.</param>
        public AppDependencies( IWindowsPhoneNavigationService navigationService )
        {
            NavigationService = navigationService;
        }
    }
}