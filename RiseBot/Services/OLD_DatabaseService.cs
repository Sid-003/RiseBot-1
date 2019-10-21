/*
using LiteDB;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    //TODO this is shit

    //it really is

    public class DatabaseService
    {
        private const string DatabaseDir = "./Database.db";

        private const ulong GuildId = 351002726207062017;
        private const string ClanTag = "#2GGCRC90";

        public Guild Guild { get; private set; }

        private readonly SemaphoreSlim _readSemaphore;
        private readonly SemaphoreSlim _writeSemaphore;

        public DatabaseService()
        {
            _readSemaphore = new SemaphoreSlim(1);
            _writeSemaphore = new SemaphoreSlim(1);
        }

        public async Task<DatabaseService> LoadGuildAsync()
        {
            Guild = await GetEntityAsync<Guild>(GuildId);

            if (!(Guild is null))
            {
                _readSemaphore.Release();
                return this;
            }

            Guild = new Guild
            {
                Id = GuildId,
                ClanTag = ClanTag
            };

            await WriteEntityAsync(Guild);
            
            return this;
        }

        public async Task<T> GetEntityAsync<T>(ulong id = 0) where T : Entity
        {
            await _readSemaphore.WaitAsync();

            using (var db = new LiteDatabase(DatabaseDir))
            {
                var collection = db.GetCollection<Guild>("guilds");

                Entity entity = collection.FindOne(x => x.Id == GuildId);

                if (id == GuildId)
                {
                    _readSemaphore.Release();
                    return entity as T;
                }

                var guild = (Guild)entity;
                entity = guild.GuildMembers.FirstOrDefault(x => x.Id == id);

                _readSemaphore.Release();
                return entity as T;
            }
        }

        public async Task WriteEntityAsync(Guild guild)
        {
            await _writeSemaphore.WaitAsync();
            Guild = guild;

            using (var db = new LiteDatabase(DatabaseDir))
            {
                var collection = db.GetCollection<Guild>("guilds");
                collection.Upsert(guild);
            }

            _writeSemaphore.Release();
        }
    }
}
*/
