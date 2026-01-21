using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
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
                PeriodoActual = DateTime.Now.ToString("yyyy-MM"),
                Detalle = new List<GestionAyudaModel>()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string periodo, string? tienda)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var queryParams = new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["periodo"] = periodo
            };
            if (!string.IsNullOrWhiteSpace(tienda))
            {
                queryParams["tienda"] = tienda;
            }

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/buscar", queryParams);
            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<GestionAyudaModel>());

            var lista = JsonConvert.DeserializeObject<List<GestionAyudaModel>>(resp.Content) ?? new List<GestionAyudaModel>();
            return Ok(lista);
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
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<TiendaModel>());

            var lista = JsonConvert.DeserializeObject<List<TiendaModel>>(resp.Content) ?? new List<TiendaModel>();
            return Ok(lista);
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
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<GestionAyudaTipoModel>());

            var lista = JsonConvert.DeserializeObject<List<GestionAyudaTipoModel>>(resp.Content) ?? new List<GestionAyudaTipoModel>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> NuevoPreview()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/nuevo-preview", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<GestionAyudaModel>());

            var lista = JsonConvert.DeserializeObject<List<GestionAyudaModel>>(resp.Content) ?? new List<GestionAyudaModel>();
            return Ok(lista);
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

            if (!resp.Registered)
                return BadRequest(new { message = resp.Message, content = resp.Content });

            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] GestionAyudaCreateRequest payload)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (payload == null || string.IsNullOrWhiteSpace(payload.U_MGS_CL_PERIODO))
                return BadRequest(new { message = "El periodo es obligatorio." });

            if (payload.MGS_CL_GESDETCollection == null || payload.MGS_CL_GESDETCollection.Count == 0)
                return BadRequest(new { message = "No se recibieron registros para crear." });

            var endpoint = QueryHelpers.AddQueryString("/api/gestionayuda/crear", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, payload);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            if (!resp.Registered)
                return BadRequest(new { message = resp.Message, content = resp.Content });

            return Ok(resp);
        }
    }
}
