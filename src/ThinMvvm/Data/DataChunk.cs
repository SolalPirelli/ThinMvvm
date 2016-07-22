namespace ThinMvvm.Data
{
    public sealed class DataChunk<T> : IDataChunk
    {
        public T Value { get; }

        public DataStatus Status { get; }

        public DataErrors Errors { get; }


        public DataChunk( T value, DataStatus status, DataErrors errors )
        {
            Value = value;
            Status = status;
            Errors = errors;
        }


        object IDataChunk.Value => Value;


        public bool Equals( IDataChunk other )
        {
            if( other == null )
            {
                return false;
            }

            return ( Value == null ? other.Value == null : Value.Equals( other.Value ) )
                && Status == other.Status
                && Errors == other.Errors;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as IDataChunk );
        }

        public override int GetHashCode()
        {
            var hash = 7;
            hash += 31 * ( Value == null ? 0 : Value.GetHashCode() );
            hash += 31 * (int) Status;
            hash += 31 * Errors.GetHashCode();
            return hash;
        }
    }
}