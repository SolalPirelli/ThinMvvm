namespace ThinMvvm
{
    /// <summary>
    /// Special class to denote that a <see cref="ViewModel{TParameter}" /> has no constructor parameter.
    /// </summary>
    public sealed class NoParameter
    {
        /// <summary>
        /// Prevents instances of the <see cref="NoParameter" /> class from being created.
        /// </summary>
        private NoParameter() { }
    }
}