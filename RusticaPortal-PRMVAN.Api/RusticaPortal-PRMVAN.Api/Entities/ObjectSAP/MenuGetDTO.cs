using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Swagger;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class MenuItemDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("ruta")]
        public string Ruta { get; set; }

        [JsonProperty("hijos")]
        public List<MenuItemDto> Hijos { get; set; } = new();
    }
}