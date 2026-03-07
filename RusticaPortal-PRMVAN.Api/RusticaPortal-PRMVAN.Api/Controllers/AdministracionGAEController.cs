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
            // 1. VALIDACIONES INICIALES
            if (request?.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new ResponseInformation { Registered = false, Message = "No se recibieron datos para actualizar." });
            }

            if (request.Items.Any(item => string.IsNullOrWhiteSpace(item.IdEmpresa) || string.IsNullOrWhiteSpace(item.NombreEmpresa) ||
                                         string.IsNullOrWhiteSpace(item.ObjectType) || string.IsNullOrWhiteSpace(item.DocEntry)))
            {
                return BadRequest(new ResponseInformation { Registered = false, Message = "Existen filas sin IdEmpresa, NombreEmpresa, ObjectType o DocEntry." });
            }

            var results = new List<AdministracionGaeUpdateResult>();
            var companyGroups = request.Items.GroupBy(x => new { x.IdEmpresa, x.NombreEmpresa }).ToList();

            // 2. PROCESAMIENTO POR EMPRESA
            foreach (var companyGroup in companyGroups)
            {
                if (!int.TryParse(companyGroup.Key.IdEmpresa, out var companyId))
                {
                    AppendErrorResults(results, companyGroup.ToList(), $"ID de empresa inválido: {companyGroup.Key.IdEmpresa}");
                    continue;
                }

                // Validar configuración de la empresa en BD local
                var empresaConfig = _empresaConfigService.GetEmpresa(companyId);
                if (empresaConfig == null)
                {
                    AppendErrorResults(results, companyGroup.ToList(), $"No existe configuración para la empresa {companyId}");
                    continue;
                }

                // LOGIN INICIAL
                var prep = await _empresaRuntime.ResolveAndLoginAsync(companyGroup.Key.IdEmpresa);
                if (!prep.Ok)
                {
                    AppendErrorResults(results, companyGroup.ToList(), prep.Error?.Message ?? "Login inicial fallido en Service Layer");
                    continue;
                }

                var currentCfg = prep.Cfg;
                var currentToken = prep.Token;

                try
                {
                    // Agrupar por Objeto + DocEntry para procesar como un solo payload (Cabecera + Líneas)
                    var objectGroups = companyGroup.GroupBy(x => new { x.ObjectType, x.DocEntry });

                    foreach (var objectGroup in objectGroups)
                    {
                        var updates = objectGroup.ToList();
                        bool success = false;
                        string lastError = string.Empty;

                        // BUCLE DE REINTENTO (Máximo 2 intentos)
                        for (int attempt = 1; attempt <= 2; attempt++)
                        {
                            // A) PING LIGERO (Metadata)
                            var ping = await EnsureServiceLayerSession(currentToken, currentCfg);
                            if (!ping.ok && IsInvalidSessionError(ping.error))
                            {
                                if (attempt == 1)
                                {
                                    _logger.LogWarning("Empresa {ID}: Sesión expirada en PING. Reintentando login...", companyId);
                                    var relogin = await _empresaRuntime.ResolveAndLoginAsync(companyGroup.Key.IdEmpresa);
                                    if (relogin.Ok) { currentToken = relogin.Token; continue; }
                                }
                                lastError = "Invalid session (relogin failed)";
                                break;
                            }

                            // B) EJECUTAR OPERACIÓN (PATCH O UPSERT NUMÉRICO)
                            (bool ok, string error) opRes;
                            if (IsNumericObjectType(objectGroup.Key.ObjectType))
                            {
                                opRes = await UpsertGaeCabAndDetailForNumericObjectType(objectGroup.Key.ObjectType, objectGroup.Key.DocEntry, updates, currentToken, currentCfg);
                            }
                            else
                            {
                                var endpoint = ResolveEndpoint(objectGroup.Key.ObjectType);
                                var payload = await BuildPayloadByObjectType(objectGroup.Key.ObjectType, updates);
                                opRes = await PatchServiceLayer($"{endpoint}({objectGroup.Key.DocEntry})", payload, currentToken, currentCfg);
                            }

                            if (opRes.ok)
                            {
                                success = true;
                                break;
                            }
                            else if (attempt == 1 && IsInvalidSessionError(opRes.error))
                            {
                                _logger.LogWarning("Empresa {ID}: 401 en Operación. Reintentando login...", companyId);
                                var relogin = await _empresaRuntime.ResolveAndLoginAsync(companyGroup.Key.IdEmpresa);
                                if (relogin.Ok) { currentToken = relogin.Token; continue; }
                            }
                            lastError = opRes.error;
                        }

                        // C) REGISTRO DE RESULTADOS
                        if (success)
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
                                    Ok = true,
                                    Estado = "OK",
                                    Message = "Actualizado correctamente"
                                });
                            }
                        }
                        else
                        {
                            var finalMsg = IsInvalidSessionError(lastError) ? "Invalid session (relogin failed)" : lastError;
                            AppendErrorResults(results, updates, finalMsg);
                        }
                    }
                }
                finally
                {
                    // SIEMPRE CERRAR SESIÓN POR EMPRESA 
                    //no es recomendable ya que se puede reutizar
                    //await LogoutServiceLayer(currentCfg, currentToken);
                    _logger.LogInformation("Fin del procesamiento para empresa {ID}. Sesión mantenida en caché.", companyId);
                }
            }

            // 3. RESPUESTA FINAL
            var totalOk = results.All(x => x.Ok);
            return Ok(new ResponseInformation
            {
                Registered = totalOk,
                Message = totalOk ? "Actualización realizada correctamente" : "Se encontraron errores en algunas filas.",
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
                    U_MGS_CL_TIPGAS = updates.FirstOrDefault()?.U_MGS_CL_TIPGAS,
                    U_MGS_CL_TIPMOP = updates.FirstOrDefault()?.U_MGS_CL_TIPMOP,
                    U_MGS_CL_IMPORT = updates.FirstOrDefault()?.U_MGS_CL_IMPORT,
                    U_MGS_CL_FEPRM = updates.FirstOrDefault()?.U_MGS_CL_FEPRM,
                    U_MGS_CL_VALIDO = NormalizeYnForSave(updates.FirstOrDefault()?.U_MGS_CL_VALIDO),
                    U_MGS_CL_AUTORI = NormalizeYnForSave(updates.FirstOrDefault()?.U_MGS_CL_AUTORI),
                    MGS_CL_GASDETCollection = updates.Select(line => new
                    {
                        LineId = int.TryParse(line.LineId, out var lineId) ? lineId : 0,
                        line.U_MGS_CL_TIPGAE,
                        U_MGS_CL_AUTORI = NormalizeYnForSave(line.U_MGS_CL_AUTORI),
                        line.U_MGS_CL_TIPGAS,
                        line.U_MGS_CL_TIPMOP,
                        line.U_MGS_CL_IMPORT,
                        line.U_MGS_CL_FEPRM,
                        U_MGS_CL_VALIDO = NormalizeYnForSave(line.U_MGS_CL_VALIDO)
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
                    U_MGS_CL_AUTORI = NormalizeYnForSave(updates.FirstOrDefault()?.U_MGS_CL_AUTORI),
                    U_MGS_CL_TIPGAS = updates.FirstOrDefault()?.U_MGS_CL_TIPGAS,
                    U_MGS_CL_TIPMOP = updates.FirstOrDefault()?.U_MGS_CL_TIPMOP,
                    U_MGS_CL_IMPORT = updates.FirstOrDefault()?.U_MGS_CL_IMPORT,
                    U_MGS_CL_FEPRM = updates.FirstOrDefault()?.U_MGS_CL_FEPRM,
                    U_MGS_CL_VALIDO = NormalizeYnForSave(updates.FirstOrDefault()?.U_MGS_CL_VALIDO)
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

                _logger.LogInformation("AdministracionGAE Payload Numérico: ObjectType={ObjectType}, DocEntryOrigen={DocEntryOrigen}, DocNumVisual={DocNumVisual}, PrimerLineId={LineId}",
                    objectType,
                    docEntryOrigen,
                    docNumber,
                    first?.LineId);

                var createPayload = JsonConvert.SerializeObject(new
                {
                    U_MGS_CL_DOCNUM = docNumber,
                    U_MGS_CL_DOCENT = docEntryOrigen,
                    U_MGS_CL_OBJTYP = objectType,
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
                detail["U_MGS_CL_AUTORI"] = ToNullableToken(NormalizeYnForSave(line.U_MGS_CL_AUTORI));
                detail["U_MGS_CL_TIPGAS"] = ToNullableToken(line.U_MGS_CL_TIPGAS);
                detail["U_MGS_CL_TIPMOP"] = ToNullableToken(line.U_MGS_CL_TIPMOP);
                detail["U_MGS_CL_IMPORT"] = line.U_MGS_CL_IMPORT.HasValue ? JToken.FromObject(line.U_MGS_CL_IMPORT.Value) : null;
                detail["U_MGS_CL_FEPRM"] = ToNullableToken(line.U_MGS_CL_FEPRM);
                detail["U_MGS_CL_VALIDO"] = ToNullableToken(NormalizeYnForSave(line.U_MGS_CL_VALIDO));
                detail["U_MGS_CL_SOLICI"] = ToNullableToken(line.U_MGS_CL_SOLICI);
            }

            var updatePayload = JsonConvert.SerializeObject(new JObject
            {
                ["MGS_CL_GAEDETCollection"] = currentLines.collection
            }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            _logger.LogInformation("AdministracionGAE GAECAB upsert: ObjectType={ObjectType}, DocEntryOrigen={DocEntryOrigen}, endpoint={Endpoint}, Accion={Accion}, DocEntryUdo={DocEntryUdo}", objectType, docEntryOrigen, endpoint, wasCreated ? "Creado" : "Actualizado", gaeCabDocEntry);
            return await PatchServiceLayer($"{endpoint}({gaeCabDocEntry})", updatePayload, token, cfg);
        }

        private static string NormalizeYnForSave(string value)
        {
            var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized == "Y" || normalized == "SI") return "Y";
            if (normalized == "N" || normalized == "NO") return "N";
            return string.Empty;
        }

        private static bool IsInvalidSessionError(string error)
        {
            if (string.IsNullOrEmpty(error)) return false;
            // Service Layer a veces devuelve JSON con "code": 301 o HTTP 401
            return error.Contains("401") ||
                   error.Contains("Invalid session", StringComparison.OrdinalIgnoreCase) ||
                   error.Contains("\"code\":301");
        }

        private static async Task<(bool ok, string error)> EnsureServiceLayerSession(string token, EmpresaConfig cfg)
        {
            // Llamamos al metadata
            var res = await GetServiceLayer("$metadata", token, cfg);

            // Si res.ok es true, no nos importa si el body es un XML o un JObject ficticio,
            // significa que el Service Layer aceptó nuestra cookie de sesión.
            return (res.ok, res.error);
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
                // 1. Configuración de Seguridad
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;


                // 2. Construcción de URL
                var baseUrl = cfg.ServiceLayer.sl_route.Trim();
                if (!baseUrl.EndsWith("/")) baseUrl += "/";
                var fullUrl = baseUrl + route;

                var request = (HttpWebRequest)WebRequest.Create(fullUrl);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Timeout = 30000; // 30 segundos

                // 3. Configuración de Cookies (Aislamiento por Host)
                Uri uri = new Uri(baseUrl);
                var cookies = new CookieContainer();
                // Importante: Domain debe ser el Host (IP o Nombre del servidor)
                cookies.Add(new Cookie("B1SESSION", token) { Domain = uri.Host });
                request.CookieContainer = cookies;

                // 4. Ejecución y Lectura de Respuesta
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var result = await sr.ReadToEndAsync();

                    // --- LÓGICA DE FILTRADO DE CONTENIDO ---

                    // A. CONTROL DE PING (METADATA): Si responde XML, la sesión está VIVA.
                    // Evitamos JObject.Parse porque el XML empieza con '<' y daría error.
                    if (result.Trim().StartsWith("<?xml") || result.Contains("edmx:Edmx"))
                    {
                        // Devolvemos un objeto ficticio pero con ok = true
                        return (true, new JObject { ["ping"] = "pong", ["type"] = "xml" }, string.Empty);
                    }

                    // B. CONTROL DE JSON: Si la respuesta es un JSON válido (empieza con '{')
                    if (!string.IsNullOrWhiteSpace(result) && result.Trim().StartsWith("{"))
                    {
                        return (true, JObject.Parse(result), string.Empty);
                    }

                    // C. CASO RESPUESTA EXITOSA PERO VACÍA (204 No Content o similar)
                    return (true, new JObject(), string.Empty);
                }
            }
            catch (WebException ex)
            {
                // Capturamos errores de HTTP (401 Unauthorized, 404 Not Found, etc.)
                var statusCode = ex.Response is HttpWebResponse wr ? (int)wr.StatusCode : 0;
                string resp = "";
                if (ex.Response != null)
                {
                    using (var s = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        resp = await s.ReadToEndAsync();
                    }
                }

                // Si es 401, el mensaje dirá Unauthorized y activará tu lógica de reintento
                return (false, null, $"StatusCode: {statusCode}. {resp}");
            }
            catch (Exception ex)
            {
                // Errores genéricos de conexión o código
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }
        //private static async Task<(bool ok, JObject body, string error)> GetServiceLayer(string route, string token, EmpresaConfig cfg)
        //{
        //    try
        //    {

        //        var baseUrl = cfg.ServiceLayer.sl_route.Trim();

        //        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

        //        var httpWebGetRequest = (HttpWebRequest)WebRequest.Create(baseUrl + route);
        //        httpWebGetRequest.ContentType = "application/json";
        //        httpWebGetRequest.Method = "GET";
        //        CookieContainer cookies = new CookieContainer();
        //        cookies.Add(new Cookie("B1SESSION", token) { Domain = cfg.ServiceLayer.sl_value });
        //        //cookies.Add(new Cookie("ROUTEID", ".node1") { Domain = _configuration["ServiceLayer:ip_value"].ToString() });
        //        httpWebGetRequest.CookieContainer = cookies;

        //       using (var streamReader = new StreamReader(httpWebGetRequest.GetResponse().GetResponseStream()))
        //        {
        //            string result = await streamReader.ReadToEndAsync();

        //            // 1. CONTROL DE PING (METADATA)
        //            // Si la respuesta es XML de SAP, no intentamos parsear a JSON
        //            if (result.Trim().StartsWith("<?xml") || result.Contains("edmx:Edmx"))
        //            {
        //                // Devolvemos un JObject ficticio para no romper la firma del método
        //                // Lo importante es que ok = true
        //                return (true, new JObject { ["ping"] = "pong", ["type"] = "xml" }, string.Empty);
        //            }

        //            // 2. CONTROL DE JSON (CONSULTAS NORMALES)
        //            if (!string.IsNullOrWhiteSpace(result) && result.Trim().StartsWith("{"))
        //            {
        //                return (true, JObject.Parse(result), string.Empty);
        //            }

        //            // 3. CASO RESPUESTA VACÍA
        //            return (true, new JObject(), string.Empty);

        //        }               
        //    }
        //    catch (WebException ex)
        //    {
        //        var statusCode = ex.Response is HttpWebResponse wr ? (int)wr.StatusCode : 0;
        //        string resp = "";
        //        if (ex.Response != null)
        //        {
        //            using var s = new StreamReader(ex.Response.GetResponseStream());
        //            resp = await s.ReadToEndAsync();
        //        }
        //        return (false, null, $"StatusCode: {statusCode}. {resp}");
        //    }
        //    catch (Exception ex) { return (false, null, ex.Message); }
        //}
        //private static async Task<(bool ok, JObject body, string error)> GetServiceLayer(string route, string token, EmpresaConfig cfg)
        //        {
        //            try
        //            {
        //                // 1. Configuración de Seguridad
        //                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

        //                var baseUrl = cfg.ServiceLayer.sl_route.Trim();
        //                if (!baseUrl.EndsWith("/")) baseUrl += "/";

        //                var request = (HttpWebRequest)WebRequest.Create(baseUrl + route);
        //                request.Method = "GET";
        //                request.ContentType = "application/json";

        //                // 2. EXTRACCIÓN DEL HOST (Crucial para que la cookie funcione)
        //                // Si sl_route es "https://192.168.1.1:50000/b1s/v1/", extraemos "192.168.1.1"
        //                Uri uri = new Uri(baseUrl);
        //                string host = uri.Host;

        //                var cookies = new CookieContainer();
        //                cookies.Add(new Cookie("B1SESSION", token) { Domain = host });
        //                request.CookieContainer = cookies;

        //                using var response = (HttpWebResponse)await request.GetResponseAsync();
        //                using var sr = new StreamReader(response.GetResponseStream());
        //                var raw = await sr.ReadToEndAsync();
        //                return (true, string.IsNullOrWhiteSpace(raw) ? new JObject() : JObject.Parse(raw), string.Empty);
        //            }
        //            catch (WebException ex)
        //            {
        //                var statusCode = ex.Response is HttpWebResponse wr ? (int)wr.StatusCode : 0;
        //                string resp = "";
        //                if (ex.Response != null)
        //                {
        //                    using var s = new StreamReader(ex.Response.GetResponseStream());
        //                    resp = await s.ReadToEndAsync();
        //                }
        //                return (false, null, $"StatusCode: {statusCode}. {resp}");
        //            }
        //            catch (Exception ex) { return (false, null, ex.Message); }
        //        }

        private static async Task<(bool ok, string docEntry, string error)> PostServiceLayer(string route, string payload, string token, EmpresaConfig cfg)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                var baseUrl = cfg.ServiceLayer.sl_route.Trim();
                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                var request = (HttpWebRequest)WebRequest.Create(baseUrl + route);
                request.Method = "POST";
                request.ContentType = "application/json";

                Uri uri = new Uri(baseUrl);
                var cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", token) { Domain = uri.Host });
                request.CookieContainer = cookies;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    await sw.WriteAsync(payload);
                }

                using var response = (HttpWebResponse)await request.GetResponseAsync();
                using var sr = new StreamReader(response.GetResponseStream());
                var raw = await sr.ReadToEndAsync();
                var json = JObject.Parse(raw);

                return (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK,
                        json["DocEntry"]?.ToString() ?? string.Empty,
                        string.Empty);
            }
            catch (WebException ex)
            {
                var statusCode = ex.Response is HttpWebResponse wr ? (int)wr.StatusCode : 0;
                string resp = "";
                if (ex.Response != null)
                {
                    using var s = new StreamReader(ex.Response.GetResponseStream());
                    resp = await s.ReadToEndAsync();
                }
                return (false, string.Empty, $"StatusCode: {statusCode}. {resp}");
            }
            catch (Exception ex) { return (false, string.Empty, ex.Message); }
        }

        private static async Task<(bool ok, string error)> PatchServiceLayer(string route, string payload, string token, EmpresaConfig cfg)
        {
            HttpWebRequest httpWebGetRequest = null;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

                var baseUrl = cfg.ServiceLayer.sl_route.Trim();
                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                httpWebGetRequest = (HttpWebRequest)WebRequest.Create(baseUrl + route);
                httpWebGetRequest.Method = "PATCH";
                httpWebGetRequest.ContentType = "application/json";

                Uri uri = new Uri(baseUrl);
                var cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", token) { Domain = uri.Host });
                httpWebGetRequest.CookieContainer = cookies;

                using (var streamWriter = new StreamWriter(httpWebGetRequest.GetRequestStream()))
                { await streamWriter.WriteAsync(payload); }

                // 2. OBTENER RESPUESTA
                using (var response = (HttpWebResponse)await httpWebGetRequest.GetResponseAsync())
                {
                    // En PATCH, SAP devuelve 204 No Content. 
                    // No hace falta StreamReader a menos que quieras leer errores (que van al catch)
                    bool isOk = response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK;
                    return (isOk, string.Empty);
                }

                //using (var sw = new StreamWriter(request.GetRequestStream()))
                //{
                //    await sw.WriteAsync(payload);
                //}

                //using var response = (HttpWebResponse)await request.GetResponseAsync();
                //// El PATCH exitoso suele devolver 204 No Content o 200 OK
                //return (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK, string.Empty);
            }
            catch (WebException ex)
            {
                var statusCode = ex.Response is HttpWebResponse wr ? (int)wr.StatusCode : 0;
                string resp = "";
                if (ex.Response != null)
                {
                    using var s = new StreamReader(ex.Response.GetResponseStream());
                    resp = await s.ReadToEndAsync();
                }
                return (false, $"StatusCode: {statusCode}. {resp}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        //private async Task LogoutServiceLayer(EmpresaConfig cfg, string token)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(token)) return;

        //        // 1. ELIMINAR DEL CACHÉ LOCAL INMEDIATAMENTE
        //        // Generamos la misma llave que usa el LoginService
        //        var cacheKey = $"sl_session_empresa_{cfg.Id}";
        //        _cache.Remove(cacheKey);

        //        // 2. LOGOUT EN SAP
        //        var baseUrl = cfg.ServiceLayer.sl_route.Trim();
        //        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        //        var request = (HttpWebRequest)WebRequest.Create(baseUrl + "Logout");
        //        request.Method = "POST";

        //        Uri uri = new Uri(baseUrl);
        //        var cookies = new CookieContainer();
        //        cookies.Add(new Cookie("B1SESSION", token) { Domain = uri.Host });
        //        request.CookieContainer = cookies;

        //        using var response = (HttpWebResponse)await request.GetResponseAsync();
        //    }
        //    catch { /* Silencioso */ }
        //}
    }
}
