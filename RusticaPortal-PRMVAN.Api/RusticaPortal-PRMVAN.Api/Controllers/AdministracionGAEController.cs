using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RusticaPortal_PRMVAN.Api.Entities.Dto.AdministracionGAE;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministracionGAEController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IEmpresaRuntimeService _empresaRuntime;
        private readonly ILogger<AdministracionGAEController> _logger;

        public AdministracionGAEController(
            IDocumentService documentService,
            IEmpresaRuntimeService empresaRuntime,
            ILogger<AdministracionGAEController> logger)
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
            if (request == null || request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "No se recibieron datos para actualizar.",
                    Content = string.Empty
                });
            }

            if (request.Items.Any(item => string.IsNullOrWhiteSpace(item.BaseDatos)
                || string.IsNullOrWhiteSpace(item.ObjectType)
                || string.IsNullOrWhiteSpace(item.DocEntry)
                || string.IsNullOrWhiteSpace(item.IdEmpresa)))
            {
                return BadRequest(new ResponseInformation
                {
                    Registered = false,
                    Message = "Existen filas sin datos de base, ObjectType o DocEntry.",
                    Content = string.Empty
                });
            }

            var prep = await _empresaRuntime.ResolveAndLoginAsync(Empresa);
            if (!prep.Ok)
            {
                return BadRequest(prep.Error);
            }

            var grouped = request.Items
                .GroupBy(item => new { item.BaseDatos, item.ObjectType, item.DocEntry })
                .ToList();

            var results = new System.Collections.Generic.List<AdministracionGaeUpdateResult>();

            foreach (var group in grouped)
            {
                var updates = group.ToList();
                var route = GetServiceLayerRoute(group.Key.ObjectType, group.Key.DocEntry);

                if (string.IsNullOrWhiteSpace(route))
                {
                    foreach (var line in updates)
                    {
                        results.Add(new AdministracionGaeUpdateResult
                        {
                            BaseDatos = line.BaseDatos,
                            ObjectType = line.ObjectType,
                            DocEntry = line.DocEntry,
                            LineId = line.LineId,
                            Ok = false,
                            Message = "ObjectType no soportado."
                        });
                    }

                    continue;
                }

                var payload = new
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
                    })
                };

                var requestInformation = new RequestInformation
                {
                    Route = route,
                    Token = prep.Token,
                    Doc = Newtonsoft.Json.JsonConvert.SerializeObject(payload, new Newtonsoft.Json.JsonSerializerSettings
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                    })
                };

                var response = await _documentService.UpdateInfo(requestInformation, "PYP", prep.Cfg);

                if (response == null || !response.Registered)
                {
                    var message = response?.Message ?? "Error al actualizar por Service Layer.";
                    foreach (var line in updates)
                    {
                        results.Add(new AdministracionGaeUpdateResult
                        {
                            BaseDatos = line.BaseDatos,
                            ObjectType = line.ObjectType,
                            DocEntry = line.DocEntry,
                            LineId = line.LineId,
                            Ok = false,
                            Message = message
                        });
                    }

                    continue;
                }

                foreach (var line in updates)
                {
                    results.Add(new AdministracionGaeUpdateResult
                    {
                        BaseDatos = line.BaseDatos,
                        ObjectType = line.ObjectType,
                        DocEntry = line.DocEntry,
                        LineId = line.LineId,
                        Ok = true,
                        Message = string.Empty
                    });
                }
            }

            return Ok(new ResponseInformation
            {
                Registered = true,
                Message = string.Empty,
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(results)
            });
        }

        private static string GetServiceLayerRoute(string objectType, string docEntry)
        {
            if (string.IsNullOrWhiteSpace(objectType) || string.IsNullOrWhiteSpace(docEntry))
            {
                return string.Empty;
            }

            return objectType switch
            {
                "17" => $"Orders({docEntry})",
                "22" => $"PurchaseOrders({docEntry})",
                "20" => $"PurchaseDeliveryNotes({docEntry})",
                "18" => $"PurchaseInvoices({docEntry})",
                "19" => $"PurchaseCreditNotes({docEntry})",
                _ => string.Empty
            };
        }
    }
}
