namespace ThinMvvm.Data
{
    /// <summary>
    /// The states a form can be in.
    /// </summary>
    public enum FormStatus
    {
        /// <summary>
        /// The form was not initialized.
        /// </summary>
        None,

        /// <summary>
        /// The form is being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// The form has been initialized.
        /// </summary>
        Initialized,

        /// <summary>
        /// The form is being submitted.
        /// </summary>
        Submitting,

        /// <summary>
        /// The form has been submitted (at least once).
        /// </summary>
        Submitted
    }
}