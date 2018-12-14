using Model = BandWrapper.Models.ErrorMessageModel;

namespace BandWrapper.Entities
{
    public class ErrorMessage
    {
        private readonly Model _model;

        internal ErrorMessage(Model model)
        {
            _model = model;
        }

        public int Code => _model.ResultCode;

        public string Message => _model.ResultData.Message;
        public string Error => _model.ResultData.Detail.Error;
        public string Description => _model.ResultData.Detail.Descroption;
    }
}
