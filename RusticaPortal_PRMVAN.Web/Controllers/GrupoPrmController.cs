using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RusticaPortal_PRMVAN.Web.Models;
using RusticaPortal_PRMVAN.Web.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
    public class GrupoPrmController : BaseController
    {
        public GrupoPrmController(ApiService api) : base(api) { }

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

            var endpoint = QueryHelpers.AddQueryString("/api/grupoprm/tiendas", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> TiposGasto()
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupoprm/tipos-gasto", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> Motivos()
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupoprm/motivos", new Dictionary<string, string?>
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

            var endpoint = QueryHelpers.AddQueryString("/api/grupoprm/grupos-maestro", new Dictionary<string, string?>
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

            var endpoint = QueryHelpers.AddQueryString("/api/grupoprm/articulos-maestro", new Dictionary<string, string?>
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

            var endpoint = QueryHelpers.AddQueryString($"/api/grupoprm/tienda/{tienda}/grupos", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpGet]
        public async Task<IActionResult> ArticulosPorGrupo(string tienda, string grupo)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(tienda))
                return BadRequest(new { message = "Tienda requerida." });

            if (string.IsNullOrWhiteSpace(grupo))
                return BadRequest(new { message = "Grupo requerido." });

            var endpoint = QueryHelpers.AddQueryString($"/api/grupoprm/tienda/{tienda}/grupo/{grupo}/articulos", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.GetAsync<ResponseInformation>(endpoint);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return BadRequest(resp);
            return Ok(resp);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarTodo(string tienda, [FromBody] GrupoPrmGuardarRequest payload)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (string.IsNullOrWhiteSpace(tienda))
                return BadRequest(new { message = "Tienda requerida." });

            if (payload == null)
                return BadRequest(new { message = "No se recibió información para guardar." });

            payload.Grupos ??= new List<GrupoPrmDto>();
            payload.Articulos ??= new List<PrmArticuloDto>();

            if (payload.Grupos.Count == 0 && payload.Articulos.Count == 0)
                return BadRequest(new { message = "No se recibieron cambios para guardar." });

            ResponseInformation resp = null;

            if (payload.Grupos.Count > 0)
            {
                var endpoint = QueryHelpers.AddQueryString($"/api/grupoprm/tienda/{tienda}/grupos/bulk", new Dictionary<string, string?>
                {
                    ["empresa"] = emp
                });

                var bulkRequest = new GrupoPrmBulkRequest { Items = payload.Grupos };
                if (bulkRequest.Items.Count == 0)
                    return BadRequest(new { message = "No se recibieron grupos para guardar." });

                resp = await _apiService.PostAsync<ResponseInformation>(endpoint, bulkRequest);
                if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
                if (!resp.Registered) return BadRequest(resp);
            }

            if (payload.Articulos.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(payload.Grupo))
                    return BadRequest(new { message = "Grupo requerido para artículos." });

                var endpoint = QueryHelpers.AddQueryString($"/api/grupoprm/tienda/{tienda}/grupo/{payload.Grupo}/articulos/bulk", new Dictionary<string, string?>
                {
                    ["empresa"] = emp
                });

                var bulkRequest = new PrmArticuloBulkRequest { Items = payload.Articulos };
                if (bulkRequest.Items.Count == 0)
                    return BadRequest(new { message = "No se recibieron artículos para guardar." });

                resp = await _apiService.PostAsync<ResponseInformation>(endpoint, bulkRequest);
                if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
                if (!resp.Registered) return BadRequest(resp);
            }

            return Ok(resp ?? new ResponseInformation { Registered = true, Message = "Sin cambios", Content = string.Empty });
        }

        [HttpPost]
        public async Task<IActionResult> CopiarTienda([FromBody] GrupoPrmCopiarRequest payload)
        {
            var emp = User.Claims.FirstOrDefault(c => c.Type == "Empresa")?.Value;
            if (string.IsNullOrEmpty(emp))
                return BadRequest(new { message = "Empresa no encontrada en sesión." });

            if (payload == null || string.IsNullOrWhiteSpace(payload.TiendaOrigen) || string.IsNullOrWhiteSpace(payload.TiendaDestino))
                return BadRequest(new { message = "Debe indicar tienda origen y destino." });

            var endpoint = QueryHelpers.AddQueryString("/api/grupoprm/copiar", new Dictionary<string, string?>
            {
                ["empresa"] = emp
            });

            var resp = await _apiService.PostAsync<ResponseInformation>(endpoint, payload);
            if (resp == null) return StatusCode(503, new { message = "Sin conexión con el API." });
            if (!resp.Registered) return Ok(resp);
            return Ok(resp);
        }
    }
}
