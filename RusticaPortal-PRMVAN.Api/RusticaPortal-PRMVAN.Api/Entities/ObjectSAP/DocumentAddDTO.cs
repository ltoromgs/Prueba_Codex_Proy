using Newtonsoft.Json;
using ArellanoCore.Api.Swagger;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{

    public class DocumentAddDTO
    {
        [Required]
        [JsonProperty("CardCode")]
        public string CardCode { get; set; }
        private string _a;
        [Required]
        [JsonProperty("DocDate")]
        public string DocDate { get; set; }
    /*{
        get { return _a; }
        set
        {
            _a = value;
            FechaDoc = value;
            //FechaNecesaria = value;
        }
    }*/
        [Required]
        [JsonProperty("DocDueDate")]
        public string DocDueDate { get; set; }
        [Required]
        [JsonProperty("TaxDate")]
        public string TaxDate { get; set; }
       /* [Required]
        [JsonProperty("RequriedDate")]
        protected string FechaNecesaria { get; private set; }*/

        [JsonProperty("DocType")]
        [SwaggerIgnore]
        protected string DocType { get; set; } = "dDocument_Service";
       // [JsonProperty("RequesterDepartment")]
       // protected string Department { get; set; } = "06";
       // [JsonProperty("U_MGS_CL_CODINTPYP")]
       // public string Cod_PyP { get; set; }
        [JsonProperty("U_MGS_LC_TIPOPE")]
        protected string Tipo_Operacion { get; set; } = "01";
   
        [JsonProperty("NumAtCard")]
       
        public string NumAtCard { get; set; }
        [JsonProperty("Indicator")]
        [SwaggerIgnore]
        protected string Indicator { get; set; } = "01";
        [JsonProperty("Project")]
        [SwaggerIgnore]
        protected string Project { get; set; } = "GENERICO";
        [JsonProperty("U_MGS_FE_TIPFAC")]
        [SwaggerIgnore]
        protected string U_MGS_FE_TIPFAC { get; set; } = "0101";
         [JsonProperty("U_MGS_FE_MEDPAG")]
        [SwaggerIgnore]
        protected string U_MGS_FE_MEDPAG { get; set; } = "001";
        [JsonProperty("U_MGS_FE_ESTFAC")]
        [SwaggerIgnore]
        protected string U_MGS_FE_ESTFAC { get; set; } = "DP";
        
        /* [JsonProperty("DocCurrency")]
         [SwaggerIgnore]
         public string DocCurrency { get; set; } = "USD";*/
        [JsonProperty("DocumentLines")]
        public List<DocumentLine4> DocumentLines { get; set; }
    }

    public class DocumentLine4
    {
       
        [JsonProperty("TaxCode")]
        public string Tax_Code { get; set; }

        [JsonProperty("UnitPrice")]
        public double Monto_tarifa { get; set; }

        [JsonProperty("U_MGS_LC_SERCOM")]
        [SwaggerIgnore]
        protected string U_MGS_LC_SERCOM { get; set; } = "SEV00020";

        [JsonProperty("AccountCode")]
        [SwaggerIgnore]
        protected string AccountCode { get; set; } = "70321001";

        [JsonProperty("ItemDescription")]
        [SwaggerIgnore]
        protected string ItemDescription { get; set; } = "Merkadat";
        [JsonProperty("WTLiable")]
        [SwaggerIgnore]
        protected string WTLiable { get; set; } = "tNO";

        [JsonProperty("U_MGS_LC_GRUDET")]
        [SwaggerIgnore]
        protected string U_MGS_LC_GRUDET { get; set; } = "000";

        [JsonProperty("U_MGS_LC_GRUPER")]
        [SwaggerIgnore]
        protected string U_MGS_LC_GRUPER { get; set; } = "0000";
        [JsonProperty("CostingCode")]
        [SwaggerIgnore]
        protected string CostingCode { get; set; } = "C9010";
        [JsonProperty("ProjectCode")]
        [SwaggerIgnore]
        protected string ProjectCode { get; set; } = "GENERICO";

        [JsonProperty("U_MGS_FE_OPEIGV")]
        [SwaggerIgnore]
        protected string U_MGS_FE_OPEIGV { get; set; } = "G";
        [JsonProperty("U_MGS_FE_ONEROSO")]
        [SwaggerIgnore]
        protected string U_MGS_FE_ONEROSO { get; set; } = "01";
        [JsonProperty("U_MGS_FE_TIPAFE")]
        [SwaggerIgnore]
        protected string U_MGS_FE_TIPAFE { get; set; } = "10";

        /*    
        [JsonProperty("ItemCode")]
        public string CatCost_Code { get; set; }

        [JsonProperty("U_MGS_CL_NITEMPYP")]
        public string Item_del_Presupuesto { get; set; }
        [JsonProperty("Quantity")]
        public double Cantidad_Presupuestada { get; set; }
        [JsonProperty("Price")]
        public double Costo_Unit { get; set; }
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
        */
    }
}
