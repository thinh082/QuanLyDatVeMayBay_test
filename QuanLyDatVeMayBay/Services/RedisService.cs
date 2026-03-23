using StackExchange.Redis;

namespace QuanLyDatVeMayBay.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService()
        {
            // Cấu hình chi tiết cho Upstash (hỗ trợ TLS và Password)
            var options = new ConfigurationOptions
            {
                EndPoints = { "eternal-rattler-78535.upstash.io:6379" },
                Password = "gQAAAAAAATLHAAIncDE5YTc5Mjg2Nzk4NTE0OWM4YmEwZTQwNmFiZjZjOTZhN3AxNzg1MzU",
                Ssl = true, // Upstash yêu cầu TLS nên phải bật Ssl = true
                AbortOnConnectFail = false,
            };

            var redis = ConnectionMultiplexer.Connect(options);
            _db = redis.GetDatabase();
        }

        public IDatabase Db => _db;
    }
}
