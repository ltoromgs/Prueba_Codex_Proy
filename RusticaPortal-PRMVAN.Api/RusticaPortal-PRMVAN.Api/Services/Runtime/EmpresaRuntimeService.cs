using System;
using System.Linq;
using System.Threading.Tasks;
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
        var valida = await _documentService.ValidaDatos(empresaId);
        if (!valida.Registered)
            return new EmpresaLoginResult { Ok = false, Error = valida };

        if (!int.TryParse(empresaId, out var idEmpresa))
        {
            return BuildError("Parámetro 'Empresa' inválido");
        }

        var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
        if (cfg is null)
        {
            return BuildError($"No existe configuración para Empresa={idEmpresa}");
        }

        return await LoginAsync(cfg);
    }

    public async Task<EmpresaLoginResult> ResolveAndLoginByDatabaseAsync(string baseDatos, string? idEmpresa)
    {
        if (string.IsNullOrWhiteSpace(baseDatos))
        {
            return BuildError("No se recibió base de datos para autenticación de Service Layer.");
        }

        EmpresaConfig cfg = null;
        if (int.TryParse(idEmpresa, out var id) && id > 0)
        {
            var byId = _empresaConfigService.GetEmpresa(id);
            if (byId != null && string.Equals(byId.ServiceLayer?.CompanyDB, baseDatos, StringComparison.OrdinalIgnoreCase))
            {
                cfg = byId;
            }
        }

        cfg ??= _empresaConfigService
            .GetEmpresas()
            .FirstOrDefault(e => string.Equals(e.ServiceLayer?.CompanyDB, baseDatos, StringComparison.OrdinalIgnoreCase));

        if (cfg == null)
        {
            return BuildError($"No se tiene acceso a la base de datos {baseDatos} (no está configurada), comunicate con el administrador.");
        }

        return await LoginAsync(cfg);
    }

    private async Task<EmpresaLoginResult> LoginAsync(EmpresaConfig cfg)
    {
        var token = await _loginService.Login(cfg);
        if (string.IsNullOrEmpty(token))
        {
            return BuildError("No fue posible conectarse al Service Layer");
        }

        return new EmpresaLoginResult
        {
            Ok = true,
            Cfg = cfg,
            Token = token
        };
    }

    private static EmpresaLoginResult BuildError(string message)
    {
        return new EmpresaLoginResult
        {
            Ok = false,
            Error = new ResponseInformation
            {
                Registered = false,
                Message = message,
                Content = string.Empty
            }
        };
    }
}
