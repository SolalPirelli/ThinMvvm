// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Logging.Tests
{
    [TestClass]
    public sealed class LogParameterAttributeTests
    {
        [TestMethod]
        public void Constructor_NoErrorOnNonEmptyString()
        {
            new LogParameterAttribute( "x" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void Constructor_ErrorOnWhitespaceString()
        {
            new LogParameterAttribute( "   \t" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void Constructor_ErrorOnEmptyString()
        {
            new LogParameterAttribute( "" );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Constructor_ErrorOnNullString()
        {
            new LogParameterAttribute( null );
        }
    }
}