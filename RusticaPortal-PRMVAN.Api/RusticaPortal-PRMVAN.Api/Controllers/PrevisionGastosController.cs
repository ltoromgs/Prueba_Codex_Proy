using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Dto.PrevisionGastos;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrevisionGastosController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IEmpresaRuntimeService _empresaRuntime;
        private readonly ILogger<PrevisionGastosController> _logger;

        public PrevisionGastosController(IDocumentService documentService,
            IEmpresaRuntimeService empresaRuntime,
            ILogger<PrevisionGastosController> logger)
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
                var rp = await _documentService.GetPrevisionGastosTiendas(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tiendas de previsión de gastos para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("conceptos-prm")]
        public async Task<ActionResult<ResponseInformation>> GetConceptos([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetPrevisionGastosConceptosPrm(Empresa);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conceptos PRM de previsión de gastos para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("motivos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetMotivos([FromQuery] string Empresa)
        {
            try
            {
                var rp = await _documentService.GetPrevisionGastosMotivosGasto(Empresa);
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

        [HttpGet("items")]
        public async Task<ActionResult<ResponseInformation>> GetItems([FromQuery] string Empresa, [FromQuery] string search)
        {
            try
            {
                var rp = await _documentService.GetPrevisionGastosItems(Empresa, search);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener artículos para la empresa: {Empresa}", Empresa);
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
            [FromQuery] string periodo,
            [FromQuery] string tiendas,
            [FromQuery] string motivo)
        {
            try
            {
                var rp = await _documentService.GetPrevisionGastosBuscar(Empresa, periodo, tiendas, motivo);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar previsión de gastos para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpGet("docentry")]
        public async Task<ActionResult<ResponseInformation>> GetDocEntryPeriodo([FromQuery] string Empresa, [FromQuery] string periodo)
        {
            try
            {
                var rp = await _documentService.GetPrevisionGastosDocEntryPeriodo(Empresa, periodo);
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener DocEntry del periodo para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }

        [HttpPost("actualizar-todo")]
        public async Task<ActionResult<ResponseInformation>> ActualizarTodo([FromQuery] string Empresa, [FromBody] PrevisionGastoSaveRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se recibieron datos para guardar.",
                    Content = string.Empty
                });
            }

            if (string.IsNullOrWhiteSpace(request.U_MGS_CL_PERIODO))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "El periodo es obligatorio.",
                    Content = string.Empty
                });
            }

            if (request.MGS_CL_GASDETCollection == null || request.MGS_CL_GASDETCollection.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se enviaron líneas para guardar.",
                    Content = string.Empty
                });
            }

            if (!TryParsePeriodo(request.U_MGS_CL_PERIODO, out var inicio, out var fin))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "Periodo inválido.",
                    Content = string.Empty
                });
            }

            foreach (var item in request.MGS_CL_GASDETCollection)
            {
                if (!DateTime.TryParseExact(item.U_MGS_CL_FECHA, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                {
                    return BadRequest(new ResponseInformation
                    {
                        Registered = false,
                        Message = "La fecha ingresada no es válida.",
                        Content = string.Empty
                    });
                }

                if (fecha < inicio || fecha > fin)
                {
                    return BadRequest(new ResponseInformation
                    {
                        Registered = false,
                        Message = "La fecha ingresada debe estar dentro del periodo seleccionado.",
                        Content = string.Empty
                    });
                }
            }

            var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
            if (!prep.Ok)
            {
                return BadRequest(prep.Error);
            }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            RequestInformation requestInformation;
            if (string.IsNullOrWhiteSpace(request.DocEntry))
            {
                requestInformation = new RequestInformation
                {
                    Route = "MGS_CL_GASCAB",
                    Token = prep.Token,
                    Doc = JsonConvert.SerializeObject(new
                    {
                        request.U_MGS_CL_PERIODO,
                        request.MGS_CL_GASDETCollection
                    }, settings)
                };

                var rp = await _documentService.PostInfo(requestInformation, "PYP", prep.Cfg);
                return Ok(rp);
            }

            requestInformation = new RequestInformation
            {
                Route = $"MGS_CL_GASCAB({request.DocEntry})",
                Token = prep.Token,
                Doc = JsonConvert.SerializeObject(new
                {
                    request.MGS_CL_GASDETCollection
                }, settings)
            };

            var response = await _documentService.UpdateInfo(requestInformation, "PYP", prep.Cfg);
            return Ok(response);
        }

        private static bool TryParsePeriodo(string periodo, out DateTime inicio, out DateTime fin)
        {
            inicio = default;
            fin = default;

            if (!DateTime.TryParseExact(periodo + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var inicioPeriodo))
            {
                return false;
            }

            inicio = inicioPeriodo.Date;
            fin = inicioPeriodo.AddMonths(1).AddDays(-1).Date;
            return true;
        }
    }
}
