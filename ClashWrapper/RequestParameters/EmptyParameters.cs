namespace ClashWrapper.RequestParameters
{
    public class EmptyParameters : BaseParameters
    {
        public override string BuildContent()
            => string.Empty;
    }
}
