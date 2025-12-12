using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{
    public class PurchaseRequestUpdateDTO
    {
        private string _a;
        [JsonProperty("DocDate")]
        public string? Fecha_Inicio_Pre
        {
            get { return _a; }
            set
            {
                _a = value;
                FechaDoc = value;
                FechaNecesaria = value;
            }
        }

        [JsonProperty("DocDueDate")]
        public string? Fecha_Fin_Pre { get; set; }
        [JsonProperty("TaxDate")]
        protected string? FechaDoc { get; private set; }
        [JsonProperty("RequriedDate")]
        protected string?    FechaNecesaria { get; private set; }
        //[JsonProperty("DocCurrency")]
        //public string DocCurrency { get; set; }

        [JsonProperty("U_MGS_CL_CODINTPYP")]
        public string? Cod_PyP { get; set; }

        [JsonProperty("DocumentLines")]
        public List<DocumentLine3>? DocumentLines { get; set; }
    }

    public class DocumentLine3
    {
        private string _b;

        [JsonProperty("ItemCode")]
        public string? CatCost_Code { get; set; }

        [JsonProperty("U_MGS_CL_NITEMPYP")]
        //public string? Item_del_Presupuesto { get; set; }
        public string? Item_del_Presupuesto
        {
            get { return _b; }
            set
            {
                _b = value;
                LineNum = int.Parse(value) - 1;
            }
        }
        [JsonProperty("LineNum")]
        protected int? LineNum { get; private set; }
        [JsonProperty("Quantity")]
        public double? Cantidad_Presupuestada { get; set; }
        [JsonProperty("UnitPrice")]
        public double? Costo_Unit { get; set; }

        [JsonProperty("ProjectCode")]
        public string? Code { get; set; }

        [JsonProperty("U_MGS_CL_TIPBENPRO")]
        public string? Tipo_Beneficio { get; set; }

        [JsonProperty("TaxCode")]
        protected string Tax_Code { get; set; } = "IGV";
        [JsonProperty("CostingCode")]
        protected string OcrCode { get; set; } = "C9204";
  
        [JsonProperty("U_MGS_LC_GRUPER")]
        protected string Grupo_Per { get; set; } = "0000";
        [JsonProperty("U_MGS_LC_GRUDET")]
        protected string Grupo_Det { get; set; } = "022";
        [JsonProperty("U_MGS_CL_CANINI")]
        public double Cantidad_Inicial { get; set; }
        [JsonProperty("U_MGS_CL_PREINI")]
        public double Costo_Unit_Inicial { get; set; }
        [JsonProperty("Currency")]
        public string moneda { get; set; }
    }
}
