// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class INotifyPropertyChangedExtensionsTests
    {
        public sealed class TestNotifyPropertyChanged : ObservableObject
        {
            private int _property;

            public int Property
            {
                get { return _property; }
                set { SetProperty( ref _property, value ); }
            }

            public int Field;
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ExceptionThrownOnListenToFieldExpression()
        {
            var inpc = new TestNotifyPropertyChanged();
            inpc.ListenToProperty<TestNotifyPropertyChanged, int>( x => x.Field, () => { } );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ExceptionThrownOnListenToConstantExpression()
        {
            var inpc = new TestNotifyPropertyChanged();
            inpc.ListenToProperty<TestNotifyPropertyChanged, int>( x => 0, () => { } );
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