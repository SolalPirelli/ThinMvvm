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

        public class TestLogValueConverter : ILogValueConverter
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

        public interface ILogValueConverterEx : ILogValueConverter { }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void FailsOnAbstractClass()
        {
            new LogValueConverterAttribute( typeof( AbstractLogValueConverter ) );
        }

        public abstract class AbstractLogValueConverter : ILogValueConverter
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

        public class NotLogValueConverter { }
    }
}
