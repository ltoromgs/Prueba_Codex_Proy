using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RusticaPortal_PRMVAN.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Web.Models;

namespace RusticaPortal_PRMVAN.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ISessionTrackerService _sessionTracker;

        public AccountController(ApiService apiService, ISessionTrackerService sessionTracker)
        {
            _apiService = apiService;
            _sessionTracker = sessionTracker;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string reason = null)
        {
            // 1) traer lista de empresas
            var empresas = (await FetchEmpresas()).ToList();

            // 2) determinar empresa por defecto (la primera en la lista)
            var defaultEmpresaId = empresas
                .Select(e => int.TryParse(e.Value, out var id) ? id : 0)
                .FirstOrDefault();

            // 3) traer datos de contacto para esa empresa
            var contacto = await FetchContactInfo(defaultEmpresaId);

            // 4) armar viewmodel
            var vm = new LoginViewModel
            {
                Empresa = defaultEmpresaId,
                Empresas = empresas,
                ContactPhone = contacto.Phone,
                ContactEmail = contacto.Email,
                LogoUrl = contacto.LogoUrl
            };

            // 5) mensaje de sesión expirada
            if (reason == "sesion_expirada")
                ViewBag.Mensaje = "Tu sesión fue cerrada porque iniciaste sesión en otro navegador o dispositivo.";

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                // cerrar sesión previa
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // llamar a la API de login
                var body = new { username = model.Username, password = model.Password };
                var response = await _apiService.PostAsync<ResponseInformation>(
                    $"/api/Account?Empresa={model.Empresa}", body);

                if (response != null && response.Registered)
                {
                    // deserializar datos de usuario
                    var usuario = JsonConvert.DeserializeObject<AccountModel>(response.Content)!;

                    // sesión única
                    string sessionId = Guid.NewGuid().ToString();
                    await _sessionTracker.SetSessionForUser(model.Username, sessionId);

                    // crear claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,         model.Username),
                        new Claim("SessionId",             sessionId),
                        new Claim("Empresa",               model.Empresa.ToString()),
                        new Claim("UsuarioID",             usuario.UsuarioID),
                        new Claim("NombreCompleto",        usuario.NombreCompleto),
                        new Claim("CardCode",              usuario.CardCode),
                        new Claim("PerfilId",              usuario.PerfilId),
                        new Claim("NombrePerfil",          usuario.NombrePerfil)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // guardar popup en sesión
                    HttpContext.Session.SetString("PopupImage", usuario.Popup ?? "");

                    return RedirectToAction("Index", "Home");
                }

                // en caso de credenciales inválidas, recargar empresas y contacto
                ViewBag.Error = "Credenciales inválidas";
                model.Empresas = (await FetchEmpresas()).ToList();
                var contactoErr = await FetchContactInfo(model.Empresa);
                model.ContactPhone = contactoErr.Phone;
                model.ContactEmail = contactoErr.Email;
                model.LogoUrl = contactoErr.LogoUrl;

                return View(model);
            }
            catch (Exception ex)
            {
                // en caso de excepción, recargar empresas y contacto
                ViewBag.Error = ex.Message;
                model.Empresas = (await FetchEmpresas()).ToList();
                var contactoErr2 = await FetchContactInfo(model.Empresa);
                model.ContactPhone = contactoErr2.Phone;
                model.ContactEmail = contactoErr2.Email;
                model.LogoUrl = contactoErr2.LogoUrl;

                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ——— Helpers privados ———

        private async Task<ContactInfoModel> FetchContactInfo(int empresaId)
        {
            var resp = await _apiService.GetAsync<ResponseInformation>(
                $"/api/contacto?empresa={empresaId}");
            if (resp?.Registered == true && !string.IsNullOrEmpty(resp.Content))
            {
                return JsonConvert.DeserializeObject<ContactInfoModel>(resp.Content)!;
            }
            // fallback estático
            return new ContactInfoModel
            {
                Phone = "(n/d)",
                Email = "(n/d)",
                LogoUrl = Url.Content("~/img/default_logo.png")
            };
        }
        private async Task<List<SelectListItem>> FetchEmpresas()
        {
            var resp = await _apiService.GetAsync<ResponseInformation>("/api/Empresas");
            if (resp?.Registered == true && !string.IsNullOrEmpty(resp.Content))
            {
                var list = JsonConvert.DeserializeObject<List<EmpresaModel>>(resp.Content)!;

                var items = list.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre,
                    Selected = (e.Id == 1) // ⬅️ por defecto ID=1
                }).ToList();

                if (!items.Any(i => i.Selected) && items.Count > 0)
                    items[0].Selected = true; // respaldo

                return items;
            }

            // Fallback con 1 por defecto
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "(empresa por defecto)", Selected = true }
    };
        }

        /*private async Task<IEnumerable<SelectListItem>> FetchEmpresas()
        {
            var resp = await _apiService.GetAsync<ResponseInformation>("/api/Empresas");
            if (resp?.Registered == true && !string.IsNullOrEmpty(resp.Content))
            {
                var list = JsonConvert.DeserializeObject<List<EmpresaModel>>(resp.Content)!;
                return list.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre
                });
            }
            // fallback
            return new[]
            {
                new SelectListItem { Value = "0", Text = "(sin empresas)" }
            };
        }*/
    }
}
