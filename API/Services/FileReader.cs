using System.IO;
using System.Threading.Tasks;



namespace API.Services
{
    public class FileReader : IFileReader
    {
        public Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }
    }
}
