using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RobGarrett365.VanityApp;

namespace VanityApp
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(s => s.AddSingleton<ICosmosWrapper, CosmosWrapper>())
                .Build();
            host.Run();
        }
    }
}