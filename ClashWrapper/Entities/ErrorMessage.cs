using ClashWrapper.Models;

namespace ClashWrapper.Entities
{
    public class ErrorMessage
    {
        public string Error { get; private set; }
        public string Reason { get; private set; }

        internal ErrorMessage(ErrorModel model)
        {
            Error = model.Error;
            Reason = model.Reason;
        }
    }
}
