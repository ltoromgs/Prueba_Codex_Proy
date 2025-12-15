using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class ProjectAddDTO
    {

        [Required]
        [JsonProperty("Code")]
        public string Proyecto_Code { get; set; }
        [Required]
        [JsonProperty("Name")]
        public string Nombre_Proyecto { get; set; }
        [Required]
        [JsonProperty("U_MGS_CL_RUCPRO")]
        public string RUC { get; set; }
        [JsonProperty("U_MGS_CL_RAZSOC")]
        public string? RazonSocial { get; set; }
        //[Required]
        //[JsonProperty("ValidFrom")]
        //public string Fecha_Inicio { get; set; }
        //[Required]
        //[JsonProperty("ValidTo")]
        //public string Fecha_Fin { get; set; }
        [Required]
        [JsonProperty("U_MGS_CL_JEFE")]
        public string Jefe_Cuenta { get; set; }
        [Required]
        [JsonProperty("U_MGS_CL_CODCON")]
        public string Contrato_Code { get; set; }
        [JsonProperty("U_MGS_CL_ESTPRO")]
        protected string U_MGS_CL_ESTPRO { get; set; } = "4"; //cambio de 2 a 4
        [JsonProperty("U_MGS_CL_TIPPRO")]
        protected string Tipo_Proyecto { get; set; } = "3";
        [Required]
        [JsonProperty("U_MGS_CL_METPRO")]
        public string Metodologia { get; set; }
        [Required]
        [JsonProperty("U_MGS_CL_UNINEG")]
        public string UnidadNegocioID { get; set; }
        [Required]
        [RegularExpression("^(1|2)$", ErrorMessage = "Valor inválido. Valores permitidos son: 1 - Dolares, 2 - Soles")]
        [JsonProperty("U_MGS_CL_MONPRO")]
        public string Moneda { get; set; }

        
        [JsonProperty("U_MGS_CL_FAMILI")]
        public string Familia { get; set; }
    }

    //public class ProjectAddDTO
    //{
    //    [Required]
    //    [JsonProperty("Code")]
    //    public string Code { get; set; }
    //    [Required]
    //    [JsonProperty("Name")]
    //    public string Name { get; set; }
    //    [Required]
    //    [JsonProperty("U_MGS_CL_RUCPRO")]
    //    public string U_MGS_CL_RUCPRO { get; set; }
    //    [Required]
    //    [JsonProperty("ValidFrom")]
    //    public string ValidFrom { get; set; }
    //    [Required]
    //    [JsonProperty("ValidTo")]
    //    public string ValidTo { get; set; }
    //    [Required]
    //    [JsonProperty("U_MGS_CL_JEFE")]
    //    public string U_MGS_CL_JEFE { get; set; }
    //    [Required]
    //    [JsonProperty("U_MGS_CL_CODCON")]
    //    public string U_MGS_CL_CODCON { get; set; }
    //    [JsonProperty("U_MGS_CL_ESTPRO")]
    //    protected string U_MGS_CL_ESTPRO { get; set; } = "2";
    //    [JsonProperty("U_MGS_CL_TIPPRO")]
    //    protected string U_MGS_CL_TIPPRO { get; set; } = "3";
    //    [Required]
    //    [JsonProperty("U_MGS_CL_METPRO")]
    //    public string U_MGS_CL_METPRO { get; set; }
    //    [Required]
    //    [JsonProperty("U_MGS_CL_UNINEG")]
    //    public string U_MGS_CL_UNINEG { get; set; }
    //    [Required]
    //    [JsonProperty("U_MGS_CL_MONPRO")]
    //    public string U_MGS_CL_MONPRO { get; set; }
    //}
}
