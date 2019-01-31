namespace RiseBot.Entities
{
    public class CachedMessage
    {
        public ulong ExecutingId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public ulong ResponseId { get; set; }
        public long CreatedAt { get; set; }
    }
}
