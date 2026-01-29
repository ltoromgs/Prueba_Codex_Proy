using Newtonsoft.Json;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Entities.Dto.PrevisionGastos
{
    public class PrevisionGastoTiendaDto
    {
        public string PrjCode { get; set; } = string.Empty;
        public string PrjName { get; set; } = string.Empty;
    }

    public class PrevisionGastoCatalogoDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class PrevisionGastoItemDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
    }

    public class PrevisionGastoDetalleDto
    {
        public string DocEntry { get; set; } = string.Empty;
        public string LineId { get; set; } = string.Empty;
        public string U_MGS_CL_TIENDA { get; set; } = string.Empty;
        public string U_MGS_CL_CONPRM { get; set; } = string.Empty;
        public string U_MGS_CL_TIPMOP { get; set; } = string.Empty;
        public string U_MGS_CL_ITEMCOD { get; set; } = string.Empty;
        public string U_MGS_CL_FECHA { get; set; } = string.Empty;
        public decimal U_MGS_CL_IMPORT { get; set; }
        public string U_MGS_CL_VALIDO { get; set; } = string.Empty;
    }

    public class PrevisionGastosSearchResponse
    {
        public string DocEntry { get; set; } = string.Empty;
        public List<PrevisionGastoDetalleDto> Items { get; set; } = new();
    }

    public class PrevisionGastoDetalleSave
    {
        public int? LineId { get; set; }
        public string U_MGS_CL_TIENDA { get; set; } = string.Empty;
        public string U_MGS_CL_CONPRM { get; set; } = string.Empty;
        public string U_MGS_CL_TIPMOP { get; set; } = string.Empty;
        public string U_MGS_CL_ITEMCOD { get; set; } = string.Empty;
        public string U_MGS_CL_FECHA { get; set; } = string.Empty;
        public decimal? U_MGS_CL_IMPORT { get; set; }
        public string U_MGS_CL_VALIDO { get; set; } = string.Empty;
    }

    public class PrevisionGastoSaveRequest
    {
        public string DocEntry { get; set; } = string.Empty;
        public string U_MGS_CL_PERIODO { get; set; } = string.Empty;

        [JsonProperty("MGS_CL_GASDETCollection")]
        public List<PrevisionGastoDetalleSave> MGS_CL_GASDETCollection { get; set; } = new();
    }
}
