

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using System.Collections.Generic;
using System.Linq;
using RusticaPortal_PRMVAN.Api.Entities.Dto;

namespace RusticaPortal_PRMVAN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmpresasController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public ActionResult<ResponseInformation> Get()
        {
            // Leer sección "Empresas" del appsettings.json
            var empresas = _config.GetSection("Empresas").Get<List<EmpresaConfig>>();

            if (empresas == null || empresas.Count == 0)
            {
                return Ok(new ResponseInformation
                {
                    Registered = false,
                    Message = "No hay empresas configuradas",
                    Content = string.Empty
                });
            }

            //// Buscar Id = 1; si no existe, tomar la primera, para que traiga por defecto la empresa 1
            //var def = empresas.FirstOrDefault(e => e.Id == 1) ?? empresas.First();

            //var list = new List<EmpresaModel>
            //    {
            //        new EmpresaModel { Id = def.Id, Nombre = def.Nombre }
            //    };


            // se comenta para que traiga solo id 1
             var list = empresas.Select(e => new EmpresaModel
             {
                 Id = e.Id,
                 Nombre = e.Nombre
             }).ToList();
            

            return Ok(new ResponseInformation
            {
                Registered = true,
                Message = string.Empty,
                Content = JsonConvert.SerializeObject(list)
            });
        }
    }
}
