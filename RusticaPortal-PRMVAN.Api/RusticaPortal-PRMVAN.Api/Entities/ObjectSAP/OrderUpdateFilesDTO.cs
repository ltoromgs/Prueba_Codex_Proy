using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class OrderUpdateFilesDTO
    {
        [JsonProperty("U_MGS_CL_APROEST")]
        [RegularExpression("^(0|1)$", ErrorMessage = "Valor inválido. Valores permitidos son: 0 - NO, 1 - SI ")]
        public string? Aprobacion_Estudio { get; set; }

        [JsonProperty("U_MGS_CL_FICCLI")]
        [RegularExpression("^(0|1)$", ErrorMessage = "Valor inválido. Valores permitidos son: 0 - NO, 1 - SI ")]
        public string? Ficha_Cliente { get; set; }
        [JsonProperty("U_MGS_CL_PROFIN")]
        [RegularExpression("^(0|1)$", ErrorMessage = "Valor inválido. Valores permitidos son: 0 - NO, 1 - SI ")]
        public string? Propuesta_Final { get; set; }
        [JsonProperty("U_MGS_CL_HOJPRE")]
        [RegularExpression("^(0|1)$", ErrorMessage = "Valor inválido. Valores permitidos son: 0 - NO, 1 - SI ")]
        public string? Hojas_de_Precios { get; set; }
        [JsonProperty("U_MGS_CL_ORDTRA")]
        [RegularExpression("^(0|1)$", ErrorMessage = "Valor inválido. Valores permitidos son: 0 - NO, 1 - SI ")]
        public string? Orden_Trabajo { get; set; }
    }
}
