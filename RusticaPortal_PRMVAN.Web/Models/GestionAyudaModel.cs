using Newtonsoft.Json;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Web.Models
{
    public class GestionAyudaViewModel
    {
        public string PeriodoActual { get; set; } = "";
        public List<GestionAyudaModel> Detalle { get; set; } = new();
    }

    public class GestionAyudaModel
    {
        public string U_MGS_CL_PERIODO { get; set; } = "";
        public string U_MGS_CL_PERIODO_DEST { get; set; } = "";
        public string U_MGS_CL_TIENDA { get; set; } = "";
        public string U_MGS_CL_NOMTIE { get; set; } = "";
        public string DocEntry { get; set; } = "";
        public string LineId { get; set; } = "";
        public string U_MGS_CL_CVENTA { get; set; } = "";
        public string U_MGS_CL_CRENTA { get; set; } = "";
        public string U_MGS_CL_CVAN { get; set; } = "";
        public string U_MGS_CL_CPERSO { get; set; } = "";
        public string U_MGS_CL_CGESTI { get; set; } = "";
        public string U_MGS_CL_CSERV { get; set; } = "";
        public string U_MGS_CL_CCC { get; set; } = "";
        public string U_MGS_CL_CADM { get; set; } = "";
    }

    public class GestionAyudaUpdateRequest
    {
        [JsonProperty("MGS_CL_GESDETCollection")]
        public List<GestionAyudaModel> MGS_CL_GESDETCollection { get; set; } = new();
    }

    public class GestionAyudaCreateRequest
    {
        public string U_MGS_CL_PERIODO { get; set; } = string.Empty;

        [JsonProperty("MGS_CL_GESDETCollection")]
        public List<GestionAyudaModel> MGS_CL_GESDETCollection { get; set; } = new();
    }

    public class GestionAyudaTipoModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
