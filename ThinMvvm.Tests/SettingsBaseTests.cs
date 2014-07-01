using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class SettingsBaseTests
    {
        public sealed class TestSettingsStorage : ISettingsStorage
        {
            public Dictionary<string, object> Values = new Dictionary<string, object>();

            public bool IsDefined( string key )
            {
                return Values.ContainsKey( key );
            }

            public T Get<T>( string key )
            {
                return (T) Values[key];
            }

            public void Set( string key, object value )
            {
                Values[key] = value;
            }
        }

        public sealed class TestSettings : SettingsBase<TestSettings>
        {
            public bool Bool
            {
                get { return Get<bool>(); }
                set { Set( value ); }
            }

            public string String
            {
                get { return Get<string>(); }
                set { Set( value ); }
            }

            public int NoDefault
            {
                get { return Get<int>(); }
                set { Set( value ); }
            }

            public TestSettings( ISettingsStorage storage ) : base( storage ) { }

            protected override SettingsDefaultValues GetDefaultValues()
            {
                return new SettingsDefaultValues
                {
                    { x => x.Bool, () => true },
                    { x => x.String, () => "abc" }
                };
            }
        }

        [TestMethod]
        public void DefaultValueIsRespected()
        {
            var settings = new TestSettings( new TestSettingsStorage() );

            Assert.AreEqual( true, settings.Bool );
            Assert.AreEqual( "abc", settings.String );
        }

        [TestMethod]
        public void DefaultValueIsStored()
        {
            var storage = new TestSettingsStorage();
            var settings = new TestSettings( storage );

            GC.KeepAlive( settings.Bool );
            GC.KeepAlive( settings.String );

            Assert.IsTrue( storage.Values.ContainsKey( "ThinMvvm.Tests.SettingsBaseTests+TestSettings.Bool" ) );
            Assert.AreEqual( true, storage.Values["ThinMvvm.Tests.SettingsBaseTests+TestSettings.Bool"] );
            Assert.IsTrue( storage.Values.ContainsKey( "ThinMvvm.Tests.SettingsBaseTests+TestSettings.String" ) );
            Assert.AreEqual( "abc", storage.Values["ThinMvvm.Tests.SettingsBaseTests+TestSettings.String"] );
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void ExceptionThrownWhenNoDefault()
        {
            var settings = new TestSettings( new TestSettingsStorage() );
            GC.KeepAlive( settings.NoDefault );
        }

        [TestMethod]
        public void SetAndGetWorks()
        {
            var settings = new TestSettings( new TestSettingsStorage() );

            settings.String = "xyz";

            Assert.AreEqual( "xyz", settings.String );
        }
    }
}