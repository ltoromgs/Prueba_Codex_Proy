using Microsoft.AspNetCore.Mvc;
using RusticaPortal_PRMVAN.Web.Services;
using RusticaPortal_PRMVAN.Web.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
  

    public class RecepcionController : BaseController
    {
        public RecepcionController(ApiService api) : base(api) { }
        public async Task<IActionResult> Index()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;

            if (string.IsNullOrEmpty(empresa))
            {
                ViewBag.Error = "Empresa no encontrada en sesión.";
                return View(new List<RecepcionModel>());
            }
            var endpoint = $"/api/recepcion?empresa={empresa}";
            var response = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (!response.Registered || string.IsNullOrEmpty(response.Content))
            {
                ViewBag.Mensaje = response.Message ?? "No se encontraron datos de cierre.";
                return View(new List<RecepcionModel>());
            }

            var lista = JsonConvert.DeserializeObject<List<RecepcionModel>>(response.Content);
            return View(lista);
        }
    }
}
