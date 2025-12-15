// Services/Runtime/EmpresaLoginResult.cs
using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.Information;


public class EmpresaLoginResult
{
    public bool Ok { get; set; }
    public ResponseInformation Error { get; set; }
    public EmpresaConfig Cfg { get; set; }
    public string Token { get; set; }
}

