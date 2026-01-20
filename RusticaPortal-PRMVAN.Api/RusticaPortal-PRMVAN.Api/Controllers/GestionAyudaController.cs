using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GestionAyudaController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IEmpresaRuntimeService _empresaRuntime;
        private readonly ILogger<GestionAyudaController> _logger;

        public GestionAyudaController(IDocumentService documentService, IEmpresaRuntimeService empresaRuntime, ILogger<GestionAyudaController> logger)
        {
            _documentService = documentService;
            _empresaRuntime = empresaRuntime;
            _logger = logger;
        }

        [HttpGet("tiendas")]
        public async Task<ActionResult<ResponseInformation>> GetTiendas([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetGestionAyudaTiendas(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tiendas para gestión de ayuda: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado al obtener tiendas.",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("tipos")]
        public async Task<ActionResult<ResponseInformation>> GetTipos([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetGestionAyudaTipos(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos para gestión de ayuda: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado al obtener tipos.",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<ResponseInformation>> Buscar([FromQuery] string Empresa, [FromQuery] string periodo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(periodo))
                {
                    return BadRequest(new ResponseInformation
                    {
                        Registered = false,
                        Message = "El periodo es obligatorio.",
                        Content = string.Empty
                    });
                }

                var cabResponse = await _documentService.GetGestionAyudaCabPeriodo(Empresa, periodo);
                if (!cabResponse.Registered || string.IsNullOrWhiteSpace(cabResponse.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No existe información para el periodo.",
                        Content = string.Empty
                    });
                }

                var cabeceras = JsonConvert.DeserializeObject<List<GestionAyudaCabDto>>(cabResponse.Content) ?? new List<GestionAyudaCabDto>();
                var cabecera = cabeceras.FirstOrDefault();
                if (cabecera == null || string.IsNullOrWhiteSpace(cabecera.DocEntry))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No existe información para el periodo.",
                        Content = string.Empty
                    });
                }

                var detalleResponse = await _documentService.GetGestionAyudaDetalle(Empresa, cabecera.DocEntry);
                if (!detalleResponse.Registered || string.IsNullOrWhiteSpace(detalleResponse.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No existe detalle para el periodo.",
                        Content = string.Empty
                    });
                }

                var detalles = JsonConvert.DeserializeObject<List<GestionAyudaDetalleDto>>(detalleResponse.Content) ?? new List<GestionAyudaDetalleDto>();
                var payload = new
                {
                    DocEntry = cabecera.DocEntry,
                    Periodo = cabecera.U_MGS_CL_PERIODO,
                    Detalle = detalles
                };

                return Ok(new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(payload)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar gestión de ayuda para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado al buscar la gestión de ayuda.",
                    Content = ex.Message
                });
            }
        }

        [HttpPost("nuevo")]
        public async Task<ActionResult<ResponseInformation>> Nuevo([FromQuery] string Empresa)
        {
            try
            {
                var tiposResponse = await _documentService.GetGestionAyudaTipos(Empresa);
                if (!tiposResponse.Registered || string.IsNullOrWhiteSpace(tiposResponse.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontraron tipos activos para gestión de ayuda.",
                        Content = tiposResponse.Content
                    });
                }

                var tipos = JsonConvert.DeserializeObject<List<GestionAyudaTipoDto>>(tiposResponse.Content) ?? new List<GestionAyudaTipoDto>();
                var codigoDefecto = tipos.FirstOrDefault(t => string.Equals(t.Name, "Por defecto", StringComparison.OrdinalIgnoreCase))?.Code;
                if (string.IsNullOrWhiteSpace(codigoDefecto))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontró el tipo 'Por defecto'.",
                        Content = string.Empty
                    });
                }

                var tiendasResponse = await _documentService.GetGestionAyudaTiendas(Empresa);
                if (!tiendasResponse.Registered || string.IsNullOrWhiteSpace(tiendasResponse.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontraron tiendas para crear el documento.",
                        Content = tiendasResponse.Content
                    });
                }

                var tiendas = JsonConvert.DeserializeObject<List<GestionAyudaTiendaDto>>(tiendasResponse.Content) ?? new List<GestionAyudaTiendaDto>();

                var ultimoPeriodoResponse = await _documentService.GetGestionAyudaUltPeriodo(Empresa);
                var ultimoPeriodo = ultimoPeriodoResponse.Registered && !string.IsNullOrWhiteSpace(ultimoPeriodoResponse.Content)
                    ? JsonConvert.DeserializeObject<string>(ultimoPeriodoResponse.Content)
                    : string.Empty;

                var periodoNuevo = CalcularPeriodoSiguiente(ultimoPeriodo);

                var cabResponse = await _documentService.GetGestionAyudaCabPeriodo(Empresa, periodoNuevo);
                if (cabResponse.Registered && !string.IsNullOrWhiteSpace(cabResponse.Content))
                {
                    var cabeceras = JsonConvert.DeserializeObject<List<GestionAyudaCabDto>>(cabResponse.Content) ?? new List<GestionAyudaCabDto>();
                    var cabecera = cabeceras.FirstOrDefault();
                    if (cabecera == null || string.IsNullOrWhiteSpace(cabecera.DocEntry))
                    {
                        return Ok(new ResponseInformation
                        {
                            Registered = false,
                            Message = "No se pudo cargar la cabecera existente.",
                            Content = string.Empty
                        });
                    }

                    var detalleResponse = await _documentService.GetGestionAyudaDetalle(Empresa, cabecera.DocEntry);
                    var detalles = detalleResponse.Registered && !string.IsNullOrWhiteSpace(detalleResponse.Content)
                        ? JsonConvert.DeserializeObject<List<GestionAyudaDetalleDto>>(detalleResponse.Content) ?? new List<GestionAyudaDetalleDto>()
                        : new List<GestionAyudaDetalleDto>();

                    var completado = CompletarTiendas(detalles, tiendas, codigoDefecto);
                    var payload = new
                    {
                        DocEntry = cabecera.DocEntry,
                        Periodo = periodoNuevo,
                        Detalle = completado
                    };

                    return Ok(new ResponseInformation
                    {
                        Registered = true,
                        Message = string.Empty,
                        Content = JsonConvert.SerializeObject(payload)
                    });
                }

                var request = new GestionAyudaCreateRequest
                {
                    U_MGS_CL_PERIODO = periodoNuevo,
                    MGS_CL_GESDETCollection = tiendas.Select(t => new GestionAyudaDetalleCreateDto
                    {
                        U_MGS_CL_TIENDA = t.PrjCode,
                        U_MGS_CL_NOMTIE = t.PrjName,
                        U_MGS_CL_CVENTA = codigoDefecto,
                        U_MGS_CL_CRENTA = codigoDefecto,
                        U_MGS_CL_CVAN = codigoDefecto,
                        U_MGS_CL_CPERSO = codigoDefecto,
                        U_MGS_CL_CGESTI = codigoDefecto,
                        U_MGS_CL_CSERV = codigoDefecto,
                        U_MGS_CL_CCC = codigoDefecto,
                        U_MGS_CL_CADM = codigoDefecto
                    }).ToList()
                };

                var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
                if (!prep.Ok)
                {
                    return BadRequest(prep.Error);
                }

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var requestInformation = new RequestInformation
                {
                    Route = "MGS_CL_GESCAB",
                    Token = prep.Token,
                    Doc = JsonConvert.SerializeObject(request, settings)
                };

                var crearResponse = await _documentService.PostInfo(requestInformation, "PYP", prep.Cfg);
                if (!crearResponse.Registered)
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = $"Error al crear doc: {crearResponse.Message}",
                        Content = crearResponse.Content
                    });
                }

                var cabCreado = await _documentService.GetGestionAyudaCabPeriodo(Empresa, periodoNuevo);
                if (!cabCreado.Registered || string.IsNullOrWhiteSpace(cabCreado.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = true,
                        Message = "Documento creado, pero no se pudo recuperar la cabecera.",
                        Content = string.Empty
                    });
                }

                var cabeceraCreada = (JsonConvert.DeserializeObject<List<GestionAyudaCabDto>>(cabCreado.Content) ?? new List<GestionAyudaCabDto>()).FirstOrDefault();
                if (cabeceraCreada == null || string.IsNullOrWhiteSpace(cabeceraCreada.DocEntry))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = true,
                        Message = "Documento creado, pero no se pudo recuperar la cabecera.",
                        Content = string.Empty
                    });
                }

                var detalleCreado = await _documentService.GetGestionAyudaDetalle(Empresa, cabeceraCreada.DocEntry);
                var detallesCreado = detalleCreado.Registered && !string.IsNullOrWhiteSpace(detalleCreado.Content)
                    ? JsonConvert.DeserializeObject<List<GestionAyudaDetalleDto>>(detalleCreado.Content) ?? new List<GestionAyudaDetalleDto>()
                    : new List<GestionAyudaDetalleDto>();

                var payloadCreado = new
                {
                    DocEntry = cabeceraCreada.DocEntry,
                    Periodo = periodoNuevo,
                    Detalle = CompletarTiendas(detallesCreado, tiendas, codigoDefecto)
                };

                return Ok(new ResponseInformation
                {
                    Registered = true,
                    Message = "Documento creado correctamente.",
                    Content = JsonConvert.SerializeObject(payloadCreado)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear gestión de ayuda para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado al crear el documento.",
                    Content = ex.Message
                });
            }
        }

        [HttpPost("actualizar")]
        public async Task<ActionResult<ResponseInformation>> Actualizar([FromQuery] string docEntry, [FromQuery] string Empresa, [FromBody] GestionAyudaUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(docEntry))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "DocEntry inválido",
                    Content = string.Empty
                });
            }

            if (request?.MGS_CL_GESDETCollection == null || request.MGS_CL_GESDETCollection.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se enviaron registros para actualizar",
                    Content = string.Empty
                });
            }

            try
            {
                var tiposResponse = await _documentService.GetGestionAyudaTipos(Empresa);
                if (!tiposResponse.Registered || string.IsNullOrWhiteSpace(tiposResponse.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontraron tipos activos para gestionar actualización.",
                        Content = tiposResponse.Content
                    });
                }

                var tipos = JsonConvert.DeserializeObject<List<GestionAyudaTipoDto>>(tiposResponse.Content) ?? new List<GestionAyudaTipoDto>();
                var codigoDefecto = tipos.FirstOrDefault(t => string.Equals(t.Name, "Por defecto", StringComparison.OrdinalIgnoreCase))?.Code;
                if (string.IsNullOrWhiteSpace(codigoDefecto))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontró el tipo 'Por defecto'.",
                        Content = string.Empty
                    });
                }

                var tiendasResponse = await _documentService.GetGestionAyudaTiendas(Empresa);
                if (!tiendasResponse.Registered || string.IsNullOrWhiteSpace(tiendasResponse.Content))
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontraron tiendas para completar el documento.",
                        Content = tiendasResponse.Content
                    });
                }

                var tiendas = JsonConvert.DeserializeObject<List<GestionAyudaTiendaDto>>(tiendasResponse.Content) ?? new List<GestionAyudaTiendaDto>();

                var actualizados = CompletarTiendas(request.MGS_CL_GESDETCollection.Select(item => new GestionAyudaDetalleDto
                {
                    DocEntry = docEntry,
                    LineId = item.LineId,
                    U_MGS_CL_TIENDA = item.U_MGS_CL_TIENDA,
                    U_MGS_CL_NOMTIE = item.U_MGS_CL_NOMTIE,
                    U_MGS_CL_CVENTA = item.U_MGS_CL_CVENTA,
                    U_MGS_CL_CRENTA = item.U_MGS_CL_CRENTA,
                    U_MGS_CL_CVAN = item.U_MGS_CL_CVAN,
                    U_MGS_CL_CPERSO = item.U_MGS_CL_CPERSO,
                    U_MGS_CL_CGESTI = item.U_MGS_CL_CGESTI,
                    U_MGS_CL_CSERV = item.U_MGS_CL_CSERV,
                    U_MGS_CL_CCC = item.U_MGS_CL_CCC,
                    U_MGS_CL_CADM = item.U_MGS_CL_CADM
                }).ToList(), tiendas, codigoDefecto);

                var updateRequest = new GestionAyudaUpdateRequest
                {
                    MGS_CL_GESDETCollection = actualizados.Select(item => new GestionAyudaDetalleUpdateDto
                    {
                        LineId = item.LineId,
                        U_MGS_CL_TIENDA = item.U_MGS_CL_TIENDA,
                        U_MGS_CL_NOMTIE = item.U_MGS_CL_NOMTIE,
                        U_MGS_CL_CVENTA = item.U_MGS_CL_CVENTA,
                        U_MGS_CL_CRENTA = item.U_MGS_CL_CRENTA,
                        U_MGS_CL_CVAN = item.U_MGS_CL_CVAN,
                        U_MGS_CL_CPERSO = item.U_MGS_CL_CPERSO,
                        U_MGS_CL_CGESTI = item.U_MGS_CL_CGESTI,
                        U_MGS_CL_CSERV = item.U_MGS_CL_CSERV,
                        U_MGS_CL_CCC = item.U_MGS_CL_CCC,
                        U_MGS_CL_CADM = item.U_MGS_CL_CADM
                    }).ToList()
                };

                var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
                if (!prep.Ok)
                {
                    return BadRequest(prep.Error);
                }

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var requestInformation = new RequestInformation
                {
                    Route = $"MGS_CL_GESCAB({docEntry})",
                    Token = prep.Token,
                    Doc = JsonConvert.SerializeObject(updateRequest, settings)
                };

                var rp = await _documentService.UpdateInfo(requestInformation, "PYP", prep.Cfg);
                if (!rp.Registered)
                {
                    return Ok(new ResponseInformation
                    {
                        Registered = false,
                        Message = $"Error al actualizar detalle: {rp.Message}",
                        Content = rp.Content
                    });
                }

                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar gestión de ayuda para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado al actualizar el documento.",
                    Content = ex.Message
                });
            }
        }

        private static string CalcularPeriodoSiguiente(string ultimoPeriodo)
        {
            if (string.IsNullOrWhiteSpace(ultimoPeriodo))
            {
                return DateTime.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            }

            if (DateTime.TryParseExact(ultimoPeriodo, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var periodoDate))
            {
                var next = periodoDate.AddMonths(1);
                return next.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            }

            if (DateTime.TryParse(ultimoPeriodo, CultureInfo.InvariantCulture, DateTimeStyles.None, out periodoDate))
            {
                var next = periodoDate.AddMonths(1);
                return next.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            }

            return DateTime.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        private static List<GestionAyudaDetalleDto> CompletarTiendas(
            List<GestionAyudaDetalleDto> detalles,
            List<GestionAyudaTiendaDto> tiendas,
            string codigoDefecto)
        {
            var tiendaMap = tiendas.ToDictionary(t => t.PrjCode, t => t.PrjName, StringComparer.OrdinalIgnoreCase);
            var existentes = new Dictionary<string, GestionAyudaDetalleDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in detalles)
            {
                var tiendaCodigo = item.U_MGS_CL_TIENDA ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(tiendaCodigo) && !existentes.ContainsKey(tiendaCodigo))
                {
                    item.U_MGS_CL_NOMTIE = string.IsNullOrWhiteSpace(item.U_MGS_CL_NOMTIE) && tiendaMap.TryGetValue(tiendaCodigo, out var nombre)
                        ? nombre
                        : item.U_MGS_CL_NOMTIE;
                    AplicarDefecto(item, codigoDefecto);
                    existentes[tiendaCodigo] = item;
                }
            }

            foreach (var tienda in tiendas)
            {
                if (existentes.ContainsKey(tienda.PrjCode))
                {
                    continue;
                }

                var nuevo = new GestionAyudaDetalleDto
                {
                    U_MGS_CL_TIENDA = tienda.PrjCode,
                    U_MGS_CL_NOMTIE = tienda.PrjName
                };
                AplicarDefecto(nuevo, codigoDefecto);
                detalles.Add(nuevo);
            }

            return detalles;
        }

        private static void AplicarDefecto(GestionAyudaDetalleDto detalle, string codigoDefecto)
        {
            detalle.U_MGS_CL_CVENTA = NormalizarValor(detalle.U_MGS_CL_CVENTA, codigoDefecto);
            detalle.U_MGS_CL_CRENTA = NormalizarValor(detalle.U_MGS_CL_CRENTA, codigoDefecto);
            detalle.U_MGS_CL_CVAN = NormalizarValor(detalle.U_MGS_CL_CVAN, codigoDefecto);
            detalle.U_MGS_CL_CPERSO = NormalizarValor(detalle.U_MGS_CL_CPERSO, codigoDefecto);
            detalle.U_MGS_CL_CGESTI = NormalizarValor(detalle.U_MGS_CL_CGESTI, codigoDefecto);
            detalle.U_MGS_CL_CSERV = NormalizarValor(detalle.U_MGS_CL_CSERV, codigoDefecto);
            detalle.U_MGS_CL_CCC = NormalizarValor(detalle.U_MGS_CL_CCC, codigoDefecto);
            detalle.U_MGS_CL_CADM = NormalizarValor(detalle.U_MGS_CL_CADM, codigoDefecto);
        }

        private static string NormalizarValor(string valor, string codigoDefecto)
        {
            return string.IsNullOrWhiteSpace(valor) ? codigoDefecto : valor;
        }
    }
}
