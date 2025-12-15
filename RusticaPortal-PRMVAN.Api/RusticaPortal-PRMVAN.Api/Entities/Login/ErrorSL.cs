namespace RusticaPortal_PRMVAN.Api.Entities.Login
{
    public class Message
    {
        public string lang { get; set; }
        public string value { get; set; }
    }

    public class Error
    {
        public int code { get; set; }
        public object message { get; set; }
    }
    public class ErrorSL
    {
        public Error error { get; set; }
    }
    
}
