// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class INotifyPropertyChangedExtensionsTests
    {
        private sealed class TestNotifyPropertyChanged : ObservableObject
        {
            private int _property;

            public int Property
            {
                get { return _property; }
                set { SetProperty( ref _property, value ); }
            }

#pragma warning disable 0649 // unused field
            public int Field;
#pragma warning restore 0649
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ExceptionThrownOnListenToFieldExpression()
        {
            var inpc = new TestNotifyPropertyChanged();
            inpc.ListenToProperty( x => x.Field, () => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ExceptionThrownOnListenToConstantExpression()
        {
            var inpc = new TestNotifyPropertyChanged();
            inpc.ListenToProperty( x => 0, () => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ExceptionThrownOnNullObject()
        {
            INotifyPropertyChangedExtensions.ListenToProperty<TestNotifyPropertyChanged, int>( null, x => x.Property, () => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ExceptionThrownOnNullExpression()
        {
            new TestNotifyPropertyChanged().ListenToProperty<TestNotifyPropertyChanged, int>( null, () => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ExceptionThrownOnNullListener()
        {
            new TestNotifyPropertyChanged().ListenToProperty( x => x.Property, null );
        }

        [TestMethod]
        public void ListenToPropertyWorks()
        {
            var inpc = new TestNotifyPropertyChanged();
            int hit = 0;

            inpc.ListenToProperty( x => x.Property, () => hit++ );
            inpc.Property = 1;

            Assert.AreEqual( 1, hit );
        }
    }
}