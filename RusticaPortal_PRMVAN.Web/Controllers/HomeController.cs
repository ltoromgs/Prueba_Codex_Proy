using RusticaPortal_PRMVAN.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public HomeController(ApiService apiService)
            : base(apiService)
        {
        }

        public IActionResult Index()
        {

        [HttpGet]
        public IActionResult PingSession()
        {
            return Ok();
        }
            // Sólo cargo el popup desde sesión
            var popup = HttpContext.Session.GetString("PopupImage");
            ViewBag.PopupImage = popup;

            // Retorno la vista (el layout leerá ViewBag.MenuModel)
            return View();
        }
    }
}
