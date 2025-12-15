using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Web.Models
{
    public class MatrizFactoresViewModel
    {
        public string PeriodoActual { get; set; } = "";
        public List<FactoresModel> Factores { get; set; } = new();
        //public OtroModelo OtraCosa { get; set; }
    }
    public class FactoresModel
    {
        public string U_MGS_CL_PERIODO { get; set; } = "";
        public string U_MGS_CL_TIENDA { get; set; } = "";
        public string U_MGS_CL_NOMTIE { get; set; } = "";
        public string DocEntry { get; set; } = "";
        public string LineId { get; set; } = "";

        // numéricos (Edm.Double)
        public decimal U_MGS_CL_META { get; set; }
        public decimal U_MGS_CL_RENTA { get; set; }
        public decimal U_MGS_CL_VAN { get; set; }
        public decimal U_MGS_CL_ESTPER { get; set; }
        public decimal U_MGS_CL_GASADM { get; set; }
        public decimal U_MGS_CL_COMTAR { get; set; }
        public decimal U_MGS_CL_IMPUES { get; set; }
        public decimal U_MGS_CL_REGALI { get; set; }
        public decimal U_MGS_CL_AUSER { get; set; }
        public decimal U_MGS_CL_AUOPE { get; set; }
        public decimal U_MGS_CL_AUCCC { get; set; }
        public decimal U_MGS_CL_AUADH { get; set; }
        public decimal U_MGS_CL_CLIMA { get; set; }
        public decimal U_MGS_CL_RUSTI { get; set; }
        public decimal U_MGS_CL_MELID { get; set; }
        public decimal U_MGS_CL_ADMGR { get; set; }
        public decimal U_MGS_CL_EXGES { get; set; }
        public decimal U_MGS_CL_EXSER { get; set; }
        public decimal U_MGS_CL_EXMAR { get; set; }

        public string U_MGS_CL_PRIMARY { get; set; } = "";


    }

    public class FactorUpdateRequest
    {
        [JsonProperty("MGS_CL_FACDETCollection")]
        public List<FactoresModel> MGS_CL_FACDETCollection { get; set; } = new();
    }
}
