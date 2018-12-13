using System;
using Model = ClashWrapper.Models.BadgeUrlModel;

namespace ClashWrapper.Entities
{
    public class BadgeUrls
    {
        private readonly Model _model;

        internal BadgeUrls(Model model)
        {
            _model = model;
        }

        private Uri _small;
        public Uri Small => _small ?? (_small = new Uri(_model.Small));

        private Uri _medium;
        public Uri Medium => _medium ?? (_medium = new Uri(_model.Medium));

        private Uri _large;
        public Uri Large => _large ?? (_large = new Uri(_model.Large));
    }
}
