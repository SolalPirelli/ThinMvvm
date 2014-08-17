using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class NavigatedEventArgsTests
    {
        [TestMethod]
        public void ConstructorWorks()
        {
            new NavigatedEventArgs( new object(), true );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorThrowsOnNullViewModel()
        {
            new NavigatedEventArgs( null, true );
        }
    }
}