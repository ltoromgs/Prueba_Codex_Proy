// Services/Interfaces/IEmpresaRuntimeService.cs
using System.Threading.Tasks;

public interface IEmpresaRuntimeService
{
    Task<EmpresaLoginResult> ResolveAndLoginAsync(string empresaId);
}