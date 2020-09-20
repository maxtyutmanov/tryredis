using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hb = CreateHostBuilder(args);
            await hb.RunConsoleAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hb = Host.CreateDefaultBuilder(args);
            hb.ConfigureServices(svc =>
            {
                svc.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var cfg = provider.GetRequiredService<IConfiguration>();
                    var host = cfg["RedisHost"];
                    var pwd = cfg["RedisPassword"];
                    var connStr = $"{host},password={pwd}";
                    return ConnectionMultiplexer.Connect(connStr);
                });
                svc.AddTransient(provider => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
                svc.AddSingleton<IHostedService, RedisTestService>();
            });
            return hb;
        }
    }
}
