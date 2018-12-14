using BandWrapper.Entities;
using System;
using System.Threading.Tasks;

namespace BandWrapper
{
    public class BandClient
    {
        private readonly RequestClient _request;

        public BandClient(BandClientConfig config)
        {
            _request = new RequestClient(this, config);
        }

        public event Func<ErrorMessage, Task> Error;

        internal Task InternalErrorReceivedAsync(ErrorMessage error)
        {
            return Error is null ? Task.CompletedTask : Error.Invoke(error);
        }

        public event Func<string, Task> Log;

        internal Task InternalLogReceivedAsync(string message)
        {
            return Log is null ? Task.CompletedTask : Log.Invoke(message);
        }

        public async Task GetPostsAsync(string bandKey, string locale, int limit)
        {

        }
    }
}
