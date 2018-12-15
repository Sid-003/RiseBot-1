using System;
using Model = BandWrapper.Models.PostScheduleModel;

namespace BandWrapper.Entities.Post
{
    public sealed class Schedule
    {
        private readonly Model _model;

        internal Schedule(Model model)
        {
            _model = model;
        }

        public string Name => _model.Name;
        
        public DateTimeOffset Start => DateTimeOffset.FromUnixTimeMilliseconds(_model.StartAt);
        public DateTimeOffset End => DateTimeOffset.FromUnixTimeMilliseconds(_model.EndAt);
    }
}
