using BandWrapper.Entities;
using BandWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return Error is null ? Task.CompletedTask : Error(error);
        }

        public event Func<string, Task> Log;

        internal Task InternalLogReceivedAsync(string message)
        {
            return Log is null ? Task.CompletedTask : Log(message);
        }

        public async Task<IReadOnlyCollection<Entities.Posts.Post>> GetPostsAsync(string bandKey, string locale,
            int limit)
        {
            var model = await _request
                .SendAsync<PostsModel>($"/v2/band/posts?band_key={bandKey}&locale={locale}&limit={limit}")
                .ConfigureAwait(false);
            var posts = model.ResultData.Posts.Select(x => new Entities.Posts.Post(x));

            return new ReadOnlyCollection<Entities.Posts.Post>(posts, () => model.ResultData.Posts.Length);
        }

        public async Task<Entities.Post.Post> GetPostAsync(string bandKey, string postKey)
        {
            var model = await _request
                .SendAsync<PostModel>($"/v2.1/band/post?band_key={bandKey}&post_key={postKey}")
                .ConfigureAwait(false);

            return new Entities.Post.Post(model);
        }
    }
}
