// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Logging.Tests
{
    [TestClass]
    public sealed class CommandLoggingRequestTests
    {
        [TestMethod]
        public void Constructor_NoErrorOnNonNullObject()
        {
            new CommandLoggingRequest( new object() );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Constructor_ErrorOnNullObject()
        {
            new CommandLoggingRequest( null );
        }
    }
}