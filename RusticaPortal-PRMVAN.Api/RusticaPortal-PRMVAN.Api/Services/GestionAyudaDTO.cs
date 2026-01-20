using Newtonsoft.Json;
using System.Collections.Generic;

public class GestionAyudaCabDto
{
    public string DocEntry { get; set; } = string.Empty;
    public string U_MGS_CL_PERIODO { get; set; } = string.Empty;
}

public class GestionAyudaDetalleDto
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

public class GestionAyudaDetalleUpdateDto
{
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
    public List<GestionAyudaDetalleUpdateDto> MGS_CL_GESDETCollection { get; set; } = new();
}

public class GestionAyudaDetalleCreateDto
{
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

public class GestionAyudaCreateRequest
{
    public string U_MGS_CL_PERIODO { get; set; } = string.Empty;

    [JsonProperty("MGS_CL_GESDETCollection")]
    public List<GestionAyudaDetalleCreateDto> MGS_CL_GESDETCollection { get; set; } = new();
}

public class GestionAyudaTipoDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string U_MGS_CL_ACTIVO { get; set; } = string.Empty;
}

public class GestionAyudaTiendaDto
{
    public string PrjCode { get; set; } = string.Empty;
    public string PrjName { get; set; } = string.Empty;
}
