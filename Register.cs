using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RobGarrett365.VanityApp
{
    public class Register
    {
        private readonly ILogger _logger;
        private ICosmosWrapper _cosmosWrapper;

        public Register(ILoggerFactory loggerFactory, ICosmosWrapper cosmosWrapper)
        {
            _logger = loggerFactory.CreateLogger<Register>();
            _cosmosWrapper = cosmosWrapper;
        }

        [Function("Register")]
        [CosmosDBOutput("VanityAppDB", "Items", ConnectionStringSetting = "ConnectionString")]
        public async Task<Mapping> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Register")] HttpRequestData req)
        {
            _logger.LogInformation("Url registration triggered.");
            try
            {
                string requestBody;
                using (var reader = new StreamReader(req.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                    reader.Close();
                }
                // Check to see if document already exists in the DB.
                var jsonObj = JsonConvert.DeserializeObject<Mapping>(requestBody);
                if (await _cosmosWrapper.MappingExistsAsync(jsonObj.Vanity))
                {
                    // Delete the existing document.
                    _logger.LogInformation($"Found existing mapping with vanity {jsonObj.Vanity}");
                    await _cosmosWrapper.DeleteMappingAsync(jsonObj.Vanity);
                    _logger.LogInformation($"Deleted existing mapping");
                }
                _logger.LogInformation($"Adding mapping {jsonObj.Vanity} -> {jsonObj.Url}");
                // Make the vanity uppercase, so we don't have to deal with case issues.
                jsonObj.Vanity = jsonObj.Vanity.ToUpper();
                return jsonObj;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Url");
                throw;
            }
        }  
    }
}
