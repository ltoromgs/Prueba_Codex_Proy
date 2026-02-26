using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Dto.AdministracionGAE;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
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
        public async Task<ActionResult<ResponseInformation>> GetTiendas([FromQuery] string Empresa) => await HandleGet(() => _documentService.GetAdministracionGaeTiendas(Empresa), "tiendas", Empresa);

        [HttpGet("tipos-gae")]
        public async Task<ActionResult<ResponseInformation>> GetTiposGae([FromQuery] string Empresa) => await HandleGet(() => _documentService.GetAdministracionGaeTiposGae(Empresa), "tipos GAE", Empresa);

        [HttpGet("tipos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetTiposGasto([FromQuery] string Empresa) => await HandleGet(() => _documentService.GetAdministracionGaeTiposGasto(Empresa), "tipos de gasto", Empresa);

        [HttpGet("motivos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetMotivosGasto([FromQuery] string Empresa) => await HandleGet(() => _documentService.GetAdministracionGaeMotivosGasto(Empresa), "motivos de gasto", Empresa);

        [HttpGet("buscar")]
        public async Task<ActionResult<ResponseInformation>> Buscar([FromQuery] string Empresa,
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] string tiendas,
            [FromQuery] string filtros,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var rp = await _documentService.GetAdministracionGaeBuscar(Empresa, fechaDesde, fechaHasta, tiendas, filtros, page, pageSize);
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
        public async Task<ActionResult<ResponseInformation>> ActualizarTodo([FromBody] AdministracionGaeUpdateRequest request)
        {
            if (request?.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new ResponseInformation { Registered = false, Message = "No se recibieron datos para actualizar.", Content = string.Empty });
            }

            var results = new List<AdministracionGaeUpdateResult>();
            var grouped = request.Items.GroupBy(item => new { item.BaseDatos, item.ObjectType, item.DocEntry }).ToList();

            foreach (var group in grouped)
            {
                var lines = group.ToList();
                var first = lines.First();

                var prep = await _empresaRuntime.ResolveAndLoginByDatabaseAsync(first.BaseDatos, first.IdEmpresa);
                if (!prep.Ok)
                {
                    var cfgMessage = prep.Error?.Message ?? $"No se tiene acceso a la base de datos {first.BaseDatos} (no está configurada), comunicate con el administrador.";
                    foreach (var line in lines)
                    {
                        results.Add(new AdministracionGaeUpdateResult { BaseDatos = line.BaseDatos, ObjectType = line.ObjectType, DocEntry = line.DocEntry, LineId = line.LineId, Ok = false, Message = cfgMessage });
                    }
                    continue;
                }

                var route = GetServiceLayerRoute(group.Key.ObjectType, group.Key.DocEntry);
                if (string.IsNullOrWhiteSpace(route))
                {
                    foreach (var line in lines)
                    {
                        results.Add(new AdministracionGaeUpdateResult { BaseDatos = line.BaseDatos, ObjectType = line.ObjectType, DocEntry = line.DocEntry, LineId = line.LineId, Ok = false, Message = "ObjectType no soportado para actualización por Service Layer." });
                    }
                    continue;
                }

                var payload = new
                {
                    DocumentLines = lines.Select(line => new
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
                    Doc = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                };

                var response = await _documentService.UpdateInfo(requestInformation, "PYP", prep.Cfg);
                var slMessage = response?.Content;
                if (string.IsNullOrWhiteSpace(slMessage))
                {
                    slMessage = response?.Message;
                }

                if (response == null || !response.Registered)
                {
                    foreach (var line in lines)
                    {
                        results.Add(new AdministracionGaeUpdateResult { BaseDatos = line.BaseDatos, ObjectType = line.ObjectType, DocEntry = line.DocEntry, LineId = line.LineId, Ok = false, Message = string.IsNullOrWhiteSpace(slMessage) ? "Error al actualizar por Service Layer." : slMessage });
                    }
                    continue;
                }

                foreach (var line in lines)
                {
                    results.Add(new AdministracionGaeUpdateResult { BaseDatos = line.BaseDatos, ObjectType = line.ObjectType, DocEntry = line.DocEntry, LineId = line.LineId, Ok = true, Message = string.Empty });
                }
            }

            return Ok(new ResponseInformation
            {
                Registered = true,
                Message = string.Empty,
                Content = JsonConvert.SerializeObject(results)
            });
        }

        private async Task<ActionResult<ResponseInformation>> HandleGet(Func<Task<ResponseInformation>> action, string desc, string empresa)
        {
            try
            {
                return Ok(await action());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener {Desc} de administración GAE para la empresa: {Empresa}", desc, empresa);
                return StatusCode(500, new ResponseInformation { Registered = false, Message = "Error inesperado en el servidor", Content = ex.Message });
            }
        }

        private static string GetServiceLayerRoute(string objectType, string docEntry)
        {
            if (string.IsNullOrWhiteSpace(objectType) || string.IsNullOrWhiteSpace(docEntry)) return string.Empty;
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
