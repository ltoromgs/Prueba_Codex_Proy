using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.Dto.AdministracionGAE;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministracionGAEController : ControllerBase
    {
        private static readonly Dictionary<string, string> SapObjectEndpointMap = new()
        {
            { "18", "PurchaseInvoices" },
            { "19", "PurchaseCreditNotes" },
            { "20", "GoodsReceiptPO" },
            { "22", "PurchaseOrders" },
            { "1470000113", "InventoryGenEntries" },
            { "1470000114", "InventoryGenExits" }
        };

        private readonly IDocumentService _documentService;
        private readonly IEmpresaRuntimeService _empresaRuntime;
        private readonly IEmpresaConfigService _empresaConfigService;
        private readonly ILogger<AdministracionGAEController> _logger;

        public AdministracionGAEController(
            IDocumentService documentService,
            IEmpresaRuntimeService empresaRuntime,
            IEmpresaConfigService empresaConfigService,
            ILogger<AdministracionGAEController> logger)
        {
            _documentService = documentService;
            _empresaRuntime = empresaRuntime;
            _empresaConfigService = empresaConfigService;
            _logger = logger;
        }

        [HttpGet("tiendas")]
        public async Task<ActionResult<ResponseInformation>> GetTiendas([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetAdministracionGaeTiendas(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tiendas de administración GAE para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("tipos-gae")]
        public async Task<ActionResult<ResponseInformation>> GetTiposGae([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetAdministracionGaeTiposGae(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos GAE para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("tipos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetTiposGasto([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetAdministracionGaeTiposGasto(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de gasto para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("motivos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetMotivosGasto([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetAdministracionGaeMotivosGasto(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener motivos de gasto para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<ResponseInformation>> Buscar([FromQuery] string Empresa,
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] string? tiendas = null,
            [FromQuery] string filtros = "")
        {
            try
            {
                var rp = await _documentService.GetAdministracionGaeBuscar(Empresa, fechaDesde, fechaHasta, tiendas ?? string.Empty, filtros ?? string.Empty);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar administración GAE para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpPost("actualizar-todo")]
        public async Task<ActionResult<ResponseInformation>> ActualizarTodo([FromQuery] string Empresa, [FromBody] AdministracionGaeUpdateRequest request)
        {
            if (request?.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se recibieron datos para actualizar.",
                    Content = string.Empty
                });
            }

            if (request.Items.Any(item => string.IsNullOrWhiteSpace(item.IdEmpresa)
                || string.IsNullOrWhiteSpace(item.NombreEmpresa)
                || string.IsNullOrWhiteSpace(item.ObjectType)
                || string.IsNullOrWhiteSpace(item.DocEntry)))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "Existen filas sin IdEmpresa, NombreEmpresa, ObjectType o DocEntry.",
                    Content = string.Empty
                });
            }

            var results = new List<AdministracionGaeUpdateResult>();

            var companyGroups = request.Items
                .GroupBy(x => new { x.IdEmpresa, x.NombreEmpresa })
                .ToList();

            foreach (var companyGroup in companyGroups)
            {
                if (!int.TryParse(companyGroup.Key.IdEmpresa, out var companyId))
                {
                    var msg = $"No se tiene acceso/configuración para la empresa {companyGroup.Key.IdEmpresa} - {companyGroup.Key.NombreEmpresa}";
                    AppendErrorResults(results, companyGroup.ToList(), msg);
                    continue;
                }

                var empresaConfig = _empresaConfigService.GetEmpresa(companyId);
                if (empresaConfig == null || !string.Equals(empresaConfig.Nombre?.Trim(), companyGroup.Key.NombreEmpresa?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var msg = $"No se tiene acceso/configuración para la empresa {companyGroup.Key.IdEmpresa} - {companyGroup.Key.NombreEmpresa}";
                    AppendErrorResults(results, companyGroup.ToList(), msg);
                    continue;
                }

                var prep = await _empresaRuntime.ResolveAndLoginAsync(companyGroup.Key.IdEmpresa);
                if (!prep.Ok)
                {
                    AppendErrorResults(results, companyGroup.ToList(), prep.Error?.Message ?? "No fue posible iniciar sesión en Service Layer.");
                    continue;
                }

                try
                {
                    var objectGroups = companyGroup
                        .GroupBy(item => new { item.ObjectType, item.DocEntry })
                        .ToList();

                    foreach (var objectGroup in objectGroups)
                    {
                        var updates = objectGroup.ToList();
                        try
                        {
                            var endpoint = ResolveEndpoint(objectGroup.Key.ObjectType);
                            var route = $"{endpoint}({objectGroup.Key.DocEntry})";
                            var payload = await BuildPayloadByObjectType(objectGroup.Key.ObjectType, updates, prep.Token, prep.Cfg);
                            var patchResult = await PatchServiceLayer(route, payload, prep.Token, prep.Cfg);

                            if (!patchResult.ok)
                            {
                                AppendErrorResults(results, updates, patchResult.error);
                                continue;
                            }

                            foreach (var line in updates)
                            {
                                results.Add(new AdministracionGaeUpdateResult
                                {
                                    IdEmpresa = line.IdEmpresa,
                                    NombreEmpresa = line.NombreEmpresa,
                                    BaseDatos = line.BaseDatos,
                                    ObjectType = line.ObjectType,
                                    DocEntry = line.DocEntry,
                                    LineId = line.LineId,
                                    Ok = true,
                                    Estado = "OK",
                                    Message = string.Empty,
                                    MensajeError = string.Empty
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            AppendErrorResults(results, updates, ex.Message);
                        }
                    }
                }
                finally
                {
                    await LogoutServiceLayer(prep.Cfg, prep.Token);
                }
            }

            var allOk = results.All(x => x.Ok);
            return Ok(new ResponseInformation
            {
                Registered = allOk,
                Message = allOk ? "Actualización realizada correctamente" : "Se encontraron errores al actualizar.",
                Content = JsonConvert.SerializeObject(results)
            });
        }

        private static string ResolveEndpoint(string objectType)
        {
            if (string.IsNullOrWhiteSpace(objectType))
                throw new Exception("ObjectType vacío.");

            if (objectType.StartsWith("@"))
                return objectType.Substring(1);

            if (SapObjectEndpointMap.TryGetValue(objectType, out var endpoint))
                return endpoint;

            throw new Exception($"ObjectType no soportado: {objectType}");
        }

        private static void AppendErrorResults(List<AdministracionGaeUpdateResult> results, List<AdministracionGaeUpdateLine> updates, string message)
        {
            foreach (var line in updates)
            {
                results.Add(new AdministracionGaeUpdateResult
                {
                    IdEmpresa = line.IdEmpresa,
                    NombreEmpresa = line.NombreEmpresa,
                    BaseDatos = line.BaseDatos,
                    ObjectType = line.ObjectType,
                    DocEntry = line.DocEntry,
                    LineId = line.LineId,
                    Ok = false,
                    Estado = "Error",
                    Message = message,
                    MensajeError = message
                });
            }
        }

        private async Task<string> BuildPayloadByObjectType(string objectType, List<AdministracionGaeUpdateLine> updates, string token, EmpresaConfig cfg)
        {
            if (objectType == "@MGS_CL_GASCAB")
            {
                var headerDetailPayload = new
                {
                    U_MGS_CL_TIPGAE = updates.FirstOrDefault()?.U_MGS_CL_TIPGAE,
                    U_MGS_CL_AUTORI = updates.FirstOrDefault()?.U_MGS_CL_AUTORI,
                    U_MGS_CL_TIPGAS = updates.FirstOrDefault()?.U_MGS_CL_TIPGAS,
                    U_MGS_CL_TIPMOP = updates.FirstOrDefault()?.U_MGS_CL_TIPMOP,
                    U_MGS_CL_IMPORT = updates.FirstOrDefault()?.U_MGS_CL_IMPORT,
                    U_MGS_CL_FEPRM = updates.FirstOrDefault()?.U_MGS_CL_FEPRM,
                    U_MGS_CL_VALIDO = updates.FirstOrDefault()?.U_MGS_CL_VALIDO,
                    MGS_CL_GASDETCollection = updates.Select(line => new
                    {
                        LineId = int.TryParse(line.LineId, out var lineId) ? lineId : 0,
                        line.U_MGS_CL_TIPGAE,
                        line.U_MGS_CL_AUTORI,
                        line.U_MGS_CL_TIPGAS,
                        line.U_MGS_CL_TIPMOP,
                        line.U_MGS_CL_IMPORT,
                        line.U_MGS_CL_FEPRM,
                        line.U_MGS_CL_VALIDO
                    }).ToList()
                };

                return JsonConvert.SerializeObject(headerDetailPayload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }

            if (objectType.StartsWith("@"))
            {
                var headerPayload = new
                {
                    U_MGS_CL_TIPGAE = updates.FirstOrDefault()?.U_MGS_CL_TIPGAE,
                    U_MGS_CL_AUTORI = updates.FirstOrDefault()?.U_MGS_CL_AUTORI,
                    U_MGS_CL_TIPGAS = updates.FirstOrDefault()?.U_MGS_CL_TIPGAS,
                    U_MGS_CL_TIPMOP = updates.FirstOrDefault()?.U_MGS_CL_TIPMOP,
                    U_MGS_CL_IMPORT = updates.FirstOrDefault()?.U_MGS_CL_IMPORT,
                    U_MGS_CL_FEPRM = updates.FirstOrDefault()?.U_MGS_CL_FEPRM,
                    U_MGS_CL_VALIDO = updates.FirstOrDefault()?.U_MGS_CL_VALIDO
                };

                return JsonConvert.SerializeObject(headerPayload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }

            var documentPayload = new
            {
                DocumentLines = updates.Select(line => new
                {
                    LineNum = int.TryParse(line.LineId, out var lineNum) ? lineNum : 0,
                    line.U_MGS_CL_TIPGAE,
                    line.U_MGS_CL_AUTORI,
                    line.U_MGS_CL_TIPGAS,
                    line.U_MGS_CL_TIPMOP,
                    line.U_MGS_CL_IMPORT,
                    line.U_MGS_CL_FEPRM,
                    line.U_MGS_CL_VALIDO
                }).ToList()
            };

            return JsonConvert.SerializeObject(documentPayload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static async Task<(bool ok, string error)> PatchServiceLayer(string route, string payload, string token, EmpresaConfig cfg)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

                var baseUrl = (cfg.ServiceLayer.sl_route ?? string.Empty).Trim();
                if (!baseUrl.EndsWith("/"))
                    baseUrl += "/";

                var request = (HttpWebRequest)WebRequest.Create(baseUrl + route);
                request.ContentType = "application/json";
                request.Method = "PATCH";

                var cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", token) { Domain = cfg.ServiceLayer.sl_value });
                request.CookieContainer = cookies;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(payload);
                }

                using var response = (HttpWebResponse)await request.GetResponseAsync();
                return (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent, string.Empty);
            }
            catch (WebException ex)
            {
                var statusCode = ex.Response is HttpWebResponse wr ? (int)wr.StatusCode : 0;
                var responseBody = string.Empty;
                if (ex.Response != null)
                {
                    using var sr = new StreamReader(ex.Response.GetResponseStream());
                    responseBody = await sr.ReadToEndAsync();
                }

                return (false, $"StatusCode: {statusCode}. {responseBody}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static async Task LogoutServiceLayer(EmpresaConfig cfg, string token)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

                var baseUrl = (cfg.ServiceLayer.sl_route ?? string.Empty).Trim();
                if (!baseUrl.EndsWith("/"))
                    baseUrl += "/";

                var request = (HttpWebRequest)WebRequest.Create(baseUrl + "Logout");
                request.ContentType = "application/json";
                request.Method = "POST";

                var cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", token) { Domain = cfg.ServiceLayer.sl_value });
                request.CookieContainer = cookies;

                using var _ = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch
            {
                // Evitar que un fallo de logout rompa el flujo.
            }
        }
    }
}
