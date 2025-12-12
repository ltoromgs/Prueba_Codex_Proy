using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services;
using ArellanoCore.Api.Services.Interfaces;
using SAPbobsCOM;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseAmountAvailableController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public PurchaseAmountAvailableController(IDocumentService documentService, ILoginService loginService)
        {
            _documentService = documentService;
            _loginService = loginService;

        }
        [HttpGet]
        public async Task<ActionResult<AmountAvailableGetDTO>> Get(string project, string Empresa)
        {
            ResponseInformation rp = new ResponseInformation();
            rp = await _documentService.ValidaDatos(Empresa);
            if (!rp.Registered)
            {
                return BadRequest(rp);
            }

            AmountAvailableGetDTO result = new AmountAvailableGetDTO();
            string token = await _loginService.Login(Empresa);
            if (token != "")
            {
                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "PurchaseAmountAvailable";
                requestInformation.Token = token;
                requestInformation.Doc = "";
                result = await _documentService.GetAmountAvailable(requestInformation, "PYP", Empresa, project);
                // result = await _documentService.GetInfo(requestInformation,  Empresa);
            }

            return Ok(result);


        }
    }
}
