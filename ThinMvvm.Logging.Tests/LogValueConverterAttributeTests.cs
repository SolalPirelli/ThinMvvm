// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Logging.Tests
{
    [TestClass]
    public sealed class LogValueConverterAttributeTests
    {
        [TestMethod]
        public void SucceedsOnValidConverter()
        {
            new LogValueConverterAttribute( typeof( TestLogValueConverter ) );
        }

        private class TestLogValueConverter : ILogValueConverter
        {
            public string Convert( object value )
            {
                return "";
            }
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void FailsGraciouslyOnNull()
        {
            new LogValueConverterAttribute( null );
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FailsOnILogValueConverter()
        {
            new LogValueConverterAttribute( typeof( ILogValueConverter ) );
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FailsOnInterface()
        {
            new LogValueConverterAttribute( typeof( ILogValueConverterEx ) );
        }

        private interface ILogValueConverterEx : ILogValueConverter { }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FailsOnAbstractClass()
        {
            new LogValueConverterAttribute( typeof( AbstractLogValueConverter ) );
        }

        private abstract class AbstractLogValueConverter : ILogValueConverter
        {
            public string Convert( object value )
            {
                return "";
            }
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FailsOnClassNotImplementingILogValueConverter()
        {
            new LogValueConverterAttribute( typeof( NotLogValueConverter ) );
        }

        private class NotLogValueConverter { }
    }
}