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
    public class AccountController : ControllerBase
        {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IDocumentService documentService,
            ILoginService loginService,
            ILogger<AccountController> logger)
        {
            _documentService = documentService;
            _loginService = loginService;
            _logger = logger;
        }

        [HttpPost]
            public async Task<ActionResult<ResponseInformation>> Post([FromBody] AccountGetDTO AccountGetDTO, [FromQuery] string Empresa)
            {
                try
                {
                    ResponseInformation rp = await _documentService.ValidaDatos(Empresa);
                    if (!rp.Registered)
                    {
                        _logger.LogWarning("Validación fallida para empresa: {Empresa} mensaje: {Message}", Empresa, rp.Message);
                        return Ok(rp); // devuelves 200 con Registered = false
                    }

                    RequestInformation requestInformation = new RequestInformation
                    {
                        Route = "Get_account",
                        Token = null,
                        Doc = JsonConvert.SerializeObject(AccountGetDTO)
                    };

                    rp = await _documentService.GetInfoDB(requestInformation, "WEBPORTAL", Empresa);

                    if (!rp.Registered)
                    {
                        _logger.LogError("Usuario: {Username} en empresa: {Empresa} mensaje: {Message} contenido:  {Content} ", AccountGetDTO.Username, Empresa, rp.Message, rp.Content);
                        return Ok(rp);
                    }

                   
                    return Ok(rp);
                }
                catch (Exception ex)
                {
                _logger.LogError("Usuario: {Username} en empresa: {Empresa} mensaje: {Message} contenido:  {Content} ", AccountGetDTO.Username, Empresa, "Error", ex.Message);

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
