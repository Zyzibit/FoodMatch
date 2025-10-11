// namespace inzynierka.Products.OpenFoodFacts.Import
// {
//     public interface IProductImporter
//     {
//         Task<ImportResult> ImportAsync(string path, int maxProducts, int batchSize);
//     }
// }

using System.Threading;
using System.Threading.Tasks;

namespace inzynierka.Products.OpenFoodFacts.Import
{
    public interface IProductImporter
    {
        Task ImportJsonlAsync(string filePath, CancellationToken ct = default);
    }
}
