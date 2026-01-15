using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Entities.Dto.GrupoPrm
{
    public class PrmTiendaDto
    {
        public string PrjCode { get; set; }
        public string PrjName { get; set; }
    }

    public class PrmGrupoMaestroDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PrmTipoDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PrmItemMaestroDto
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
    }

    public class PrmGrupoDetalleDto
    {
        public int? DocEntry { get; set; }
        public int LineId { get; set; }
        public string U_MGS_CL_GRPCOD { get; set; }
        public string U_MGS_CL_GRPNOM { get; set; }
        public string MGS_CL_TIPGAS { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
    }

    public class PrmArticuloDetalleDto
    {
        public int? DocEntry { get; set; }
        public int LineId { get; set; }
        public string U_MGS_CL_GRPCOD { get; set; }
        public string U_MGS_CL_ITEMCOD { get; set; }
        public string U_MGS_CL_ITEMNAM { get; set; }
        public string MGS_CL_TIPGAS { get; set; }
        public string MGS_CL_TIPMOP { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
    }

    public class GrupoPrmBulkRequest
    {
        public List<PrmGrupoDetalleDto> Items { get; set; } = new();
    }

    public class ArticuloPrmBulkRequest
    {
        public List<PrmArticuloDetalleDto> Items { get; set; } = new();
    }

    public class CopiarPrmRequest
    {
        public string TiendaOrigen { get; set; }
        public string TiendaDestino { get; set; }
    }
}
