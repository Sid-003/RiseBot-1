namespace ClashWrapper.Entities
{
    public class PagedEntity<T>
    {
        public T Entity { get; internal set; }
        public string Before { get; internal set; }
        public string After { get; internal set; }

        internal PagedEntity() { }
    }
}
