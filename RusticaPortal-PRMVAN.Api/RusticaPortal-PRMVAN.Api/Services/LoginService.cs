using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ArellanoCore.Api.Entities.Login;
using ArellanoCore.Api.Helpers;
using ArellanoCore.Api.Services.Interfaces;
using RestSharp;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Threading.Tasks;

namespace ArellanoCore.Api.Services
{
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;
        public LoginService(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<string> Login(string BaseDatos)
        {
            B1Session obj = null;
            string rs = "";
            string token = "";
            
            if (BaseDatos == "1")
            {
                token = _configuration["Cache:TokenSl"].ToString();
            }
            else if (BaseDatos == "2")
            {
                token = _configuration["Cache2:TokenSl2"].ToString();
            }
            try
            {
                if (_cache.TryGetValue<string>(token, out var tokenValue))
                {

                    rs = tokenValue;
                }
                else
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;
                    
                    string data = "";
                    HttpWebRequest httpWebRequest = null;

                    if (BaseDatos == "1") {
                        data = "{    \"CompanyDB\": \"" + _configuration["ServiceLayer:CompanyDB"].ToString() + "\",  \"UserName\": \"" + _configuration["ServiceLayer:UserName"].ToString() + "\", \"Password\": \"" + _configuration["ServiceLayer:Password"].ToString() + "\", \"Language\":\"23\"}";
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(_configuration["ServiceLayer:sl_route"].ToString() + "Login");

                    }
                    else if (BaseDatos == "2")
                    {
                        data = "{    \"CompanyDB\": \"" + _configuration["ServiceLayer2:CompanyDB"].ToString() + "\",  \"UserName\": \"" + _configuration["ServiceLayer2:UserName"].ToString() + "\", \"Password\": \"" + _configuration["ServiceLayer2:Password"].ToString() + "\", \"Language\":\"23\"}";
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(_configuration["ServiceLayer2:sl_route"].ToString() + "Login");
                    }


                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    { streamWriter.Write(data); }

                    using (var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();
                        obj = JsonConvert.DeserializeObject<B1Session>(result);
                        if (BaseDatos == "1")
                        {
                            _cache.Set(token, obj.SessionId.ToString(), TimeSpan.FromSeconds(int.Parse(_configuration["Cache:TimeSl"].ToString())));
                        }
                        else if (BaseDatos == "2")
                        {
                            _cache.Set(token, obj.SessionId.ToString(), TimeSpan.FromSeconds(int.Parse(_configuration["Cache2:TimeSl2"].ToString())));
                        }
                        
                        rs = obj.SessionId.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                rs = "";
            }

            return rs;
        }
    }
}
