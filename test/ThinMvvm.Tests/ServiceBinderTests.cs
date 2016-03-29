using System;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Tests for <see cref="ServiceBinder" />.
    /// </summary>
    public sealed class ServiceBinderTests
    {
        [Fact]
        public void GetThrowsOnNullType()
        {
            var binder = new ServiceBinder();

            Assert.Throws<ArgumentNullException>( () => binder.Get( null, null ) );
        }

        /// <summary>
        /// Tests for object construction that do not use arguments.
        /// </summary>
        public sealed class NoArguments
        {
            private interface IDependency1 { }

            private interface IDependency1Child : IDependency1 { }

            private abstract class Dependency1Child : IDependency1 { }

            private interface IDependency2 { }

            private sealed class Dependency1 : IDependency1 { }

            private sealed class Dependency2 : IDependency2 { }

            private sealed class IndependentService { }

            private sealed class DependentService1
            {
                public readonly IDependency1 Dependency;

                public DependentService1( IDependency1 dependency )
                {
                    Dependency = dependency;
                }
            }

            private sealed class DependentService12
            {
                public readonly IDependency1 Dependency1;
                public readonly IDependency2 Dependency2;

                public DependentService12( IDependency1 dependency1, IDependency2 dependency2 )
                {
                    Dependency1 = dependency1;
                    Dependency2 = dependency2;
                }
            }

            private sealed class RecursivelyDependentService
            {
                public readonly DependentService12 Service;
                public readonly IDependency1 Dependency;

                public RecursivelyDependentService( DependentService12 service, IDependency1 dependency )
                {
                    Service = service;
                    Dependency = dependency;
                }
            }

            private sealed class NoPublicConstructorService
            {
                private NoPublicConstructorService() { }
            }

            private sealed class MultipleConstructorsService
            {
                public readonly int Value;

                public MultipleConstructorsService()
                {
                    Value = 0;
                }

                public MultipleConstructorsService( int value )
                {
                    Value = value;
                }
            }

            private sealed class SinglePublicConstructorService
            {
                public readonly int Value;

                public SinglePublicConstructorService()
                {
                    Value = 0;
                }

                private SinglePublicConstructorService( int value )
                {
                    Value = value;
                }
            }

            [Fact]
            public void GetReturnsBoundClass()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1, Dependency1>();

                var inst = binder.Get( typeof( IDependency1 ), null );

                Assert.IsType( typeof( Dependency1 ), inst );
            }

            [Fact]
            public void GetReturnsBoundInstance()
            {
                var binder = new ServiceBinder();
                var dep = new Dependency1();
                binder.Bind<IDependency1>( dep );

                var inst = binder.Get( typeof( IDependency1 ), null );

                Assert.Equal( dep, inst );
            }

            [Fact]
            public void GetCanInstantiateUnknownIndependentClasses()
            {
                var binder = new ServiceBinder();

                var inst = binder.Get( typeof( IndependentService ), null );

                Assert.IsType( typeof( IndependentService ), inst );
            }

            [Fact]
            public void GetCanInstantiateUnknownClassesWithOneDependency()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1, Dependency1>();

                var inst = (DependentService1) binder.Get( typeof( DependentService1 ), null );

                Assert.IsType( typeof( Dependency1 ), inst.Dependency );
            }

            [Fact]
            public void GetCanInstantiateUnknownClassesWithMultipleDependencies()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1, Dependency1>();
                binder.Bind<IDependency2, Dependency2>();

                var inst = (DependentService12) binder.Get( typeof( DependentService12 ), null );

                Assert.IsType( typeof( Dependency1 ), inst.Dependency1 );
                Assert.IsType( typeof( Dependency2 ), inst.Dependency2 );
            }

            [Fact]
            public void GetUsesBoundInstancesWhenInstantiatingUnknownTypes()
            {
                var binder = new ServiceBinder();
                var dep = new Dependency1();
                binder.Bind<IDependency1>( dep );

                var inst = (DependentService1) binder.Get( typeof( DependentService1 ), null );

                Assert.Equal( dep, inst.Dependency );
            }

            [Fact]
            public void GetResolvesDependenciesRecursively()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1, Dependency1>();
                binder.Bind<IDependency2, Dependency2>();

                var inst = (RecursivelyDependentService) binder.Get( typeof( RecursivelyDependentService ), null );

                Assert.NotNull( inst.Dependency );
                Assert.NotNull( inst.Service );
                Assert.NotNull( inst.Service.Dependency1 );
            }

            [Fact]
            public void GetSelectsSinglePublicConstructor()
            {
                var binder = new ServiceBinder();

                var inst = (SinglePublicConstructorService) binder.Get( typeof( SinglePublicConstructorService ), null );

                Assert.Equal( 0, inst.Value );
            }

            [Fact]
            public void BindThrowsWhenRegisteringNullInstance()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentNullException>( () => binder.Bind<IDependency1>( null ) );
            }

            [Fact]
            public void BindThrowsWhenRegisteringToInterface()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Bind<IDependency1, IDependency1Child>() );
            }

            [Fact]
            public void BindThrowsWhenRegisteringToAbstractClass()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Bind<IDependency1, Dependency1Child>() );
            }

            [Fact]
            public void BindThrowsWhenBindingClassTwice()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1, Dependency1>();

                Assert.Throws<InvalidOperationException>( () => binder.Bind<IDependency1, Dependency1>() );
            }

            [Fact]
            public void BindThrowsWhenBindingInstanceTwice()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1>( new Dependency1() );

                Assert.Throws<InvalidOperationException>( () => binder.Bind<IDependency1>( new Dependency1() ) );
            }

            [Fact]
            public void BindClassThrowsWhenInstanceWasAlreadyBound()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1>( new Dependency1() );

                Assert.Throws<InvalidOperationException>( () => binder.Bind<IDependency1, Dependency1>() );
            }

            [Fact]
            public void BindInstanceThrowsWhenClassWasAlreadyBound()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency1, Dependency1>();

                Assert.Throws<InvalidOperationException>( () => binder.Bind<IDependency1>( new Dependency1() ) );
            }

            [Fact]
            public void GetThrowsOnUnknownDependency()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( DependentService1 ), null ) );
            }

            [Fact]
            public void GetThrowsWhenTypeHasNoPublicConstructor()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( NoPublicConstructorService ), null ) );
            }

            [Fact]
            public void GetThrowsWhenTypeHasMultiplePublicConstructors()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( MultipleConstructorsService ), null ) );
            }
        }

        /// <summary>
        /// Tests for object construction that use arguments.
        /// </summary>
        public sealed class Arguments
        {
            private interface IDependency { }

            private sealed class Dependency : IDependency { }

            private sealed class IndependentService
            {
                public readonly int Value;

                public IndependentService( int value )
                {
                    Value = value;
                }
            }

            private sealed class DependentService
            {
                public readonly IDependency Dependency;
                public readonly int Value;

                public DependentService( IDependency dependency, int value )
                {
                    Dependency = dependency;
                    Value = value;
                }
            }

            private sealed class TwoArgumentsService
            {
                public readonly int Value1;
                public readonly int Value2;

                public TwoArgumentsService( int value1, int value2 )
                {
                    Value1 = value1;
                    Value2 = value2;
                }
            }

            private sealed class RecursivelyDependentService
            {
                public readonly DependentService Service;
                public readonly int Value;

                public RecursivelyDependentService( DependentService service, int value )
                {
                    Service = service;
                    Value = value;
                }
            }

            [Fact]
            public void GetInstantiatesIndependentServiceWithArgument()
            {
                var binder = new ServiceBinder();

                var service = (IndependentService) binder.Get( typeof( IndependentService ), 42 );

                Assert.Equal( 42, service.Value );
            }

            [Fact]
            public void GetInstantiatesDependentServiceWithArgument()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency, Dependency>();

                var service = (DependentService) binder.Get( typeof( DependentService ), 100 );

                Assert.IsType( typeof( Dependency ), service.Dependency );
                Assert.Equal( 100, service.Value );
            }

            [Fact]
            public void GetThrowsWhenArgumentIsNotProvided()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( IndependentService ), null ) );
            }

            [Fact]
            public void GetThrowsWhenArgumentIsOfWrongType()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( IndependentService ), "hello" ) );
            }

            [Fact]
            public void GetThrowsWhenTooManyArgumentsAreExpected()
            {
                var binder = new ServiceBinder();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( TwoArgumentsService ), 42 ) );
            }

            [Fact]
            public void GetThrowsOnRecursiveDependencyRequiringArgument()
            {
                var binder = new ServiceBinder();
                binder.Bind<IDependency, Dependency>();

                Assert.Throws<ArgumentException>( () => binder.Get( typeof( RecursivelyDependentService ), 42 ) );
            }
        }
    }
}