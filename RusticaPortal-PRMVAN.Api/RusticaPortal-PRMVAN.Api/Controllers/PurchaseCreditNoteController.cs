using Microsoft.AspNetCore.Mvc;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseCreditNoteController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public PurchaseCreditNoteController(IDocumentService documentService, ILoginService loginService)
        {
            _documentService = documentService;
            _loginService = loginService;

        }
        [HttpGet]
        public async Task<ActionResult<DocumentGetDTO>> Get(string project, string Empresa)
        {
            ResponseInformation rp = new ResponseInformation();
            rp = await _documentService.ValidaDatos(Empresa);
            if (!rp.Registered)
            {
                return BadRequest(rp);
            }

            DocumentGetDTO result = new DocumentGetDTO();
            string token = await _loginService.Login(Empresa);
            if (token != "")
            {
                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "PurchaseCreditNote";
                requestInformation.Token = token;
                requestInformation.Doc = "";
                result = await _documentService.GetDoc(requestInformation, "PYP", Empresa, project);
                // result = await _documentService.GetInfo(requestInformation,  Empresa);
            }

            return Ok(result);


        }
    }
}
