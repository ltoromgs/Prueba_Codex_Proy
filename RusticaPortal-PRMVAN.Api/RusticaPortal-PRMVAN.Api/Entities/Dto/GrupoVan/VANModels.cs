using System.Collections.Generic;
namespace RusticaPortal_PRMVAN.Api.Entities.Dto.GrupoVan
{
    public class VanTiendaDto
    {
        public string PrjCode { get; set; }
        public string PrjName { get; set; }
    }

    public class VanGrupoMaestroDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string U_MGS_CL_TIPO { get; set; }
        public decimal U_MGS_CL_PORC { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
        public string U_MGS_CL_FECPRO { get; set; }
        public string U_MGS_CL_PRIMARY { get; set; }
    }

    public class VanGrupoDetalleDto
    {
        public int? DocEntry { get; set; }
        public int LineId { get; set; }
        public string U_MGS_CL_GRPCOD { get; set; }
        public string U_MGS_CL_GRPNOM { get; set; }
        public string U_MGS_CL_TIPO { get; set; }
        public decimal U_MGS_CL_PORC { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
    }

    public class VanArticuloDetalleDto
    {
        public int? DocEntry { get; set; }
        public int LineId { get; set; }
        public string U_MGS_CL_ITEMCOD { get; set; }
        public string U_MGS_CL_ITEMNAM { get; set; }
        public string U_MGS_CL_TIPO { get; set; }
        public decimal U_MGS_CL_PORC { get; set; }
        public string U_MGS_CL_ACTIVO { get; set; }
    }

    public class GrupoVanBulkRequest
    {
        public List<VanGrupoDetalleDto> Items { get; set; } = new();
    }

    public class ArticuloVanBulkRequest
    {
        public List<VanArticuloDetalleDto> Items { get; set; } = new();
    }
}
