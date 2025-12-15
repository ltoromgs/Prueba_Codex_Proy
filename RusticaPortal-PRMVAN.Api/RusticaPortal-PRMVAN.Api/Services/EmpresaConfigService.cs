using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace RusticaPortal_PRMVAN.Api.Services
{
    public class EmpresaConfigService : IEmpresaConfigService
    {
        private readonly List<EmpresaConfig> _empresas;
        public EmpresaConfigService(IOptions<List<EmpresaConfig>> opciones)
            => _empresas = opciones.Value;

        public EmpresaConfig GetEmpresa(int id)
            => _empresas.FirstOrDefault(e => e.Id == id);
    }
}
