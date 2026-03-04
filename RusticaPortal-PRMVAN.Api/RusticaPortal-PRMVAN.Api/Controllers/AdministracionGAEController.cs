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
                            if (IsNumericObjectType(objectGroup.Key.ObjectType))
                            {
                                var upsertResult = await UpsertGaeCabAndDetailForNumericObjectType(objectGroup.Key.ObjectType, objectGroup.Key.DocEntry, updates, prep.Token, prep.Cfg);
                                if (!upsertResult.ok)
                                {
                                    AppendErrorResults(results, updates, upsertResult.error);
                                    continue;
                                }
                            }
                            else
                            {
                                var endpoint = ResolveEndpoint(objectGroup.Key.ObjectType);
                                var route = $"{endpoint}({objectGroup.Key.DocEntry})";
                                var payload = await BuildPayloadByObjectType(objectGroup.Key.ObjectType, updates);
                                _logger.LogInformation("AdministracionGAE ActualizarTodo: ObjectType={ObjectType}, DocEntryOrigen={DocEntryOrigen}, endpoint={Endpoint}, route={Route}", objectGroup.Key.ObjectType, objectGroup.Key.DocEntry, endpoint, route);
                                var patchResult = await PatchServiceLayer(route, payload, prep.Token, prep.Cfg);

                                if (!patchResult.ok)
                                {
                                    AppendErrorResults(results, updates, patchResult.error);
                                    continue;
                                }
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

            if (IsNumericObjectType(objectType))
                return "MGS_CL_GAECAB";

            throw new Exception($"ObjectType no soportado: {objectType}");
        }

        private static bool IsNumericObjectType(string objectType)
        {
            return int.TryParse(objectType, out _);
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

        private Task<string> BuildPayloadByObjectType(string objectType, List<AdministracionGaeUpdateLine> updates)
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

                return Task.FromResult(JsonConvert.SerializeObject(headerDetailPayload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
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

                return Task.FromResult(JsonConvert.SerializeObject(headerPayload, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }

            throw new Exception($"ObjectType no soportado para payload directo: {objectType}");
        }

        private async Task<(bool ok, string error)> UpsertGaeCabAndDetailForNumericObjectType(string objectType, string docEntryOrigen, List<AdministracionGaeUpdateLine> updates, string token, EmpresaConfig cfg)
        {
            var endpoint = ResolveEndpoint(objectType);
            _logger.LogInformation("AdministracionGAE ActualizarTodo Numérico: ObjectType={ObjectType}, DocEntryOrigen={DocEntryOrigen}, endpoint={Endpoint}", objectType, docEntryOrigen, endpoint);

            var existing = await FindGaeCabByDocEntryAndObjectType(docEntryOrigen, objectType, token, cfg);
            if (!existing.ok)
                return (false, existing.error);

            var gaeCabDocEntry = existing.docEntry;
            var wasCreated = false;

            if (string.IsNullOrWhiteSpace(gaeCabDocEntry))
            {
                var first = updates.FirstOrDefault();
                var docNumber = !string.IsNullOrWhiteSpace(first?.DocNum)
                    ? first.DocNum
                    : (!string.IsNullOrWhiteSpace(first?.NumAtCard) ? first.NumAtCard : docEntryOrigen);

                var createPayload = JsonConvert.SerializeObject(new
                {
                    U_MGS_CL_DOCENT = docEntryOrigen,
                    U_MGS_CL_OBJTYP = objectType,
                    U_MGS_CL_DOCNUM = docNumber
                }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                var postResult = await PostServiceLayer(endpoint, createPayload, token, cfg);
                if (!postResult.ok)
                    return (false, postResult.error);

                gaeCabDocEntry = postResult.docEntry;
                wasCreated = true;
            }

            if (string.IsNullOrWhiteSpace(gaeCabDocEntry))
                return (false, "No se pudo determinar el DocEntry del UDO MGS_CL_GAECAB.");

            var currentLines = await GetCurrentGaeDetLines(gaeCabDocEntry, token, cfg);
            if (!currentLines.ok)
                return (false, currentLines.error);

            var lineMap = currentLines.lines;
            foreach (var line in updates)
            {
                _logger.LogInformation("AdministracionGAE Detalle Numérico: ObjectType={ObjectType}, DocEntryOrigen={DocEntryOrigen}, LineId={LineId}", objectType, docEntryOrigen, line.LineId);

                if (!int.TryParse(line.LineId, out var lineNum))
                    lineNum = 0;

                var detail = currentLines.collection
                    .OfType<JObject>()
                    .FirstOrDefault(x => string.Equals(x["U_MGS_CL_LINENUM"]?.ToString(), line.LineId, StringComparison.Ordinal));

                if (detail == null)
                {
                    detail = new JObject
                    {
                        ["U_MGS_CL_LINENUM"] = line.LineId
                    };
                    currentLines.collection.Add(detail);
                }

                if (lineMap.TryGetValue(lineNum, out var existingLineId))
                    detail["LineId"] = existingLineId;

                detail["U_MGS_CL_TIPGAE"] = ToNullableToken(line.U_MGS_CL_TIPGAE);
                detail["U_MGS_CL_AUTORI"] = ToNullableToken(line.U_MGS_CL_AUTORI?.Trim().ToUpper() == "SI" ? "Y" : (line.U_MGS_CL_AUTORI?.Trim().ToUpper() == "NO" ? "N" : line.U_MGS_CL_AUTORI));
                detail["U_MGS_CL_TIPGAS"] = ToNullableToken(line.U_MGS_CL_TIPGAS);
                detail["U_MGS_CL_TIPMOP"] = ToNullableToken(line.U_MGS_CL_TIPMOP);
                detail["U_MGS_CL_IMPORT"] = line.U_MGS_CL_IMPORT.HasValue ? JToken.FromObject(line.U_MGS_CL_IMPORT.Value) : null;
                detail["U_MGS_CL_FEPRM"] = ToNullableToken(line.U_MGS_CL_FEPRM);
                detail["U_MGS_CL_VALIDO"] = ToNullableToken(line.U_MGS_CL_VALIDO?.Trim().ToUpper() == "SI" ? "Y" : (line.U_MGS_CL_VALIDO?.Trim().ToUpper() == "NO" ? "N" : line.U_MGS_CL_VALIDO));
                detail["U_MGS_CL_SOLICI"] = ToNullableToken(line.U_MGS_CL_SOLICI);
            }

            var updatePayload = JsonConvert.SerializeObject(new JObject
            {
                ["MGS_CL_GAEDETCollection"] = currentLines.collection
            }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            _logger.LogInformation("AdministracionGAE GAECAB upsert: ObjectType={ObjectType}, DocEntryOrigen={DocEntryOrigen}, endpoint={Endpoint}, Accion={Accion}, DocEntryUdo={DocEntryUdo}", objectType, docEntryOrigen, endpoint, wasCreated ? "Creado" : "Actualizado", gaeCabDocEntry);
            return await PatchServiceLayer($"{endpoint}({gaeCabDocEntry})", updatePayload, token, cfg);
        }

        private static JToken ToNullableToken(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : JToken.FromObject(value);
        }

        private static async Task<(bool ok, string docEntry, string error)> FindGaeCabByDocEntryAndObjectType(string docEntryOrigen, string objectType, string token, EmpresaConfig cfg)
        {
            var filter = Uri.EscapeDataString($"U_MGS_CL_DOCENT eq '{docEntryOrigen}' and U_MGS_CL_OBJTYP eq '{objectType}'");
            var route = $"MGS_CL_GAECAB?$select=DocEntry&$filter={filter}";
            var getResult = await GetServiceLayer(route, token, cfg);
            if (!getResult.ok)
                return (false, string.Empty, getResult.error);

            var value = getResult.body?["value"] as JArray;
            var docEntry = value?.FirstOrDefault()?["DocEntry"]?.ToString() ?? string.Empty;
            return (true, docEntry, string.Empty);
        }

        private static async Task<(bool ok, Dictionary<int, int> lines, JArray collection, string error)> GetCurrentGaeDetLines(string gaeCabDocEntry, string token, EmpresaConfig cfg)
        {
            var route = $"MGS_CL_GAECAB({gaeCabDocEntry})";
            var getResult = await GetServiceLayer(route, token, cfg);
            if (!getResult.ok)
                return (false, new Dictionary<int, int>(), new JArray(), getResult.error);

            var collection = getResult.body?["MGS_CL_GAEDETCollection"] as JArray ?? new JArray();
            var map = new Dictionary<int, int>();
            foreach (var tokenLine in collection.OfType<JObject>())
            {
                var keyStr = tokenLine["U_MGS_CL_LINENUM"]?.ToString();
                if (!int.TryParse(keyStr, out var key))
                    continue;

                var lineId = tokenLine["LineId"]?.Value<int>() ?? 0;
                map[key] = lineId;
            }

            return (true, map, collection, string.Empty);
        }

        private static async Task<(bool ok, JObject body, string error)> GetServiceLayer(string route, string token, EmpresaConfig cfg)
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
                request.Method = "GET";

                var cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", token) { Domain = cfg.ServiceLayer.sl_value });
                request.CookieContainer = cookies;

                using var response = (HttpWebResponse)await request.GetResponseAsync();
                using var sr = new StreamReader(response.GetResponseStream());
                var raw = await sr.ReadToEndAsync();
                var json = string.IsNullOrWhiteSpace(raw) ? new JObject() : JObject.Parse(raw);
                return (true, json, string.Empty);
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

                return (false, null, $"StatusCode: {statusCode}. {responseBody}");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private static async Task<(bool ok, string docEntry, string error)> PostServiceLayer(string route, string payload, string token, EmpresaConfig cfg)
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
                request.Method = "POST";

                var cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", token) { Domain = cfg.ServiceLayer.sl_value });
                request.CookieContainer = cookies;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(payload);
                }

                using var response = (HttpWebResponse)await request.GetResponseAsync();
                using var sr = new StreamReader(response.GetResponseStream());
                var raw = await sr.ReadToEndAsync();
                var json = string.IsNullOrWhiteSpace(raw) ? new JObject() : JObject.Parse(raw);
                return (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK, json["DocEntry"]?.ToString() ?? string.Empty, string.Empty);
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

                return (false, string.Empty, $"StatusCode: {statusCode}. {responseBody}");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
            }
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
