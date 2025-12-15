using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Services.Interfaces
{
    public interface IDocumentService
    {
        public Task<ResponseInformation> PostInfo(RequestInformation requestInformation, string nomSistema, EmpresaConfig BaseDatos);
        public Task<ResponseInformation> UpdateInfo(RequestInformation requestInformation, string nomSistem, EmpresaConfig BaseDatos);
        public Task<DocumentGetDTO> GetInfo(RequestInformation requestInformation, string BaseDatos);
        public Task<ResponseInformation> GetInfoDB(RequestInformation requestInformation, string nomSistem, string BaseDatos);

        public Task<DocumentGetDTO> GetDoc(RequestInformation requestInformation, string nomSistem, string BaseDatos, string docEntry);

        public Task<AmountAvailableGetDTO> GetAmountAvailable(RequestInformation requestInformation, string nomSistem, string BaseDatos, string docEntry);
        public Task<AditionalInfomation> GetClienteMoneda(string project, string BaseDatos);
        public Task<string> GetOVByProject(string project, bool esOV, string BaseDatos);
        public Task<List<string>> GetOVByContract(string contract, string BaseDatos);
        public Task<ResponseInformation> DeleteInfo(RequestInformation requestInformation, string nomSistem, EmpresaConfig BaseDatos, string docEntry);
        public Task<ResponseInformation> ValidaDatos(string baseDatos);
        Task<ResponseInformation> ValidaRUC(string empresa, string ruc, RequestInformation requestInformation);        
        Task<ResponseInformation> GetMenuDB(string empresa, string userId);
        Task<ResponseInformation> GetRecepcionDB(string empresa);
        Task<ResponseInformation> GetFactoresDB(string empresa, string periodo, string tiendas);
        Task<ResponseInformation> GetTiendasActivas(string empresa);
        Task<ResponseInformation> GetContactoDB(string empresa);
    }
}
