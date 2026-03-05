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
    public class AdministracionGAEController : BaseController
    {
        public AdministracionGAEController(ApiService api) : base(api) { }

        [HttpGet]
        public IActionResult Index()
        {
            var now = DateTime.Now;
            var primerDia = new DateTime(now.Year, now.Month, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

            var vm = new AdministracionGaeViewModel
            {
                Periodo = primerDia.ToString("yyyy-MM"),
                FechaDesde = primerDia.ToString("yyyy-MM-dd"),
                FechaHasta = ultimoDia.ToString("yyyy-MM-dd")
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Tiendas()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/AdministracionGAE/tiendas", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<AdministracionGaeTiendaDto>());

            var lista = JsonConvert.DeserializeObject<List<AdministracionGaeTiendaDto>>(resp.Content) ?? new List<AdministracionGaeTiendaDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> TiposGae()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/AdministracionGAE/tipos-gae", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<AdministracionGaeCatalogoDto>());

            var lista = JsonConvert.DeserializeObject<List<AdministracionGaeCatalogoDto>>(resp.Content) ?? new List<AdministracionGaeCatalogoDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> TiposGasto()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/AdministracionGAE/tipos-gasto", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<AdministracionGaeCatalogoDto>());

            var lista = JsonConvert.DeserializeObject<List<AdministracionGaeCatalogoDto>>(resp.Content) ?? new List<AdministracionGaeCatalogoDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> MotivosGasto()
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/AdministracionGAE/motivos-gasto", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<AdministracionGaeCatalogoDto>());

            var lista = JsonConvert.DeserializeObject<List<AdministracionGaeCatalogoDto>>(resp.Content) ?? new List<AdministracionGaeCatalogoDto>();
            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string fechaDesde, string fechaHasta, string? tiendas, string? filtros)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/AdministracionGAE/buscar", new Dictionary<string, string?>
            {
                ["empresa"] = empresa,
                ["fechaDesde"] = fechaDesde,
                ["fechaHasta"] = fechaHasta,
                ["tiendas"] = tiendas ?? string.Empty,
                ["filtros"] = filtros ?? string.Empty
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered || string.IsNullOrEmpty(resp.Content))
                return Ok(new List<AdministracionGaeDetalleDto>());

            var lista = JsonConvert.DeserializeObject<List<AdministracionGaeDetalleDto>>(resp.Content) ?? new List<AdministracionGaeDetalleDto>();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarTodo([FromBody] AdministracionGaeUpdateRequest request)
        {
            var empresa = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(empresa))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/AdministracionGAE/actualizar-todo", new Dictionary<string, string?>
            {
                ["empresa"] = empresa
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, request);

            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });

            List<AdministracionGaeUpdateResult> listaParsed = new();
            if (!string.IsNullOrWhiteSpace(resp.Content))
            {
                try
                {
                    listaParsed = JsonConvert.DeserializeObject<List<AdministracionGaeUpdateResult>>(resp.Content) ?? new List<AdministracionGaeUpdateResult>();
                }
                catch
                {
                    listaParsed = new List<AdministracionGaeUpdateResult>();
                }
            }

            return Ok(new
            {
                registered = resp.Registered,
                message = resp.Message,
                content = resp.Content,
                items = listaParsed
            });
        }
    }
}
