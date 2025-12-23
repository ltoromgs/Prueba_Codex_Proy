using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RusticaPortal_PRMVAN.Web.Models;
using RusticaPortal_PRMVAN.Web.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> Tiendas()
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupovan/tiendas", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> Tipos()
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupovan/tipos", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> GruposMaestro()
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupovan/grupos-maestro", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> ArticulosMaestro(string search)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupovan/articulos-maestro", new Dictionary<string, string?>
            {
                ["empresa"] = emp,
                ["search"] = search
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> GruposPorTienda(string tienda)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(tienda))
                return BadRequest(new { message = "Tienda requerida." });

            var endpoint = QueryHelpers.AddQueryString($"/api/grupovan/tienda/{tienda}/grupos", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> ArticulosPorGrupo(string grupo)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(grupo))
                return BadRequest(new { message = "Grupo requerido." });

            var endpoint = QueryHelpers.AddQueryString($"/api/grupovan/grupo/{grupo}/articulos", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarGruposPorTienda(string prjCode, [FromBody] object payload)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(prjCode))
                return BadRequest(new { message = "Tienda requerida." });

            var endpoint = QueryHelpers.AddQueryString($"/api/grupovan/tienda/{prjCode}/grupos/bulk", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, payload);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarArticulosPorGrupo(string grpCode, [FromBody] object payload)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(grpCode))
                return BadRequest(new { message = "Grupo requerido." });

            var endpoint = QueryHelpers.AddQueryString($"/api/grupovan/grupo/{grpCode}/articulos/bulk", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, payload);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }
    }
}
