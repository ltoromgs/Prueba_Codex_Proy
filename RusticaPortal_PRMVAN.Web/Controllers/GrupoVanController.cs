using Microsoft.AspNetCore.Mvc;
using RusticaPortal_PRMVAN.Web.Services;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
    public class GrupoVanController : BaseController
    {
        public GrupoVanController(ApiService api) : base(api) { }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
