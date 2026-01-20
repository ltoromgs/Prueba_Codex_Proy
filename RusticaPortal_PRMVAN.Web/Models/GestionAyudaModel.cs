using Newtonsoft.Json;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Web.Models
{
    public class GestionAyudaViewModel
    {
        public string PeriodoActual { get; set; } = string.Empty;
    }

    public class GestionAyudaDetalleModel
    {
        public string DocEntry { get; set; } = string.Empty;
        public string LineId { get; set; } = string.Empty;
        public string U_MGS_CL_TIENDA { get; set; } = string.Empty;
        public string U_MGS_CL_NOMTIE { get; set; } = string.Empty;
        public string U_MGS_CL_CVENTA { get; set; } = string.Empty;
        public string U_MGS_CL_CRENTA { get; set; } = string.Empty;
        public string U_MGS_CL_CVAN { get; set; } = string.Empty;
        public string U_MGS_CL_CPERSO { get; set; } = string.Empty;
        public string U_MGS_CL_CGESTI { get; set; } = string.Empty;
        public string U_MGS_CL_CSERV { get; set; } = string.Empty;
        public string U_MGS_CL_CCC { get; set; } = string.Empty;
        public string U_MGS_CL_CADM { get; set; } = string.Empty;
    }

    public class GestionAyudaUpdateRequest
    {
        [JsonProperty("MGS_CL_GESDETCollection")]
        public List<GestionAyudaDetalleModel> MGS_CL_GESDETCollection { get; set; } = new();
    }

    public class GestionAyudaTipoModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string U_MGS_CL_ACTIVO { get; set; } = string.Empty;
    }

    public class GestionAyudaTiendaModel
    {
        public string PrjCode { get; set; } = string.Empty;
        public string PrjName { get; set; } = string.Empty;
    }
}
