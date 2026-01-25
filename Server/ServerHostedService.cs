using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Server
{
    public class ServerHostedService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => Server.Start(stoppingToken), stoppingToken);
        }
    }
}
