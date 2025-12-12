using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;
using SAPbobsCOM;


namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessPartnersCustomerController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public BusinessPartnersCustomerController(IDocumentService documentService, ILoginService loginService)
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
                //string pro = purchaseRequest.DocumentLines[0].Code;
                // AditionalInfomation ai = await _documentService.GetClienteMoneda(pro);
                BusinessPartners.CardCode = "C" + BusinessPartners.LicTradNum;
                BusinessPartners.GroupCode = 100;
                BusinessPartners.U_MGS_LC_TIPDOC = "6";
                BusinessPartners.U_MGS_LC_TIPPER = "TPJ";        
                BusinessPartners.Currency = "##"; 
                BusinessPartners.EmailAddress = "esantos@arellano.pe"; 
                BusinessPartners.CardType = "cCustomer";

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "BusinessPartners";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(BusinessPartners);

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
