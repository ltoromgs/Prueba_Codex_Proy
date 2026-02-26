using RusticaPortal_PRMVAN.Api.Entities.Dto;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Services.Interfaces
{
    public interface IEmpresaConfigService
    {
        EmpresaConfig GetEmpresa(int id);
        IReadOnlyList<EmpresaConfig> GetEmpresas();
    }
}
