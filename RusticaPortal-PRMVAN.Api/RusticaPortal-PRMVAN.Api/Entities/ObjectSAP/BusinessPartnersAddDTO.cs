using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Swagger;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class BusinessPartnersAddDTO
    {

        [JsonProperty("CardCode")]
        [SwaggerIgnore]
        public string? CardCode { get; set; }

        [JsonProperty("CardName")]
        public string? CardName { get; set; }

        [JsonProperty("CardForeignName")]
        public string? CardFName { get; set; }

        [JsonProperty("GroupCode")]
        [SwaggerIgnore]
        public int? GroupCode { get; set; } 

        [JsonProperty("CardType")]
        [SwaggerIgnore]
        public string? CardType { get; set; }

        [JsonProperty("FederalTaxID")]
        public string? LicTradNum { get; set; }

        [JsonProperty("U_MGS_LC_TIPDOC")]

        public string? U_MGS_LC_TIPDOC { get; set; }

        [JsonProperty("U_MGS_LC_TIPPER")]

        public string? U_MGS_LC_TIPPER { get; set; } 

        [JsonProperty("U_MGS_LC_PRINOM")]
        public string? U_MGS_LC_PRINOM { get; set; }


        [JsonProperty("U_MGS_LC_SEGNOM")]
        public string? U_MGS_LC_SEGNOM { get; set; }

        [JsonProperty("U_MGS_LC_APEPAT")]
        public string? U_MGS_LC_APEPAT { get; set; }

        [JsonProperty("U_MGS_LC_APEMAT")]
        public string? U_MGS_LC_APEMAT { get; set; }


        //BusinessParthersCLiente

        [JsonProperty("Currency")]
        [SwaggerIgnore]
        public string? Currency { get; set; }

        [JsonProperty("EmailAddress")]
        [SwaggerIgnore]
        public string? EmailAddress { get; set; }        

        public List<BPAddresses>? BPAddresses  { get; set; }

    }

    public class BPAddresses
    {
        [JsonProperty("AddressName")]
        [SwaggerIgnore]
        public string? Address { get; set; } = "FISCAL";
        [JsonProperty("Street")]
        public string? Street { get; set; }
        [JsonProperty("City")]
        public string? City { get; set; }
        [JsonProperty("County")]
        public string? County { get; set; }
        [JsonProperty("State")]
        public string? State { get; set; }
        [JsonProperty("Country")]
        public string? Country { get; set; }
         [JsonProperty("AddressType")]
        public string? AddressType { get; set; } = "bo_BillTo";

    }
}
