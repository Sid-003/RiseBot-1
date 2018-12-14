namespace BandWrapper.Models
{
    internal abstract class BandBaseModel<T>
    {
        public abstract int ResultCode { get; set; }
        public abstract T ResultData { get; set; }
    }
}
