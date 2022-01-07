using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace RobGarrett365.VanityApp
{
    public interface ICosmosWrapper
    {
        Task<bool> MappingExistsAsync(string vanity);

        Task<string> GetMappingAsync(string vanity);

        Task DeleteMappingAsync(string vanity);
    }
}