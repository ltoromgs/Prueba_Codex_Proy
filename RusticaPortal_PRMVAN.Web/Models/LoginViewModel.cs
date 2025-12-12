// Models/LoginViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace RusticaPortal_PRMVAN.Web.Models
{
    public class LoginViewModel
    {

        public int Empresa { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // Nuevos, para contacto y logo
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string LogoUrl { get; set; }

        // Nueva lista de empresas para el dropdown
        //public IEnumerable<SelectListItem> Empresas { get; set; }

        public IEnumerable<SelectListItem> Empresas { get; set; } = Enumerable.Empty<SelectListItem>();

    }
}




