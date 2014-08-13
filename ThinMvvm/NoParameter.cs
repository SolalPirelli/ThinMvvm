// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm
{
    /// <summary>
    /// Special class to denote that a <see cref="IViewModel{TParameter}"/> has no constructor parameter in addition to its dependencies.
    /// </summary>
    public sealed class NoParameter
    {
        /// <summary>
        /// Prevents instances of the <see cref="NoParameter"/> class from being created.
        /// </summary>
        private NoParameter() { }
    }
}