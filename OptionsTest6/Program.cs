using Microsoft.Extensions.Options;

internal class Program
{
    internal class TestOptions
    {
    }

    internal class DummyService
    {

    }

    public class Worker : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public Worker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(500);

            var tasks = Enumerable.Range(0, 5).Select(_ => CreateTestOptionsAsync());
            await Task.WhenAll(tasks);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(60000, stoppingToken);
            }
        }

        private Task CreateTestOptionsAsync()
        {
            return Task.Run(() =>
            {
                Console.WriteLine("Creating TestOptions instance");
                using (var scope = serviceProvider.CreateScope())
                {
                    var optionsSnapshot = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
                    var options = optionsSnapshot.Get("testing");
                }
            });
        }
    }

    private static async Task Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddTransient<DummyService>();

                services.AddOptions<TestOptions>("testing")
                    .Configure<DummyService>((options, dummyService) =>
                    {
                        Console.WriteLine("Configuring TestOptions instance");
                    });


                services.AddHostedService<Worker>();
            })
            .Build();

        await host.RunAsync();
    }
}