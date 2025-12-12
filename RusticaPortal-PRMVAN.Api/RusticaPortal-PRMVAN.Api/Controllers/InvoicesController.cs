using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public InvoicesController(IDocumentService documentService, ILoginService loginService)
        {
            _documentService = documentService;
            _loginService = loginService;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseInformation>> Post(DocumentAddDTO invoces, string Empresa)
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
                //string pro = invoces.DocumentLines[0].Code;
                //AditionalInfomation ai = await _documentService.GetClienteMoneda(pro);
                //invoces.DocCurrency = ai.CurCode;


                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "Invoices";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(invoces);

                return Ok(await _documentService.PostInfo(requestInformation, "MARKADAT", Empresa));

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

