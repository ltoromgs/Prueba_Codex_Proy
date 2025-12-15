using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Swagger;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class JournalEntriesAddDTO
    {      

         [JsonProperty("ReferenceDate")]
         public string? FechadeContabilizacion  { get; set; }

         [JsonProperty("Memo")]
        public string? Memo { get; set; }

         [JsonProperty("Reference")]
        public string? Ref1 { get; set; }

         [JsonProperty("Reference2")]
        public string? Ref2 { get; set; }

        [Required]
        [JsonProperty("TransactionCode")]
        public string? TransCode { get; set; }

         [JsonProperty("TaxDate")]
         public string? FechadeDocumento  { get; set; }

         [JsonProperty("VatDate")]
         public string? FechadeVencimiento  { get; set; }

         [JsonProperty("DueDate")]
         public string? DueDate { get; set; }    

        [JsonProperty("JournalEntryLines")]
        public List<JournalEntryLine> JournalEntryLines { get; set; }
    }

    public class JournalEntryLine
    {
        [JsonProperty("Line_ID")]
        public int? LineNum  { get; set; }

        [JsonProperty("AccountCode")]
        public string? Account { get; set; }

        [JsonProperty("ShortName")]
        public string? ShortName { get; set; }
        [JsonProperty("Debit")]
        public double? Debit { get; set; }
        [JsonProperty("Credit")]
        public double? Credit { get; set; }

        [JsonProperty("FCDebit")]
        public double? FCDebit { get; set; }
        [JsonProperty("FCCredit")]
        public double? FCCredit { get; set; }
        [JsonProperty("FCCurrency")]
        public string? FCCurrency { get; set; }
        [JsonProperty("LineMemo")]
        public string? LineMemo { get; set; }
        [JsonProperty("Reference1")]
        public string? Ref1 { get; set; }
        [JsonProperty("Reference2")]
        public string? Ref2 { get; set; }
        [JsonProperty("ProjectCode")]
        public string? Project { get; set; }
        [JsonProperty("CostingCode")]
        public string? ProfitCode { get; set; }
    }
}

