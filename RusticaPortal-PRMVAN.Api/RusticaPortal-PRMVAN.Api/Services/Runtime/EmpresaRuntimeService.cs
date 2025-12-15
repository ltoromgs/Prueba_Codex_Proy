// Services/Runtime/EmpresaRuntimeService.cs
using System.Threading.Tasks;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces; // IDocumentService, ILoginService, IEmpresaConfigService
using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;

public class EmpresaRuntimeService : IEmpresaRuntimeService
{
    private readonly IDocumentService _documentService;
    private readonly IEmpresaConfigService _empresaConfigService;
    private readonly ILoginService _loginService;

    public EmpresaRuntimeService(
        IDocumentService documentService,
        IEmpresaConfigService empresaConfigService,
        ILoginService loginService)
    {
        _documentService = documentService;
        _empresaConfigService = empresaConfigService;
        _loginService = loginService;
    }

    public async Task<EmpresaLoginResult> ResolveAndLoginAsync(string empresaId)
    {
        // 1) Validar parámetro y existencia de la empresa
        var valida = await _documentService.ValidaDatos(empresaId);
        if (!valida.Registered)
            return new EmpresaLoginResult { Ok = false, Error = valida };

        if (!int.TryParse(empresaId, out var idEmpresa))
        {
            return new EmpresaLoginResult
            {
                Ok = false,
                Error = new ResponseInformation
                {
                    Registered = false,
                    Message = "Parámetro 'Empresa' inválido",
                    Content = ""
                }
            };
        }

        var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
        if (cfg is null)
        {
            return new EmpresaLoginResult
            {
                Ok = false,
                Error = new ResponseInformation
                {
                    Registered = false,
                    Message = $"No existe configuración para Empresa={idEmpresa}",
                    Content = ""
                }
            };
        }

        // 2) Login con esa empresa
        var token = await _loginService.Login(cfg);
        if (string.IsNullOrEmpty(token))
        {
            return new EmpresaLoginResult
            {
                Ok = false,
                Error = new ResponseInformation
                {
                    Registered = false,
                    Message = "No fue posible conectarse al Service Layer",
                    Content = ""
                }
            };
        }

        // 3) Éxito: devolvemos cfg + token
        return new EmpresaLoginResult
        {
            Ok = true,
            Cfg = cfg,
            Token = token
        };
    }
}
