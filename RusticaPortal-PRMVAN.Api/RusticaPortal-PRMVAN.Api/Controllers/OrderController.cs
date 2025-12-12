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
    public class OrderController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        public OrderController(IDocumentService documentService, ILoginService loginService)
        {
            _documentService = documentService;
            _loginService = loginService;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseInformation>> Post(OrderAddDTO order, string Empresa)
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
                string pro = order.DocumentLines[0].Code;
                order.Project = pro;
                AditionalInfomation ai = await _documentService.GetClienteMoneda(pro, Empresa);

                order.Cliente = ai.CardCode;
                order.DocCurrency = ai.CurCode;



                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "Orders";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(order);

                return Ok(await _documentService.PostInfo(requestInformation, "PYP", Empresa  ));

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
        public async Task<ActionResult<ResponseInformation>> Patch(string project, OrderUpdateDTO order , string Empresa)
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

                string docEntry = await _documentService.GetOVByProject(project, true, Empresa);
                if (docEntry == "")
                    return NotFound();

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "Orders(" + docEntry + ")";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(order, settings);

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




        [HttpPatch("{contrato}/files")]
        public async Task<ActionResult<ResponseInformation>> Patch1(string contrato, OrderUpdateFilesDTO order, string Empresa)
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
                

                List<string> docEntryList = await _documentService.GetOVByContract(contrato, Empresa);
                if(docEntryList == null)
                    return NotFound();
                bool estafoFinal = true;
                foreach (string docEntry in docEntryList)
                {
                    RequestInformation requestInformation = new RequestInformation();
                    requestInformation.Route = "Orders(" + docEntry + ")";
                    requestInformation.Token = token;
                    requestInformation.Doc = JsonConvert.SerializeObject(order, settings);
                    ResponseInformation rif =  await _documentService.UpdateInfo(requestInformation, "PYP", Empresa);
                    estafoFinal = rif.Registered;
                }

                if(estafoFinal)
                {
                    rp.Content = "";
                    rp.Message = "Se actualizó con éxito";
                    rp.Registered = true;
                }
                else
                {
                    rp.Content = "";
                    rp.Message = "No fue posible actualizar los registros";
                    rp.Registered = false;
                }

                return Ok(rp);

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
