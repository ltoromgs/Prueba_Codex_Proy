using Newtonsoft.Json;
using ArellanoCore.Api.Swagger;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{
   
    public class PurchaseRequestAddDTO
    {
        private string _a;
        [Required]
        [JsonProperty("DocDate")]
        public string Fecha_Inicio_Pre
        {
            get { return _a; }
            set
            {
                _a = value;
                FechaDoc = value;
                FechaNecesaria = value;
            }
        }
        [Required]
        [JsonProperty("DocDueDate")]
        public string Fecha_Fin_Pre { get; set; }
        [Required]
        [JsonProperty("TaxDate")]
        protected string FechaDoc { get; private set; }
        [Required]
        [JsonProperty("RequriedDate")]
        protected string FechaNecesaria { get; private set; }
        //[JsonProperty("DocCurrency")]
        //public string DocCurrency { get; set; }
        [JsonProperty("DocType")]
        protected string DocType { get; set; } = "dDocument_Items";
        [JsonProperty("RequesterDepartment")]
        protected string Department { get; set; } = "06"; //pyp
        [JsonProperty("U_MGS_CL_CODINTPYP")]
        public string Cod_PyP { get; set; } 
        [JsonProperty("U_MGS_LC_TIPOPE")]
        protected string Tipo_Operacion { get; set; } = "02";
        //[JsonProperty("DocCurrency")]
        //[SwaggerIgnore]
        //public string DocCurrency { get; set; } = "USD";
        [JsonProperty("DocumentLines")]
        public List<DocumentLine2> DocumentLines { get; set; }
    }

    public class DocumentLine2
    {
        [JsonProperty("ItemCode")]
        public string CatCost_Code { get; set; }
        
        [JsonProperty("U_MGS_CL_NITEMPYP")]
        public string Item_del_Presupuesto { get; set; }
        [JsonProperty("Quantity")]
        public double Cantidad_Presupuestada { get; set; }
        [JsonProperty("Price")]
        public double Costo_Unit { get; set; }
        [JsonProperty("U_MGS_CL_CANINI")]
        public double Cantidad_Inicial { get; set; }
        [JsonProperty("U_MGS_CL_PREINI")]
        public double Costo_Unit_Inicial { get; set; }
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
        [JsonProperty("Currency")]
        public string moneda { get; set; }
    }
}
