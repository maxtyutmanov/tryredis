using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StreamConsumer
{
    public class RedisReader : BackgroundService
    {
        private readonly IDatabase _redis;
        private readonly ILogger<RedisReader> _logger;

        public RedisReader(IDatabase redis, ILogger<RedisReader> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var totalItems = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                var items = await _redis.StreamReadAsync("stream", 0, 3);
                if (items.Length == 0)
                {
                    await Task.Delay(1000);
                    continue;
                }
                totalItems += items.Length;

                if (!ProcessItems(items))
                {
                    break;
                }
            }

            _logger.LogInformation("Total items processed: {0}", totalItems);
        }

        private bool ProcessItems(StreamEntry[] items)
        {
            return items.All(ProcessItem);
        }

        private bool ProcessItem(StreamEntry entry)
        {
            if (entry.Values[0].Name == "eof")
            {
                _logger.LogInformation("End of stream");
                return false;
            }

            var serializedVals = JsonSerializer.Serialize(
                entry.Values.Select(x => new { Name =  x.Name.ToString(), Value = x.Value.ToString() }).ToList());
            _logger.LogInformation("Processed message {0} with values {1}", entry.Id, serializedVals);
            _redis.StreamDelete("stream", new[] { entry.Id });
            return true;
        }
    }
}
