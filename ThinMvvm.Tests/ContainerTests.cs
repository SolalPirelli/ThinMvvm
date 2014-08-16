// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public class ContainerTests
    {
        private interface IDependency { }
        private interface IDependencyEx : IDependency { }
        private abstract class AbstractDependency : IDependency { }
        private class Dependency : IDependency { }

        private interface IOtherDependency { }
        private class OtherDependency : IOtherDependency { }

        private class NotDependent
        {
            public NotDependent()
            {
            }
        }

        private class DependentOnOneDependency
        {
            public IDependency Dependency { get; private set; }

            public DependentOnOneDependency( IDependency dependency )
            {
                Dependency = dependency;
            }
        }

        private class DependentOnBothDependencies
        {
            public IDependency Dependency { get; private set; }
            public IOtherDependency OtherDependency { get; private set; }

            public DependentOnBothDependencies( IDependency dependency, IOtherDependency otherDependency )
            {
                Dependency = dependency;
                OtherDependency = otherDependency;
            }
        }

        private class WithTwoArguments
        {
            public WithTwoArguments( int a, int b )
            {
                // nothing
            }
        }

        private class NonAbstract
        {
            public NonAbstract( IDependency dependency ) { }
        }

        private class InheritsNonAbstract : NonAbstract
        {
            public InheritsNonAbstract( IDependency dependency, IOtherDependency otherDependency ) : base( dependency ) { }
        }

        private class WithTwoConstructors
        {
            public WithTwoConstructors( IDependency dep ) { }

            public WithTwoConstructors( IOtherDependency dep ) { }
        }

        [TestCleanup]
        public void Cleanup()
        {
            Container.Clear();
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Bind() should not allow binding a type to itself." )]
        public void BindDoesNotAllowBindingToOneself()
        {
            Container.Bind<object, object>();
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Bind() should not allow abstract implementations." )]
        public void BindDoesNotAllowAbstractImplementations()
        {
            Container.Bind<IDependency, AbstractDependency>();
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Bind() should not allow binding to an interface." )]
        public void BindDoesNotAllowInterfaceImplementations()
        {
            Container.Bind<IDependency, IDependencyEx>();
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ), "Bind() should not allow overrides." )]
        public void BindDoesNotAllowOverrides()
        {
            Container.Bind<IDependency, Dependency>();
            Container.Bind<IDependency, Dependency>();
        }

        [TestMethod]
        public void GetWorksWithNoDependencies()
        {
            Container.Get( typeof( NotDependent ), null );
            // no check - this should simply not throw any exceptions
        }

        [TestMethod]
        public void GetResolvesBoundDependency()
        {
            Container.Bind<IDependency, Dependency>();
            var obj = (DependentOnOneDependency) Container.Get( typeof( DependentOnOneDependency ), null );
            Assert.IsInstanceOfType( obj.Dependency, typeof( IDependency ), "Get() should resolve dependencies bound with Bind() correctly." );
        }

        [TestMethod]
        public void GetResolvesArgument()
        {
            var arg = new Dependency();
            var obj = (DependentOnOneDependency) Container.Get( typeof( DependentOnOneDependency ), arg );
            Assert.AreEqual( arg, obj.Dependency, "Get() should use the provided argument." );
        }

        [TestMethod]
        public void GetResolvesArgumentAndDependency()
        {
            var arg = new Dependency();
            Container.Bind<IOtherDependency, OtherDependency>();
            var obj = (DependentOnBothDependencies) Container.Get( typeof( DependentOnBothDependencies ), arg );
            Assert.AreEqual( arg, obj.Dependency, "Get() should use the provided argument." );
            Assert.IsInstanceOfType( obj.OtherDependency, typeof( IOtherDependency ), "Get() should resolve dependencies bound with Bind() correctly." );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Get() should fail if a constructor argument is ambiguous between a dependency and the argument." )]
        public void GetFailsOnAmbiguousConstructorArguments()
        {
            Container.Bind<IDependency, Dependency>();
            Container.Get( typeof( DependentOnOneDependency ), new Dependency() );
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ), "Get() should fail if the additional argument is required twice." )]
        public void GetFailsOnDuplicateArgument()
        {
            Container.Get( typeof( WithTwoArguments ), 42 );
        }

        [TestMethod]
        public void GetResolvesNonAbstractObject()
        {
            Container.Bind<IDependency, Dependency>();
            Container.Bind<IOtherDependency, OtherDependency>();
            Container.Bind<NonAbstract, InheritsNonAbstract>();

            var obj = Container.Get( typeof( NonAbstract ), null );

            Assert.IsInstanceOfType( obj, typeof( InheritsNonAbstract ) );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Get() should throw if no implementation was registered for an interface." )]
        public void ErrorOnInterfaceGetWithoutImplementation()
        {
            Container.Get( typeof( IDependency ), null );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Get() should throw if no implementation was registered for an abstract class." )]
        public void ErrorOnAbstractGetWithoutImplementation()
        {
            Container.Get( typeof( AbstractDependency ), null );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ), "Get() should throw if a class has more than one constructor." )]
        public void ErrorOnGetWhenMoreThanOneConstructor()
        {
            Container.Get( typeof( WithTwoConstructors ), null );
        }
    }
}