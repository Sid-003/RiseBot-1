using Model = BandWrapper.Models.PostModel;

namespace BandWrapper.Entities.Post
{
    public sealed class Post
    {
        private readonly Model _model;

        internal Post(Model model)
        {
            _model = model;
        }

        private Schedule _schedule;

        public Schedule Schedule => _model.ResultData.Post.Schedule is null
            ? null
            : _schedule ?? (_schedule = new Schedule(_model.ResultData.Post.Schedule));

        public string Content => _model.ResultData.Post.Content;
    }
}
