using Newtonsoft.Json;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class TiendaActivaDTO
    {
        [JsonProperty("Codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
