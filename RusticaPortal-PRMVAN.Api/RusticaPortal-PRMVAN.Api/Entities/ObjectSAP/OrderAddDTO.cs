using Newtonsoft.Json;
using ArellanoCore.Api.Swagger;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{
    public class OrderAddDTO
    {
        private string _a;

        [JsonProperty("CardCode")]
        [SwaggerIgnore]
        public string Cliente { get; set; } = "C111";
        [Required]
        [JsonProperty("DocDate")]
        public string Fecha_Inicio_Pre { get{ return _a; } set
            {
                _a = value;
                FechaDoc = value;
            } 
        }
        [Required]
        [JsonProperty("DocDueDate")]
        public string Fecha_Fin_Pre { get; set; }
        [Required]
        [JsonProperty("TaxDate")]
        protected string FechaDoc { get; private set ; }
        [Required]
        [JsonProperty("U_MGS_CL_CODINTPYP")]
        public string Cod_PyP { get; set; }
        [JsonProperty("U_MGS_LC_TIPOPE")]
        protected string Tipo_Operacion { get; set; } = "01";

        [JsonProperty("DocCurrency")]
        [SwaggerIgnore]
        public string DocCurrency { get; set; } = "USD";

        [JsonProperty("Project")]
        [SwaggerIgnore]
        public string Project { get; set; } = "";

        [JsonProperty("DocumentLines")]
        public List<DocumentLine> DocumentLines { get; set; }
    }

    public class DocumentLine
    {
        [JsonProperty("ItemCode")]
        public string Tarifa_ID { get; set; }
        [JsonProperty("DocType")]
        protected string DocType { get; set; } = "dDocument_Items";
        [JsonProperty("U_MGS_CL_NITEMPYP")]
        public string Nro_Secuencia_Item { get; set; }
        [JsonProperty("Quantity")]
        public double Cantidad { get; set; }
        [JsonProperty("Price")]
        public double Monto_tarifa { get; set; }
        [JsonProperty("TaxCode")]
        protected string Tax_Code { get; set; } = "IGV";
        [JsonProperty("CostingCode")]
        protected string OcrCode { get; set; } = "C9204";
        [JsonProperty("ProjectCode")]
        public string Code { get; set; }
        [JsonProperty("U_MGS_LC_GRUPER")]
        protected string Grupo_Per { get; set; } = "0000";
        [JsonProperty("U_MGS_LC_GRUDET")]
        protected string Grupo_Det { get; set; } = "022";
        [JsonProperty("U_MGS_CL_TIPBENPRO")]
        public string Tipo_Beneficio { get; set; }
        
    }

}
