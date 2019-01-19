using LiteDB;

namespace RiseBot.Services
{
    [Service]
    public class DatabaseService
    {
        private const string DatabaseDir = "./Database.db";
        private const ulong GuildId = 351002726207062017;

        private readonly LiteDatabase _database;

        private Guild _guild;
        public Guild Guild => _guild ?? (_guild = GetGuild());

        public DatabaseService()
        {
            _database = new LiteDatabase(DatabaseDir);
        }

        private Guild GetGuild()
        {
            var collection = _database.GetCollection<Guild>("guilds");
            return collection.FindOne(x => x.Id == GuildId);
        }

        public void UpdateGuild()
        {
            var collection = _database.GetCollection<Guild>("guilds");
            collection.Upsert(Guild);
        }
    }
}
