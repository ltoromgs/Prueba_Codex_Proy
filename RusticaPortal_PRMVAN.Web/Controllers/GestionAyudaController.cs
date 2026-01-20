using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RusticaPortal_PRMVAN.Web.Models;
using RusticaPortal_PRMVAN.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
    public class GestionAyudaController : BaseController
    {
        public GestionAyudaController(ApiService api) : base(api) { }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new GestionAyudaViewModel
            {
                PeriodoActual = DateTime.Now.ToString("yyyy-MM")
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Tiendas()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/tiendas", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> Tipos()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/tipos", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string periodo)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/buscar", new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["periodo"] = periodo
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/nuevo", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, new { });
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(string docEntry, [FromBody] GestionAyudaUpdateRequest payload)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(docEntry))
                return BadRequest(new { message = "DocEntry inválido." });

            if (payload?.MGS_CL_GESDETCollection == null || payload.MGS_CL_GESDETCollection.Count == 0)
                return BadRequest(new { message = "No se recibieron registros para actualizar." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/actualizar", new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["docEntry"] = docEntry
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, payload);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            return Ok(resp);
        }
    }
}
