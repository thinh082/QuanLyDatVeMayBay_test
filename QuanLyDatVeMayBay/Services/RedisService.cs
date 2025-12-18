using StackExchange.Redis;

namespace QuanLyDatVeMayBay.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService()
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _db = redis.GetDatabase();
        }

        public IDatabase Db => _db;
    }
}
