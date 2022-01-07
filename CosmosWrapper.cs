using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace RobGarrett365.VanityApp
{
    class CosmosWrapper : ICosmosWrapper, IDisposable
    {
        private const string _dbName = "VanityAppDB";
        private const string _collName = "Items";
        private const string _partitionKey = "/Vanity";
        private bool _disposed = false;
        private CosmosClient _cosmosClient = null;

        ~CosmosWrapper() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) _cosmosClient.Dispose();
            _cosmosClient = null;
            _disposed = true;
        }

        public async Task<bool> MappingExistsAsync(string vanity)
        {
            var container = await GetContainerAsync(_dbName, _collName);
            var queryDef = new QueryDefinition("SELECT * from m WHERE m.Vanity = @vanity").WithParameter("@vanity", vanity.ToUpper());
            using (var iterator = container.GetItemQueryIterator<JObject>(queryDef))
            {
                if (iterator.HasMoreResults)
                {
                    var resultSet = await iterator.ReadNextAsync();
                    if (resultSet.Any()) return true;
                }
            }
            return false;
        }

        public async Task<string> GetMappingAsync(string vanity)
        {
            var container = await GetContainerAsync(_dbName, _collName);
            var queryDef = new QueryDefinition("SELECT * from m WHERE m.Vanity = @vanity").WithParameter("@vanity", vanity.ToUpper());
            using (var iterator = container.GetItemQueryIterator<Mapping>(queryDef))
            {
                if (iterator.HasMoreResults)
                {
                    var resultSet = await iterator.ReadNextAsync();
                    if (resultSet.Any()) return resultSet.First().Url;
                }
            }    
            return null;
        }

        public async Task DeleteMappingAsync(string vanity)
        {
            var container = await GetContainerAsync(_dbName, _collName);
            vanity = vanity.ToUpper();
            var queryDef = new QueryDefinition("SELECT * from m WHERE m.Vanity = @vanity").WithParameter("@vanity", vanity);
            using (var iterator = container.GetItemQueryIterator<JObject>(queryDef))
            {
                if (iterator.HasMoreResults)
                {
                    var resultSet = await iterator.ReadNextAsync();
                    var result = resultSet.FirstOrDefault();
                    if (null != result)
                    {
                        // Get the id from the document.
                        var docId = result["id"].ToString();
                        await container.DeleteItemAsync<JObject>(docId, new PartitionKey(vanity));
                    }
                }
            }

        }

        private CosmosClient GetClient()
        {
            if (null == _cosmosClient)
                _cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("ConnectionString"));
            return _cosmosClient;
        }

        private async Task<Container> GetContainerAsync(string dbName, string collName)
        {
            var client = GetClient();
            var dbResponse = await client.CreateDatabaseIfNotExistsAsync(dbName);
            var uniqueKey = new UniqueKey();
            uniqueKey.Paths.Add(_partitionKey);
            var uniqueKeyPolicy = new UniqueKeyPolicy();
            uniqueKeyPolicy.UniqueKeys.Add(uniqueKey);
            var contResponse = await dbResponse.Database.CreateContainerIfNotExistsAsync(new ContainerProperties {
                Id = collName,
                UniqueKeyPolicy = uniqueKeyPolicy, 
                PartitionKeyPath = _partitionKey
            });
            return contResponse.Container;
        }
    }
}