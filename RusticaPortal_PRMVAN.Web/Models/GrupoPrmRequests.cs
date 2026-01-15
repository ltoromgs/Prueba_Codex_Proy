using System.Collections.Generic;

namespace RusticaPortal_PRMVAN.Web.Models
{
    public class GrupoPrmGuardarRequest
    {
        public List<object> Grupos { get; set; } = new();
        public List<object> Articulos { get; set; } = new();
        public string Grupo { get; set; }
    }

    public class GrupoPrmCopiarRequest
    {
        public string TiendaOrigen { get; set; }
        public string TiendaDestino { get; set; }
    }
}
