using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RusticaPortal_PRMVAN.Api.Entities.Dto.GrupoPrm;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoPrmController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<GrupoPrmController> _logger;

        public GrupoPrmController(IDocumentService documentService, ILogger<GrupoPrmController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        [HttpGet("tiendas")]
        public async Task<ActionResult<ResponseInformation>> GetTiendas([FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmTiendas(Empresa);
            return Ok(rp);
        }

        [HttpGet("tipos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetTiposGasto([FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmTiposGasto(Empresa);
            return Ok(rp);
        }

        [HttpGet("motivos-gasto")]
        public async Task<ActionResult<ResponseInformation>> GetMotivosGasto([FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmMotivosGasto(Empresa);
            return Ok(rp);
        }

        [HttpGet("grupos-maestro")]
        public async Task<ActionResult<ResponseInformation>> GetGruposMaestro([FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmMaestro(Empresa);
            return Ok(rp);
        }

        [HttpGet("articulos-maestro")]
        public async Task<ActionResult<ResponseInformation>> GetArticulosMaestro([FromQuery] string Empresa, [FromQuery] string search = "")
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmItemsMaestro(Empresa, search);
            return Ok(rp);
        }

        [HttpGet("tienda/{prjCode}/grupos")]
        public async Task<ActionResult<ResponseInformation>> GetGruposPorTienda(string prjCode, [FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmPorTienda(Empresa, prjCode);
            return Ok(rp);
        }

        [HttpGet("tienda/{prjCode}/grupo/{grpCode}/articulos")]
        public async Task<ActionResult<ResponseInformation>> GetArticulosPorGrupo(string prjCode, string grpCode, [FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoPrmArticulos(Empresa, prjCode, grpCode);
            return Ok(rp);
        }

        [HttpPost("tienda/{prjCode}/grupos/bulk")]
        public async Task<ActionResult<ResponseInformation>> GuardarGrupos(string prjCode, [FromQuery] string Empresa, [FromBody] GrupoPrmBulkRequest payload)
        {
            var error = ValidarGrupos(payload?.Items);
            if (error != null)
            {
                return BadRequest(error);
            }

            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.SetGrupoPrmPorTiendaBulk(Empresa, prjCode, payload.Items);
            return Ok(rp);
        }

        [HttpPost("tienda/{prjCode}/grupo/{grpCode}/articulos/bulk")]
        public async Task<ActionResult<ResponseInformation>> GuardarArticulos(string prjCode, string grpCode, [FromQuery] string Empresa, [FromBody] ArticuloPrmBulkRequest payload)
        {
            var error = ValidarArticulos(payload?.Items);
            if (error != null)
            {
                return BadRequest(error);
            }

            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.SetGrupoPrmArticulosBulk(Empresa, prjCode, grpCode, payload.Items);
            return Ok(rp);
        }

        [HttpPost("copiar")]
        public async Task<ActionResult<ResponseInformation>> CopiarTienda([FromQuery] string Empresa, [FromBody] CopiarPrmRequest payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.TiendaOrigen) || string.IsNullOrWhiteSpace(payload.TiendaDestino))
            {
                return BadRequest(new ResponseInformation { Registered = false, Message = "Debe indicar tienda origen y destino." });
            }

            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.CopiarGrupoPrmTienda(Empresa, payload.TiendaOrigen, payload.TiendaDestino);
            return Ok(rp);
        }

        private ResponseInformation ValidarGrupos(IEnumerable<PrmGrupoDetalleDto> items)
        {
            if (items == null || !items.Any())
            {
                return new ResponseInformation { Registered = false, Message = "No se recibieron registros para actualizar" };
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_ACTIVO))
                {
                    item.U_MGS_CL_ACTIVO = "SI";
                }
                if (string.IsNullOrWhiteSpace(item.MGS_CL_TIPGAS))
                {
                    return new ResponseInformation { Registered = false, Message = "El tipo de gasto es obligatorio para cada grupo PRM" };
                }
            }

            var duplicados = items
                .Where(x => !string.IsNullOrWhiteSpace(x.U_MGS_CL_GRPCOD))
                .GroupBy(x => x.U_MGS_CL_GRPCOD.ToUpper())
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicados != null)
            {
                return new ResponseInformation { Registered = false, Message = $"El grupo {duplicados.Key} está duplicado" };
            }

            return null;
        }

        private ResponseInformation ValidarArticulos(IEnumerable<PrmArticuloDetalleDto> items)
        {
            if (items == null || !items.Any())
            {
                return new ResponseInformation { Registered = false, Message = "No se recibieron registros para actualizar" };
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_ACTIVO))
                {
                    item.U_MGS_CL_ACTIVO = "SI";
                }
                if (string.IsNullOrWhiteSpace(item.MGS_CL_TIPGAS) || string.IsNullOrWhiteSpace(item.MGS_CL_TIPMOP))
                {
                    return new ResponseInformation { Registered = false, Message = "El tipo y motivo de gasto son obligatorios para cada artículo PRM" };
                }
            }

            var duplicados = items
                .Where(x => !string.IsNullOrWhiteSpace(x.U_MGS_CL_ITEMCOD))
                .GroupBy(x => x.U_MGS_CL_ITEMCOD.ToUpper())
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicados != null)
            {
                return new ResponseInformation { Registered = false, Message = $"El artículo {duplicados.Key} está duplicado" };
            }

            return null;
        }
    }
}
