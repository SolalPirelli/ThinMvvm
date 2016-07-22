using ThinMvvm.Data;
using ThinMvvm.Tests.TestInfrastructure;
using Xunit;

namespace ThinMvvm.Tests.Data
{
    public sealed class DataChunkTests
    {
        [Fact]
        public void ConstructorSetsValue()
        {
            var chunk = new DataChunk<int>( 42, DataStatus.Normal, default( DataErrors ) );

            Assert.Equal( 42, chunk.Value );
        }

        [Fact]
        public void ConstructorSetsStatus()
        {
            var chunk = new DataChunk<int>( 0, DataStatus.Error, default( DataErrors ) );

            Assert.Equal( DataStatus.Error, chunk.Status );
        }

        [Fact]
        public void ConstructorSetsErrors()
        {
            var errors = new DataErrors( new MyException(), null, null );
            var chunk = new DataChunk<int>( 0, DataStatus.Normal, errors );

            Assert.Equal( errors, chunk.Errors );
        }

        [Fact]
        public void Equality()
        {
            EqualityTests.For<IDataChunk>( new DataChunk<string>( "X", DataStatus.Normal, default( DataErrors ) ) )
                .WithEqual( new DataChunk<string>( "X", DataStatus.Normal, default( DataErrors ) ) )
                .WithUnequal( new DataChunk<string>( null, DataStatus.Normal, default( DataErrors ) ) )
                .WithUnequal( new DataChunk<string>( "Y", DataStatus.Normal, default( DataErrors ) ) )
                .WithUnequal( new DataChunk<string>( "X", DataStatus.Error, default( DataErrors ) ) )
                .WithUnequal( new DataChunk<string>( "X", DataStatus.Normal, new DataErrors( new MyException(), null, null ) ) )
                .Test( includeOperators: false );

            EqualityTests.For<IDataChunk>( new DataChunk<string>( null, DataStatus.Error, default( DataErrors ) ) )
                .WithEqual( new DataChunk<string>( null, DataStatus.Error, default( DataErrors ) ) )
                .Test( includeOperators: false );
        }
    }
}