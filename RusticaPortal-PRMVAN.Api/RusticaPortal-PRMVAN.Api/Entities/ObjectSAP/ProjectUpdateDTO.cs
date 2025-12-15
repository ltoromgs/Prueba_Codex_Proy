using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class ProjectUpdateDTO
    {
   
        [JsonProperty("Name")]
        public string? Nombre_Proyecto { get; set; }
        [JsonProperty("U_MGS_CL_RUCPRO")]
        public string? RUC { get; set; }
        //[JsonProperty("ValidFrom")]
        //public string? Fecha_Inicio { get; set; }
        //[JsonProperty("ValidTo")]
        //public string? Fecha_Fin { get; set; }
        [JsonProperty("U_MGS_CL_JEFE")]
        public string? Jefe_Cuenta { get; set; }
        [JsonProperty("U_MGS_CL_CODCON")]
        public string? Contrato_Code { get; set; }
        [JsonProperty("U_MGS_CL_METPRO")]
        public string? Metodologia { get; set; }
        [JsonProperty("U_MGS_CL_UNINEG")]
        public string? UnidadNegocioID { get; set; }
        [JsonProperty("U_MGS_CL_MONPRO")]
        public string? Moneda { get; set; }
        [JsonProperty("U_MGS_CL_ESTPRO")]
        public string? Estado { get; set; }
        [JsonProperty("U_MGS_CL_RAZSOC")]
        public string? RazonSocial { get; set; }
        [JsonProperty("Active")]
        public string? Active { get; set; }
        [JsonProperty("U_MGS_CL_FAMILI")]
        public string Familia { get; set; }
    }
}
