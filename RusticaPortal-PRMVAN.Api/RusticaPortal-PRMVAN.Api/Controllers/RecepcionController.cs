using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;
using SAPbobsCOM;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecepcionController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        private readonly ILogger<AccountController> _logger;

        public RecepcionController(
            IDocumentService documentService,
            ILoginService loginService,
            ILogger<AccountController> logger)
        {
            _documentService = documentService;
            _loginService = loginService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseInformation>> GetRecepcion([FromQuery] string Empresa)
        {
            try
            {
                // Validación de empresa
                ResponseInformation rp = await _documentService.ValidaDatos(Empresa);
                if (!rp.Registered)
                {
                    _logger.LogWarning("Validación fallida para empresa: {Empresa}", Empresa);
                    return Ok(rp);
                }

                // Obtener menú desde SAP HANA
                rp = await _documentService.GetRecepcionDB(Empresa);

                if (!rp.Registered)
                {
                    _logger.LogWarning("Fecha de recepción no disponible para la empresa: {Empresa}", Empresa);
                    return Ok(rp);
                }

                // Éxito
                return Ok(rp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fecha de recepción para la empresa: {Empresa}", Empresa);
                return StatusCode(500, new ResponseInformation
                {
                    Registered = false,
                    Message = "Error inesperado en el servidor",
                    Content = ex.Message
                });
            }
        }
    }
}
