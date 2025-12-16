using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Web.Models;
using RusticaPortal_PRMVAN.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
    // Hereda de tu BaseController para que ya coloque el menú, etc.
    public class FactoresController : BaseController
    {
        public FactoresController(ApiService api) : base(api) { }

        // Página
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new MatrizFactoresViewModel
            {
                PeriodoActual = DateTime.Now.ToString("yyyy-MM"),
                Factores = new List<FactoresModel>() // lo que cargues
            };

            return View(vm);
            // La vista arranca vacía; el usuario buscará con filtros
            //return View(new List<FactoresModel>());
        }

        // Data GET para la grilla (llamado por fetch)
        // /Factores/Buscar?periodo=2024-10&tiendas=P0045,P0031
        [HttpGet]
        public async Task<IActionResult> Buscar(string periodo, string? tiendas)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            // Pasar tal cual a tu API (ajusta nombres si difieren)
            var endpoint = QueryHelpers.AddQueryString("/api/factores", new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["periodo"] = periodo,
                ["tiendas"] = tiendas
            });
            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<FactoresModel>()); // vacío sin romper

            var lista = JsonConvert.DeserializeObject<List<FactoresModel>>(resp.Content) ?? new List<FactoresModel>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Tiendas()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/factores/tiendas", new Dictionary<string, string?>
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
        public async Task<IActionResult> Previsualizar(string? periodoOrigen)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/factores/nuevo", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<FactoresModel>());

            var lista = JsonConvert.DeserializeObject<List<FactoresModel>>(resp.Content) ?? new List<FactoresModel>();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(string docEntry, [FromBody] FactorUpdateRequest payload)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(docEntry))
                return BadRequest(new { message = "DocEntry inválido." });

            if (payload?.MGS_CL_FACDETCollection == null || payload.MGS_CL_FACDETCollection.Count == 0)
                return BadRequest(new { message = "No se recibieron factores para actualizar." });

            var endpoint = QueryHelpers.AddQueryString("/api/factores/actualizar", new Dictionary<string, string?>
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
        public async Task<IActionResult> Crear([FromBody] FactorCreateRequest payload)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (payload == null || string.IsNullOrWhiteSpace(payload.U_MGS_CL_PERIODO))
                return BadRequest(new { message = "El periodo es obligatorio." });

            if (payload.MGS_CL_FACDETCollection == null || payload.MGS_CL_FACDETCollection.Count == 0)
                return BadRequest(new { message = "No se recibieron registros para crear." });

            var endpoint = QueryHelpers.AddQueryString("/api/factores/crear", new Dictionary<string, string?>
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
