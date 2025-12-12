using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ArellanoCore.Api.Swagger;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{
    public class DocumentGetDTO
    {

        public List<Value1> value { get; set; } 

        // public List<Value1> value = new List<Value1>();


    }

    public class Value1
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }

        public string DocDate { get; set; }
        public string DocDueDate { get; set; }

        [JsonProperty("U_MGS_CL_CODINTPYP")]
        public string Cod_PyP { get; set; }
      

        [JsonProperty("NumAtCard")]
        public string NumAtCard { get; set; }
        [JsonProperty("TaxDate")]
        public string TaxDate { get; set; }
        [JsonProperty("idEmpresa")]
        public string idEmpresa { get; set; }
        [JsonProperty("Project")]
        public string Project { get; set; }
        [JsonProperty("tipoDoc")]
        public string tipoDoc { get; set; }
        [JsonProperty("Moneda")]
        public string DocCur { get; set; }
        [JsonProperty("TotalSol")]
        public double TotalSol { get; set; }
        [JsonProperty("TotalDol")]
        public double TotalDol { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("UserCreator")]
        public string UserCreator { get; set; }
        [JsonProperty("Ruc")]
        public string Ruc { get; set; }
        [JsonProperty("Referencia")]
        public string Proveedor { get; set; }
        [JsonProperty("JrnlMemo")]
        public string JrnlMemo { get; set; }
                       
                        
        public List<DocumentLineD> DocumentLines { get; set; }
        //public List<DocumentLineD> DocumentLines = new List<DocumentLineD>();
    }

    public class DocumentLineD
    {
        [JsonProperty("ItemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("U_MGS_CL_NITEMPYP")]
        public string U_MGS_CL_NITEMPYP { get; set; }
        [JsonProperty("Quantity")]
        public double Quantity { get; set; }
        [JsonProperty("Price")]
        public double Price { get; set; }

        [JsonProperty("QuantityIni")]
        public double QuantityIni { get; set; }
        [JsonProperty("PriceIni")]
        public double PriceIni { get; set; }

        [JsonProperty("Project")]
        public string Project { get; set; }
        [JsonProperty("U_MGS_CL_TIPBENPRO")]
        public string U_MGS_CL_TIPBENPRO { get; set; }
        [JsonProperty("LineTotal")]
        public double LineTotal { get; set; }

        [JsonProperty("UnidadNegocio")]
        public string UnidadNegocio { get; set; }

        [JsonProperty("Porcentaje")]
        public double Porcentaje { get; set; }

        [JsonProperty("JefeCuenta")]
        public string JefeCuenta { get; set; }

        [JsonProperty("Familia")]
        public string Familia { get; set; }

         [JsonProperty("EstadoProyecto")]
        public string EstadoProyecto { get; set; }

        


        /* [JsonProperty("TaxCode")]
         public string Tax_Code { get; set; } 
         [JsonProperty("CostingCode")]
         public string OcrCode { get; set; } 
         [JsonProperty("ProjectCode")]
         public string Code { get; set; }
         [JsonProperty("U_MGS_LC_GRUPER")]
         public string Grupo_Per { get; set; } 
         [JsonProperty("U_MGS_LC_GRUDET")]
         public string Grupo_Det { get; set; } 
         [JsonProperty("U_MGS_CL_TIPBENPRO")]
         public string Tipo_Beneficio { get; set; }*/


    }
}
