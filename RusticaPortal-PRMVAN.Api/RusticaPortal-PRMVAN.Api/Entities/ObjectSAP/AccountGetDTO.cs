using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Swagger;
using System.ComponentModel.DataAnnotations;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class AccountGetDTO
    {      
        [JsonProperty("Param1")]
        //[SwaggerIgnore]
        public string? Username { get; set; }
        

        [JsonProperty("Param2")]
        public string? Password { get; set; }


    }
}
