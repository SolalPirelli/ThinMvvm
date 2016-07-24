using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Data
{
    public sealed class DataErrorsTests
    {
        [Fact]
        public void ConstructorSetsFetch()
        {
            var ex = new MyException();
            var errors = new DataErrors( ex, null, null );

            Assert.Equal( ex, errors.Fetch );
        }

        [Fact]
        public void ConstructorSetsCache()
        {
            var ex = new MyException();
            var errors = new DataErrors( null, ex, null );

            Assert.Equal( ex, errors.Cache );
        }

        [Fact]
        public void ConstructorSetsProcess()
        {
            var ex = new MyException();
            var errors = new DataErrors( null, null, ex );

            Assert.Equal( ex, errors.Process );
        }

        [Fact]
        public void Equality()
        {
            EqualityTests.For( default( DataErrors ) ).Test();

            var ex = new MyException();
            EqualityTests.For( new DataErrors( ex, null, null ) )
                .WithUnequal( new DataErrors( new MyException(), null, null ) )
                .WithUnequal( new DataErrors( null, ex, null ) )
                .WithUnequal( new DataErrors( null, null, ex ) )
                .WithUnequal( new DataErrors( ex, ex, ex ) )
                .Test();

            EqualityTests.For( new DataErrors( null, ex, null ) ).Test();

            EqualityTests.For( new DataErrors( null, null, ex ) ).Test();
        }
    }
}