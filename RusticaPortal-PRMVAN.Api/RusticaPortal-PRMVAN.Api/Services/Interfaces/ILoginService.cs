using System.Threading.Tasks;

namespace ArellanoCore.Api.Services.Interfaces
{
    public interface ILoginService
    {
        public Task<string> Login(string baseDatos);
    }
}
