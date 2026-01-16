using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Web.Models
{
    public class GrupoPrmDto
    {
        public int? DocEntry { get; set; }
        public int? LineId { get; set; }
        public string U_MGS_CL_GRPCOD { get; set; }
        public string U_MGS_CL_GRPNOM { get; set; }
        public string U_MGS_CL_TIPGAS { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
    }

    public class PrmArticuloDto
    {
        public int? DocEntry { get; set; }
        public int? LineId { get; set; }
        public string U_MGS_CL_GRPCOD { get; set; }
        public string U_MGS_CL_ITEMCOD { get; set; }
        public string U_MGS_CL_ITEMNAM { get; set; }
        public string U_MGS_CL_TIPGAS { get; set; }
        public string U_MGS_CL_TIPMOP { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
    }

    public class GrupoPrmBulkRequest
    {
        public List<GrupoPrmDto> Items { get; set; } = new();
    }

    public class PrmArticuloBulkRequest
    {
        public List<PrmArticuloDto> Items { get; set; } = new();
    }

    public class GrupoPrmGuardarRequest
    {
        public List<GrupoPrmDto> Grupos { get; set; } = new();
        public List<PrmArticuloDto> Articulos { get; set; } = new();
        public string Grupo { get; set; }
    }

    public class GrupoPrmCopiarRequest
    {
        public string TiendaOrigen { get; set; }
        public string TiendaDestino { get; set; }
    }
}
