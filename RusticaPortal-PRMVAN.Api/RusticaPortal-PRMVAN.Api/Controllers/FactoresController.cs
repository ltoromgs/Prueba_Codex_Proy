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
    public class FactoresController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        private readonly ILogger<AccountController> _logger;

        public FactoresController(
            IDocumentService documentService,
            ILoginService loginService,
            ILogger<AccountController> logger)
        {
            _documentService = documentService;
            _loginService = loginService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseInformation>> GetFactores([FromQuery] string Empresa,
                                                   [FromQuery] string periodo,
                                                   [FromQuery] string tiendas)
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
                rp = await _documentService.GetFactoresDB(Empresa, periodo, tiendas);

                if (!rp.Registered)
                {
                    _logger.LogWarning("Datos de factores no disponible para la empresa: {Empresa}", Empresa);
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

        [HttpPatch]
        public async Task<ActionResult<ResponseInformation>> Patch(string docEntry, string Empresa, MatrizFactorDTO MatrizFactor)
        {
            ResponseInformation rp = new ResponseInformation();

            rp = await _documentService.ValidaDatos(Empresa);
            if (!rp.Registered)
            {
                return BadRequest(rp);
            }

            string token = await _loginService.Login(Empresa);

            if (token != "")
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                //string docEntry = await _documentService.GetOVByProject(docEntry, true, Empresa);
                //if (docEntry == "")
                //    return NotFound();

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "MGS_CL_FACCAB(" + docEntry + ")";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(MatrizFactor, settings);

                return Ok(await _documentService.UpdateInfo(requestInformation, "PYP", Empresa));

            }
            else
            {
                rp.Content = "";
                rp.Message = "No fue posible conectarse al SL";
                rp.Registered = false;
            }

            return BadRequest(rp);

        }
    }
}
