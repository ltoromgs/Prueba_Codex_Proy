using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessPartnersController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public BusinessPartnersController(IDocumentService documentService, ILoginService loginService)
        {
            _documentService = documentService;
            _loginService = loginService;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseInformation>> Post(BusinessPartnersAddDTO BusinessPartners, string Empresa)
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
                BusinessPartners.CardCode = "P000" + BusinessPartners.LicTradNum;
                BusinessPartners.GroupCode = 106;
                BusinessPartners.U_MGS_LC_TIPDOC = "1";
                BusinessPartners.U_MGS_LC_TIPPER = "TPN";
                BusinessPartners.Currency = "SOL";
                BusinessPartners.CardType = "cSupplier";

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "BusinessPartners";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(BusinessPartners);

                return Ok(await _documentService.PostInfo(requestInformation, "PLANILLA", Empresa));

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
