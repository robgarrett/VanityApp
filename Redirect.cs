using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace RobGarrett365.VanityApp
{
    public class Redirect
    {
        private readonly ILogger _logger;
        private ICosmosWrapper _cosmosWrapper;

        public Redirect(ILoggerFactory loggerFactory, ICosmosWrapper cosmosWrapper)
        {
            _logger = loggerFactory.CreateLogger<Redirect>();
            _cosmosWrapper = cosmosWrapper;
        }

        [Function("Redirect")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{vanity}")] HttpRequestData req, string vanity)
        {
            _logger.LogInformation($"Request to redirect {vanity}");
            HttpResponseData response;
            try
            {
                var url = await _cosmosWrapper.GetMappingAsync(vanity);
                if (!string.IsNullOrEmpty(url))
                {
                    _logger.LogInformation($"Found mapping {vanity} -> {url}");
                    response = req.CreateResponse(HttpStatusCode.Redirect);
                    response.Headers.Add("Location", url);
                }
                else
                {
                    _logger.LogWarning($"Mapping not found {vanity}");
                    response = req.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to redirect {vanity}");
                throw;
            }
            return response;
        }
    }
}
