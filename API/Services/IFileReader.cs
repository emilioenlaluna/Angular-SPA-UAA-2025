using System.Threading.Tasks;

namespace API.Services
{
    public interface IFileReader
    {
        Task<string> ReadAllTextAsync(string path);
    }
}
