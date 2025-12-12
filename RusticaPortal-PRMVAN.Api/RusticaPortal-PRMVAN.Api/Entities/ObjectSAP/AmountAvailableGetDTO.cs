using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArellanoCore.Api.Entities.ObjectSAP
{
    public class AmountAvailableGetDTO
    {
        public List<Value2> value { get; set; }

        // public List<Value1> value = new List<Value1>();


    }

    public class Value2
    {

        [JsonProperty("contrato")]
        public string contrato { get; set; }

        [JsonProperty("Project")]
        public string Project { get; set; }
           
        [JsonProperty("CategoriaCosto")]
        public string CategoriaCosto { get; set; }        

       /* [JsonProperty("precioUnit")]
        public double precioUnit { get; set; }*/

        [JsonProperty("montoPresu")]
        public double montoPresu { get; set; }
        [JsonProperty("montoOC")]
        public double montoOC { get; set; }

        [JsonProperty("montoFact")]
        public double montoFact { get; set; }

        [JsonProperty("montoDisp")]
        public double montoDisp { get; set; }
        
            // public List<Value1> value = new List<Value1>();        
        
    }
}
