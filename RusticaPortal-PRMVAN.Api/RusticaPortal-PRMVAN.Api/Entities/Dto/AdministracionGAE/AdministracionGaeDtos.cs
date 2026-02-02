using System;
using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Api.Entities.Dto.AdministracionGAE
{
    public class AdministracionGaeTiendaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AdministracionGaeCatalogoDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class AdministracionGaeDetalleDto
    {
        public string IdEmpresa { get; set; } = string.Empty;
        public string NombreEmpresa { get; set; } = string.Empty;
        public string BaseDatos { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public string DocEntry { get; set; } = string.Empty;
        public string LineId { get; set; } = string.Empty;
        public string NumAtCard { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
        public string Tienda { get; set; } = string.Empty;
        public string U_MGS_CL_TIPGAE { get; set; } = string.Empty;
        public string U_MGS_CL_AUTORI { get; set; } = string.Empty;
        public string U_MGS_CL_TIPGAS { get; set; } = string.Empty;
        public string U_MGS_CL_TIPMOP { get; set; } = string.Empty;
        public decimal U_MGS_CL_IMPORT { get; set; }
        public string U_MGS_CL_FEPRM { get; set; } = string.Empty;
        public string U_MGS_CL_SOLICI { get; set; } = string.Empty;
        public string U_MGS_CL_VALIDO { get; set; } = string.Empty;
        public string Pendiente { get; set; } = string.Empty;
        public string MensajeError { get; set; } = string.Empty;
    }

    public class AdministracionGaeUpdateRequest
    {
        public List<AdministracionGaeUpdateLine> Items { get; set; } = new();
    }

    public class AdministracionGaeUpdateLine
    {
        public string BaseDatos { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public string DocEntry { get; set; } = string.Empty;
        public string LineId { get; set; } = string.Empty;
        public string U_MGS_CL_TIPGAE { get; set; } = string.Empty;
        public string U_MGS_CL_AUTORI { get; set; } = string.Empty;
        public string U_MGS_CL_TIPGAS { get; set; } = string.Empty;
        public string U_MGS_CL_TIPMOP { get; set; } = string.Empty;
        public decimal? U_MGS_CL_IMPORT { get; set; }
        public string U_MGS_CL_FEPRM { get; set; } = string.Empty;
        public string U_MGS_CL_VALIDO { get; set; } = string.Empty;
    }

    public class AdministracionGaeUpdateResult
    {
        public string BaseDatos { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public string DocEntry { get; set; } = string.Empty;
        public string LineId { get; set; } = string.Empty;
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
