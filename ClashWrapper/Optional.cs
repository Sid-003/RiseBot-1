namespace ClashWrapper
{
    public class Optional<T>
    {
        internal Optional()
        {
            Specified = false;
        }

        internal Optional(T value)
        {
            _value = value;
            Specified = true;
        }

        private readonly T _value;

        public bool Specified { get; }

        public T GetValueOrDefault()
        {
            return Specified ? _value : default;
        }
    }
}
