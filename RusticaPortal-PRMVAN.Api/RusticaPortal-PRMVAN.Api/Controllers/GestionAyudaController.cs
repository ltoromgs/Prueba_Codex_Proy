using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GestionAyudaController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IEmpresaRuntimeService _empresaRuntime;
        private readonly ILogger<GestionAyudaController> _logger;

        public GestionAyudaController(IDocumentService documentService,
            IEmpresaRuntimeService empresaRuntime,
            ILogger<GestionAyudaController> logger)
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
                _logger.LogError(ex, "Error al obtener tiendas para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
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
                _logger.LogError(ex, "Error al obtener tipos para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("ultimo-periodo")]
        public async Task<ActionResult<ResponseInformation>> GetUltimoPeriodo([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetGestionAyudaUltPeriodo(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener último periodo para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<ResponseInformation>> Buscar([FromQuery] string Empresa, [FromQuery] string periodo, [FromQuery] string? tiendas)
        {
            try
            {
                var validacion = await _documentService.ValidaDatos(Empresa);
                if (!validacion.Registered)
                {
                    _logger.LogWarning("Validación fallida para empresa: {Empresa}", Empresa);
                    return Ok(validacion);
                }

                var rp = await _documentService.GetGestionAyudaBuscar(Empresa, periodo, tiendas);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar gestión de ayuda para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("nuevo-preview")]
        public async Task<ActionResult<ResponseInformation>> GetPreview([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetGestionAyudaPreview(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener previsualización para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpPost("crear")]
        public async Task<ActionResult<ResponseInformation>> Crear([FromQuery] string Empresa, [FromBody] GestionAyudaCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(Empresa))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "Parámetros incompletos para crear la gestión de ayuda.",
                    Content = string.Empty
                });
            }

            if (string.IsNullOrWhiteSpace(request.U_MGS_CL_PERIODO))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "El periodo de destino es obligatorio.",
                    Content = string.Empty
                });
            }

            if (request.MGS_CL_GESDETCollection == null || request.MGS_CL_GESDETCollection.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se enviaron registros de tiendas para crear la gestión de ayuda.",
                    Content = string.Empty
                });
            }

            var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
            if (!prep.Ok) return BadRequest(prep.Error);

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

            var rp = await _documentService.PostInfo(requestInformation, "PYP", prep.Cfg);
            return Ok(rp);
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

            var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
            if (!prep.Ok) return BadRequest(prep.Error);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var requestInformation = new RequestInformation
            {
                Route = $"MGS_CL_GESCAB({docEntry})",
                Token = prep.Token,
                Doc = JsonConvert.SerializeObject(request, settings)
            };

            var rp = await _documentService.UpdateInfo(requestInformation, "PYP", prep.Cfg);
            return Ok(rp);
        }
    }
}
