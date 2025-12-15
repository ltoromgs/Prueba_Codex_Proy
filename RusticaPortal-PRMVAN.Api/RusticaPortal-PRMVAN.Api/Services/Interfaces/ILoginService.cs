using RusticaPortal_PRMVAN.Api.Entities.Dto;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Services.Interfaces
{
    public interface ILoginService
    {
        //public Task<string> Login(string baseDatos);
        public Task<string> Login(EmpresaConfig cfg);
    }
}
