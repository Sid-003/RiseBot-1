using Newtonsoft.Json;

namespace BandWrapper.Models
{
    internal class PostsModel : BandBaseModel<PostsDataModel>
    {
        [JsonProperty("result_code")]
        public override int ResultCode { get; set; }

        [JsonProperty("result_data")]
        public override PostsDataModel ResultData { get; set; }
    }

    internal class PostsDataModel
    {
        [JsonProperty("items")]
        public PostsItemModel[] Posts { get; set; }

        [JsonProperty("paging")]
        public PostsPagingModel Paging { get; set; }
    }

    internal class PostsItemModel
    {
        [JsonProperty("author")]
        public PostsAuthorModel Author { get; set; }

        [JsonProperty("post_key")]
        public string Key { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("photos")]
        public PostsPhotosModel[] Photos { get; set; }

        [JsonProperty("emotion_count")]
        public int EmotionCount { get; set; }

        [JsonProperty("band_key")]
        public string BandKey { get; set; }

        [JsonProperty("latest_comments")]
        public PostsCommentModel[] Comments { get; set; }
    }

    internal class PostsAuthorModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("member_type")]
        public string Type { get; set; }

        [JsonProperty("member_certified")]
        public bool Certified { get; set; }

        [JsonProperty("user_key")]
        public string Key { get; set; }
    }

    internal class PostsPhotosModel
    {
        [JsonProperty("author")]
        public PostsAuthorModel Author { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Wigth { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("emotion_count")]
        public int EmotionCount { get; set; }

        [JsonProperty("photo_key")]
        public string Key { get; set; }

        [JsonProperty("photo_album_key", NullValueHandling = NullValueHandling.Ignore)]
        public string AlbumKey { get; set; }

        [JsonProperty("is_video_thumbnail")]
        public bool IsVideoThumbnail { get; set; }
    }

    internal class PostsCommentModel
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("author")]
        public PostsCommentAuthorModel Author { get; set; }
    }

    internal class PostsCommentAuthorModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("user_key")]
        public string Key { get; set; }
    }

    internal class PostsPagingModel
    {
        [JsonProperty("previous_params", NullValueHandling = NullValueHandling.Ignore)]
        public string Previous { get; set; }

        [JsonProperty("next_params")]
        public PostsNextParamsModel Next { get; set; }
    }

    internal class PostsNextParamsModel
    {
        [JsonProperty("band_key")]
        public string BandKey { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }
}
