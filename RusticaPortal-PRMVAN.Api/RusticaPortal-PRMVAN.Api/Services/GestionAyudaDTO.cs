using Newtonsoft.Json;
using System.Collections.Generic;

public class GestionAyudaDTO
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

public class GestionAyudaDetalleUpdate
{
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
    public List<GestionAyudaDetalleUpdate> MGS_CL_GESDETCollection { get; set; } = new();
}

public class GestionAyudaDetalleCreate
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
    public List<GestionAyudaDetalleCreate> MGS_CL_GESDETCollection { get; set; } = new();
}

public class GestionAyudaTipoDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class GestionAyudaPeriodoDto
{
    public string U_MGS_CL_PERIODO { get; set; } = string.Empty;
}
