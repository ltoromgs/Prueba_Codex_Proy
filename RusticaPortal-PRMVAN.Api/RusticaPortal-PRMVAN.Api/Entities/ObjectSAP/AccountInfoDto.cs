using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Swagger;
using System.ComponentModel.DataAnnotations;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class AccountInfoDto
    {
        public string UsuarioID { get; set; }
        public string NombreCompleto { get; set; }
        public string CardCode { get; set; }
        public string PerfilId { get; set; }
        public string NombrePerfil { get; set; }
        public string Popup { get; set; }
        
    }
}
