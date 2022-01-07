using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace RobGarrett365.VanityApp
{
    public class Unregister
    {
        private readonly ILogger _logger;
        private ICosmosWrapper _cosmosWrapper;

        public Unregister(ILoggerFactory loggerFactory, ICosmosWrapper cosmosWrapper)
        {
            _logger = loggerFactory.CreateLogger<Unregister>();
            _cosmosWrapper = cosmosWrapper;
        }

        [Function("Unregister")]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Unregister/{vanity}")] HttpRequestData req, string vanity)
        {
            _logger.LogInformation("Url unregistration triggered.");
            try
            {
                if (!(await _cosmosWrapper.MappingExistsAsync(vanity)))
                {
                    _logger.LogInformation($"Asked to remove vanity {vanity}, which doesn't exist");
                    return;
                }
                await _cosmosWrapper.DeleteMappingAsync(vanity);
                _logger.LogInformation($"Removed mapping for {vanity}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister Url");
                throw;
            }
        }
    }
}
