using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using RusticaPortal_PRMVAN.Api.Entities.Dto.GrupoVan;
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
        Task<ResponseInformation> GetFactoresNuevoDB(string empresa);
        Task<ResponseInformation> GetTiendasActivas(string empresa);
        Task<ResponseInformation> GetContactoDB(string empresa);
        Task<ResponseInformation> GetGrupoVanTiendas(string empresa);
        Task<ResponseInformation> GetGrupoVanMaestro(string empresa);
        Task<ResponseInformation> GetGrupoVanPorTienda(string empresa, string tiendaCodigo);
        Task<ResponseInformation> GetGrupoVanArticulos(string empresa, string grupoCodigo);
        Task<ResponseInformation> SetGrupoVanPorTiendaBulk(string empresa, string tiendaCodigo, IEnumerable<VanGrupoDetalleDto> items);
        Task<ResponseInformation> SetGrupoVanArticulosBulk(string empresa, string grupoCodigo, IEnumerable<VanArticuloDetalleDto> items);
    }
}
