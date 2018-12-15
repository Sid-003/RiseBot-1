using System;
using Model = ClashWrapper.Models.IconUrls;

namespace ClashWrapper.Entities.ClanMembers
{
    public sealed class IconUrls
    {
        private readonly Model _model;

        internal IconUrls(Model model)
        {
            _model = model;
        }

        private Uri _small;
        public Uri Small => _small ?? (_small = new Uri(_model.Small));

        private Uri _medium;
        public Uri Medium => _medium ?? (_medium = new Uri(_model.Medium));

        private Uri _tiny;
        public Uri Large => _tiny ?? (_tiny = new Uri(_model.Tiny));
    }
}
