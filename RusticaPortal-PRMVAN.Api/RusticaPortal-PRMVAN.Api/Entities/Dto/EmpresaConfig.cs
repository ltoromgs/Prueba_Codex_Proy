namespace RusticaPortal_PRMVAN.Api.Entities.Dto
{
    public class EmpresaConfig
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ConnectionString { get; set; }
        public ServiceLayerConfig ServiceLayer { get; set; }
        public CacheConfig Cache { get; set; }

    }

    public class ServiceLayerConfig
    {
        public string sl_route { get; set; }
        public string sl_value { get; set; }
        public string CompanyDB { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string ServerLicencia { get; set; }

    }
    public class CacheConfig
    {
        public string TokenSl { get; set; }
        public string TimeSl { get; set; }
    }
}
