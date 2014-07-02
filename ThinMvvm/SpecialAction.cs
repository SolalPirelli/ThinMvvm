// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// Special actions that are logged separately from commands.
    /// </summary>
    public enum SpecialAction
    {
        /// <summary>
        /// Forwards navigation to a ViewModel.
        /// </summary>
        ForwardsNavigation,

        /// <summary>
        /// Backwards navigation to a ViewModel.
        /// </summary>
        BackwardsNavigation,

        /// <summary>
        /// "Refresh" command execution on a ViewModel.
        /// </summary>
        Refresh
    }
}