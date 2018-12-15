using ClashWrapper.Models;

namespace ClashWrapper.Entities
{
    public class ErrorMessage
    {
        public string Message { get; private set; }
        public string Reason { get; private set; }

        internal ErrorMessage(ErrorModel model)
        {
            Message = model.Message;
            Reason = model.Reason;
        }
    }
}
