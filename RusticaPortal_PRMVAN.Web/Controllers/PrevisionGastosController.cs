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
    public class PrevisionGastosController : BaseController
    {
        public PrevisionGastosController(ApiService api) : base(api) { }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new PrevisionGastosViewModel
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

            var endpoint = QueryHelpers.AddQueryString("/api/previsiongastos/tiendas", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<PrevisionGastoTiendaDto>());

            var lista = JsonConvert.DeserializeObject<List<PrevisionGastoTiendaDto>>(resp.Content) ?? new List<PrevisionGastoTiendaDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> ConceptosPrm()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/previsiongastos/conceptos-prm", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<PrevisionGastoCatalogoDto>());

            var lista = JsonConvert.DeserializeObject<List<PrevisionGastoCatalogoDto>>(resp.Content) ?? new List<PrevisionGastoCatalogoDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> MotivosGasto()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/previsiongastos/motivos-gasto", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<PrevisionGastoCatalogoDto>());

            var lista = JsonConvert.DeserializeObject<List<PrevisionGastoCatalogoDto>>(resp.Content) ?? new List<PrevisionGastoCatalogoDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Items(string? search)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/previsiongastos/items", new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["search"] = search
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<PrevisionGastoItemDto>());

            var lista = JsonConvert.DeserializeObject<List<PrevisionGastoItemDto>>(resp.Content) ?? new List<PrevisionGastoItemDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string periodo, string? tiendas, string motivo)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/previsiongastos/buscar", new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["periodo"] = periodo,
                ["tiendas"] = tiendas,
                ["motivo"] = motivo
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
            {
                return Ok(new PrevisionGastosSearchResponse());
            }

            var resultado = JsonConvert.DeserializeObject<PrevisionGastosSearchResponse>(resp.Content) ?? new PrevisionGastosSearchResponse();

            if (string.IsNullOrWhiteSpace(resultado.DocEntry))
            {
                var docEntryEndpoint = QueryHelpers.AddQueryString("/api/previsiongastos/docentry", new Dictionary<string, string?>
                {
                    ["empresa"] = empresa,
                    ["periodo"] = periodo
                });

                var docEntryResp = await _apiService.GetAsync<ResponseInformation>(docEntryEndpoint);
                if (docEntryResp != null && docEntryResp.Registered && !string.IsNullOrWhiteSpace(docEntryResp.Content))
                {
                    resultado.DocEntry = docEntryResp.Content;
                }
            }

            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarTodo([FromBody] PrevisionGastoSaveRequest payload)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (payload == null)
                return BadRequest(new { message = "No se recibió información para guardar." });

            var endpoint = QueryHelpers.AddQueryString("/api/previsiongastos/actualizar-todo", new Dictionary<string, string?>
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
