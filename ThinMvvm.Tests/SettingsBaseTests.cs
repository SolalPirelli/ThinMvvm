// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class SettingsBaseTests
    {
        private sealed class TestSettingsStorage : ISettingsStorage
        {
            public Dictionary<string, object> Values = new Dictionary<string, object>();
            public Dictionary<string, int> SetCallsCounts = new Dictionary<string, int>();

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
                if ( Values.ContainsKey( key ) )
                {
                    Values[key] = value;
                    SetCallsCounts[key]++;
                }
                else
                {
                    Values.Add( key, value );
                    SetCallsCounts.Add( key, 1 );
                }
            }
        }

        private sealed class TestObservableObject : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public void FirePropertyChanged()
            {
                PropertyChanged( this, new PropertyChangedEventArgs( "" ) );
            }
        }

        // don't use ObservableCollection since it also implements INotifyPropertyChanged
        private sealed class TestObservableCollection : INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public void FireCollectionChanged()
            {
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
            }
        }

        private sealed class TestSettings : SettingsBase<TestSettings>
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

            public TestObservableObject ObservableObject
            {
                get { return Get<TestObservableObject>(); }
                set { Set( value ); }
            }

            public TestObservableCollection ObservableCollection
            {
                get { return Get<TestObservableCollection>(); }
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
                    { x => x.String, () => "abc" },
                    { x => x.ObservableObject, () => null },
                    { x => x.ObservableCollection, () => null }
                };
            }

            public void EnumerateOverDefaultValues()
            {
                foreach ( var o in GetDefaultValues() )
                {
                    o.ToString();
                }
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

            bool b = settings.Bool;
            string s = settings.String;

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
            int n = settings.NoDefault;
        }

        [TestMethod]
        public void SetAndGetWorks()
        {
            var settings = new TestSettings( new TestSettingsStorage() );

            settings.String = "xyz";

            Assert.AreEqual( "xyz", settings.String );
        }

        [TestMethod]
        public void SettingsAreSavedWhenNestedPropertyChanges()
        {
            var storage = new TestSettingsStorage();
            var settings = new TestSettings( storage );

            settings.ObservableObject = new TestObservableObject();
            settings.ObservableObject.FirePropertyChanged();

            Assert.AreEqual( 2, storage.SetCallsCounts["ThinMvvm.Tests.SettingsBaseTests+TestSettings.ObservableObject"] );
        }

        [TestMethod]
        public void SettingsAreSavedWhenObservableCollectionChanges()
        {
            var storage = new TestSettingsStorage();
            var settings = new TestSettings( storage );

            settings.ObservableCollection = new TestObservableCollection();
            settings.ObservableCollection.FireCollectionChanged();

            Assert.AreEqual( 2, storage.SetCallsCounts["ThinMvvm.Tests.SettingsBaseTests+TestSettings.ObservableCollection"] );
        }

        [TestMethod]
        [ExpectedException( typeof( NotSupportedException ) )]
        public void EnumeratingOverDefaultValuesThrows()
        {
            new TestSettings( new TestSettingsStorage() ).EnumerateOverDefaultValues();
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ErrorOnNullStorage()
        {
            new TestSettings( null );
        }
    }
}