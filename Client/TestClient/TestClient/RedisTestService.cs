using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    public class RedisTestService : BackgroundService
    {
        private readonly IDatabase _redis;
        private readonly ILogger<RedisTestService> _logger;

        public RedisTestService(IDatabase redis, ILogger<RedisTestService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting redis client test");

            var currentVal = _redis.StringGet("singlevalue");
            int currentValInt = 0;
            if (currentVal.HasValue && currentVal.TryParse(out currentValInt))
            {
                currentValInt++;
            }
            _redis.StringSet("singlevalue", currentValInt);

            var rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                _redis.SortedSetAdd("sortedset", i.ToString(), rand.NextDouble());
            }

            for (int i = 0; i < 10; i++)
            {
                _redis.HashSet("hash", i, i * 2);
            }

            for (int i = 0; i < 10; i++)
            {
                _redis.ListRightPush("list", i);
            }

            for (int i = 0; i < 10; i++)
            {
                var messageId = _redis.StreamAdd("stream", new[]
                {
                    new NameValueEntry("field1", 1),
                    new NameValueEntry("field2", 2)
                });
                Console.WriteLine("Added message {0} to stream", messageId);
            }

            _redis.StreamAdd("stream", "eof", RedisValue.Null);
            _logger.LogInformation("Finished redis client test");

            return Task.CompletedTask;
        }
    }
}
