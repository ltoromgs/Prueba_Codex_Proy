namespace ArellanoCore.Api.Entities.Information
{
    public class ResponseInformation
    {
        public bool Registered { get; set; }
        public string Message { get; set; }
        public string Content { get; set; }

        public ResponseInformation()
        {
            Registered = true;
            Message = "";
            Content = "";
        }
    }
}
