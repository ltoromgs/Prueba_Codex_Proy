using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;
using SAPbouiCOM;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseRequestController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public PurchaseRequestController(IDocumentService documentService, ILoginService loginService)
        {
            _documentService = documentService;
            _loginService = loginService;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseInformation>> Post(PurchaseRequestAddDTO purchaseRequest, string Empresa)
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
                string pro = purchaseRequest.DocumentLines[0].Code;
               // AditionalInfomation ai = await _documentService.GetClienteMoneda(pro, Empresa);
                // purchaseRequest.DocCurrency = ai.CurCode;


                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "PurchaseRequests";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(purchaseRequest);

                return Ok(await _documentService.PostInfo(requestInformation, "PYP", Empresa));

            }
            else
            {
                rp.Content = "";
                rp.Message = "No fue posible conectarse al SL";
                rp.Registered = false;
            }

            return BadRequest(rp);

        }

        [HttpPatch]
        public async Task<ActionResult<ResponseInformation>> Patch(string project, PurchaseRequestUpdateDTO purchaseRequest, string Empresa)
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

                string docEntry = await _documentService.GetOVByProject(project, false, Empresa  );
                if (docEntry == "")
                    return NotFound();

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "PurchaseRequests(" + docEntry + ")";
                requestInformation.Token = token;

                // Convertir a JSON
                string json = JsonConvert.SerializeObject(purchaseRequest, settings);
                var jsonObject = JObject.Parse(json);

                // Iterar sobre cada línea y eliminar la propiedad "Cantidad_Inicial" si su valor es 0
                foreach (var item in jsonObject["DocumentLines"])
                {
                    if (item["U_MGS_CL_CANINI"].Value<int>() == 0)
                    {
                        item["U_MGS_CL_CANINI"].Parent.Remove();
                        item["U_MGS_CL_PREINI"].Parent.Remove();
                    }
                }   
                
                requestInformation.Doc = jsonObject.ToString();

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
                requestInformation.Route = "PurchaseRequests";
                requestInformation.Token = token;
                requestInformation.Doc = "";
                result = await _documentService.GetDoc(requestInformation, "PYP", Empresa, project);
                // result = await _documentService.GetInfo(requestInformation,  Empresa);
            }

            return Ok(result);


        }

        [HttpPatch("{project}/delete")]
        public async Task<ActionResult<ResponseInformation>> Patch2(string project, PurchaseRequestUpdateDTO purchaseRequest, string Empresa)
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

                string docEntry = await _documentService.GetOVByProject(project, false, Empresa);
                if (docEntry == "")
                    return NotFound();

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "PurchaseRequests";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(purchaseRequest, settings);

                return Ok(await _documentService.DeleteInfo(requestInformation, "PYP", Empresa, docEntry));

            }
            else
            {
                rp.Content = "";
                rp.Message = "No fue posible conectarse al SL";
                rp.Registered = false;
            }

            return BadRequest(rp);

        }

        /*
        [HttpPatch("{project}/delete")]
        public async Task<ActionResult<ResponseInformation>> Patch2(string project, PurchaseRequestUpdateDTO purchaseRequest, string Empresa)
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

                string docEntry = await _documentService.GetOVByProject(project, false, Empresa);
                if (docEntry == "")
                    return NotFound();

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "PurchaseRequests";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(purchaseRequest, settings);

                return Ok(await _documentService.DeleteInfo(requestInformation, "PYP", Empresa, docEntry));

            }
            else
            {
                rp.Content = "";
                rp.Message = "No fue posible conectarse al SL";
                rp.Registered = false;
            }

            return BadRequest(rp);

        }*/
    }
}
