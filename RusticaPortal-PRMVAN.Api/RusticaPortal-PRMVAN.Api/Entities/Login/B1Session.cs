namespace ArellanoCore.Api.Entities.Login
{
    public class B1Session
    {
        public string OdataMetadata { get; set; }
        public string SessionId { get; set; }
        public string Version { get; set; }
        public int SessionTimeout { get; set; }
    }

    public class LoginModelSL
    {
        public string CompanyDB { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
    }
}
