namespace BandWrapper.Entities
{
    public class PaginatedEntity<T>
    {
        public string Before { get; set; }
        public string After { get; set; }
        public T Entity { get; set; }
    }
}
