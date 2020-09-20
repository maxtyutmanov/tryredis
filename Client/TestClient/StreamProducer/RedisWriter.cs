using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamProducer
{
    public class RedisWriter : BackgroundService
    {
        private readonly IDatabase _redis;
        private readonly ILogger<RedisWriter> _logger;

        public RedisWriter(IDatabase redis, ILogger<RedisWriter> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _redis.StreamCreateConsumerGroupAsync("stream", "main", createStream: true);

            var rand = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                var x = rand.Next();
                await _redis.StreamAddAsync("stream", "field1", x);
                await Task.Delay(100);
                _logger.LogInformation("Added item with field1 value {0}", x);
            }

            await _redis.StreamAddAsync("stream", "eof", RedisValue.Null);
        }
    }
}
