using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Web.Services;
using RusticaPortal_PRMVAN.Web.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System;

public abstract class BaseController : Controller
{
    protected readonly ApiService _apiService;

    public BaseController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var menuModel = new MenuModel { Menu = new List<MenuItem>() };

        string? empresa = User?.Claims?.FirstOrDefault(c => c.Type == "Empresa")?.Value;

        try
        {
            if (!string.IsNullOrEmpty(empresa))
            {
                var resp = await _apiService.GetAsync<ResponseInformation>(
                    $"/api/menu?empresa={empresa}&userId={User?.Identity?.Name}");

                if (resp?.Registered == true && !string.IsNullOrEmpty(resp.Content))
                {
                    menuModel = JsonConvert.DeserializeObject<MenuModel>(resp.Content) ?? menuModel;
                }
                else
                {
                    // API respondió pero sin datos válidos
                    TempData["GlobalError"] = string.IsNullOrWhiteSpace(resp?.Message)
                        ? "No se pudo obtener el menú en este momento."
                        : resp!.Message;
                }
            }
            else
            {
                TempData["GlobalError"] = "Empresa no especificada en la sesión.";
            }
        }
        catch (HttpRequestException)
        {
            TempData["GlobalError"] = "No hay conexión con el servidor. Inténtalo nuevamente más tarde.";
        }
        catch (TaskCanceledException tce) when (!tce.CancellationToken.IsCancellationRequested)
        {
            TempData["GlobalError"] = "La solicitud tardó demasiado (timeout). Inténtalo nuevamente.";
        }
        catch (Exception)
        {
            TempData["GlobalError"] = "Ocurrió un error inesperado al cargar el menú.";
        }

        // Deja el menú disponible para el _Layout
        ViewBag.MenuModel = menuModel;

        await next();
    }
}


