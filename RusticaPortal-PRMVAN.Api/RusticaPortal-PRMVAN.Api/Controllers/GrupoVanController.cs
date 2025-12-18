using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RusticaPortal_PRMVAN.Api.Entities.Dto.GrupoVan;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoVanController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<GrupoVanController> _logger;

        public GrupoVanController(IDocumentService documentService, ILogger<GrupoVanController> logger)
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

            var rp = await _documentService.GetGrupoVanTiendas(Empresa);
            return Ok(rp);
        }

        [HttpGet("tipos")]
        public async Task<ActionResult<ResponseInformation>> GetTipos([FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoVanTipos(Empresa);
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

            var rp = await _documentService.GetGrupoVanMaestro(Empresa);
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

            var rp = await _documentService.GetGrupoVanItemsMaestro(Empresa, search);
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

            var rp = await _documentService.GetGrupoVanPorTienda(Empresa, prjCode);
            return Ok(rp);
        }

        [HttpGet("grupo/{grpCode}/articulos")]
        public async Task<ActionResult<ResponseInformation>> GetArticulosPorGrupo(string grpCode, [FromQuery] string Empresa)
        {
            var validacion = await _documentService.ValidaDatos(Empresa);
            if (!validacion.Registered)
            {
                _logger.LogWarning("Validación fallida para empresa {Empresa}", Empresa);
                return Ok(validacion);
            }

            var rp = await _documentService.GetGrupoVanArticulos(Empresa, grpCode);
            return Ok(rp);
        }

        [HttpPost("tienda/{prjCode}/grupos/bulk")]
        public async Task<ActionResult<ResponseInformation>> GuardarGrupos(string prjCode, [FromQuery] string Empresa, [FromBody] GrupoVanBulkRequest payload)
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

            var rp = await _documentService.SetGrupoVanPorTiendaBulk(Empresa, prjCode, payload.Items);
            return Ok(rp);
        }

        [HttpPost("grupo/{grpCode}/articulos/bulk")]
        public async Task<ActionResult<ResponseInformation>> GuardarArticulos(string grpCode, [FromQuery] string Empresa, [FromBody] ArticuloVanBulkRequest payload)
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

            var rp = await _documentService.SetGrupoVanArticulosBulk(Empresa, grpCode, payload.Items);
            return Ok(rp);
        }

        private ResponseInformation ValidarGrupos(IEnumerable<VanGrupoDetalleDto> items)
        {
            if (items == null || !items.Any())
            {
                return new ResponseInformation { Registered = false, Message = "No se recibieron registros para actualizar" };
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_TIPO))
                {
                    return new ResponseInformation { Registered = false, Message = "El tipo es obligatorio para cada grupo VAN" };
                }

                if (!item.U_MGS_CL_PORC.HasValue || item.U_MGS_CL_PORC.Value <= 0)
                {
                    item.U_MGS_CL_PORC = 100;
                }

                if (item.U_MGS_CL_PORC < 0 || item.U_MGS_CL_PORC > 100)
                {
                    return new ResponseInformation { Registered = false, Message = "El porcentaje de grupo debe estar entre 0 y 100" };
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

        private ResponseInformation ValidarArticulos(IEnumerable<VanArticuloDetalleDto> items)
        {
            if (items == null || !items.Any())
            {
                return new ResponseInformation { Registered = false, Message = "No se recibieron registros para actualizar" };
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_TIPO))
                {
                    return new ResponseInformation { Registered = false, Message = "El tipo es obligatorio para cada artículo VAN" };
                }

                if (!item.U_MGS_CL_PORC.HasValue || item.U_MGS_CL_PORC.Value <= 0)
                {
                    item.U_MGS_CL_PORC = 100;
                }

                if (item.U_MGS_CL_PORC < 0 || item.U_MGS_CL_PORC > 100)
                {
                    return new ResponseInformation { Registered = false, Message = "El porcentaje del artículo debe estar entre 0 y 100" };
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
