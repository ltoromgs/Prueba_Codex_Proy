using System.Threading.Tasks;

public interface IEmpresaRuntimeService
{
    Task<EmpresaLoginResult> ResolveAndLoginAsync(string empresaId);
    Task<EmpresaLoginResult> ResolveAndLoginByDatabaseAsync(string baseDatos, string? idEmpresa);
}
