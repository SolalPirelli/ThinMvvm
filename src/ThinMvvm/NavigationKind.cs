namespace ThinMvvm
{
    /// <summary>
    /// Navigation kinds.
    /// </summary>
    public enum NavigationKind
    {
        /// <summary>
        /// Forwards navigation.
        /// </summary>
        Forwards,

        /// <summary>
        /// Backwards navigation.
        /// </summary>
        Backwards,

        /// <summary>
        /// Navigation to the ViewModel that was active before the app was suspended.
        /// </summary>
        Restore
    }
}