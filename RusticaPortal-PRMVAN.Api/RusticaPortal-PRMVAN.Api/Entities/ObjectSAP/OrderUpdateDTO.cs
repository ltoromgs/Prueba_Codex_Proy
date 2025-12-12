using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{

    public class OrderUpdateDTO
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
            }
        }
        [JsonProperty("DocDueDate")]
        public string? Fecha_Fin_Pre { get; set; }
        [JsonProperty("TaxDate")]
        protected string? FechaDoc { get; private set; }
        [JsonProperty("U_MGS_CL_CODINTPYP")]
        public string? Cod_PyP { get; set; }

        [JsonProperty("DocumentLines")]
        
        public List<DocumentLine1>? DocumentLines { get; set; }
    }

    public class DocumentLine1
    {
        private string _b;

        [JsonProperty("ItemCode")]
        public string? Tarifa_ID { get; set; }
        [JsonProperty("U_MGS_CL_NITEMPYP")]
        public string? Nro_Secuencia_Item
        {
            get { return _b; }
            set
            {
                _b = value;
                LineNum = int.Parse(value) - 1 ;
            }
        }
        [JsonProperty("LineNum")]
        protected int? LineNum { get; private set; }
        [JsonProperty("Quantity")]
        public double? Cantidad { get; set; }
        [JsonProperty("UnitPrice")]
        public double? Monto_tarifa { get; set; }

        [JsonProperty("ProjectCode")]
        public string? Code { get; set; }
        [JsonProperty("U_MGS_CL_TIPBENPRO")]
        public string? Tipo_Beneficio { get; set; }
    }
}
