using Model = BandWrapper.Models.PostsItemModel;

namespace BandWrapper.Entities.Posts
{
    public sealed class Post
    {
        private readonly Model _model;

        internal Post(Model model)
        {
            _model = model;
        }

        public string Key => _model.Key;
    }
}
