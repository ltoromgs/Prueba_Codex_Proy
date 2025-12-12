namespace RusticaPortal_PRMVAN.Web.Models
{
    // Modelo del menú
    public class MenuModel
    {
        public System.Collections.Generic.List<MenuItem> Menu { get; set; }
    }

    public class MenuItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public System.Collections.Generic.List<MenuItem> Hijos { get; set; }
    }
}
