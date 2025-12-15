using System;

namespace RusticaPortal_PRMVAN.Api.Entities.ObjectSAP
{
    public class FechaRecepcionDTO
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Mes { get; set; }
        public string DiaSemanaNumero { get; set; }
    }
}
