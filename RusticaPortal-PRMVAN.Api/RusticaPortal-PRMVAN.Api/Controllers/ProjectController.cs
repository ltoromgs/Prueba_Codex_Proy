using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ArellanoCore.Api.Entities.Dto.Information;
using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services.Interfaces;

namespace ArellanoCore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILoginService _loginService;
        private readonly IMapper _mapper;
        public ProjectController(IDocumentService documentService, ILoginService loginService, IMapper mapper)
        {
            _documentService = documentService;
            _loginService = loginService;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseInformation>> Post(ProjectAddDTO project, string Empresa)
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
                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "Projects";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(project);

                return Ok(await _documentService.PostInfo( requestInformation, "PYP", Empresa));

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
        public async Task<ActionResult<ResponseInformation>> Patch( string  code,ProjectUpdateDTO project, string Empresa)
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

                project.Active = project.Active == "N" ? "tNO" : "tYES";

                RequestInformation requestInformation = new RequestInformation();
                requestInformation.Route = "Projects('"+code+"')";
                requestInformation.Token = token;
                requestInformation.Doc = JsonConvert.SerializeObject(project,settings);

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
        public async Task<ActionResult<AditionInformationDTO>> GetByProject(string project, string Empresa)
        {
           
            //AditionalInfomation rp = new AditionalInfomation();

            var rp = await _documentService.GetClienteMoneda(project, Empresa);
            var rs = _mapper.Map<AditionInformationDTO>(rp);
            if (rp.CardCode == null)
            {
                return NotFound();
            }

            if (rs!=null)
                return Ok(rs);

            return BadRequest(rs);

        }
    }
}
