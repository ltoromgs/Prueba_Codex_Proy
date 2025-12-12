using Newtonsoft.Json;
using ArellanoCore.Api.Swagger;
using System.ComponentModel.DataAnnotations;

namespace ArellanoCore.Api.Entities.ObjectSAP
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
