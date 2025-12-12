using ArellanoCore.Api.Entities.Dto;
using ArellanoCore.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace ArellanoCore.Api.Services
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
