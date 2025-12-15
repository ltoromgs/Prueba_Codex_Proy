using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RusticaPortal_PRMVAN.Api.Entities.Login;
using RusticaPortal_PRMVAN.Api.Helpers;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using RestSharp;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Threading.Tasks;
using RusticaPortal_PRMVAN.Api.Entities.Dto;

namespace RusticaPortal_PRMVAN.Api.Services
{
    public class LoginService : ILoginService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmpresaConfigService _empresaConfigService;

        public LoginService(IMemoryCache cache, IEmpresaConfigService empresaConfigService)
        {
            _cache = cache;
            _empresaConfigService = empresaConfigService;
        }

        public async Task<string> Login(EmpresaConfig cfg)
        {

            // 2) Preparar claves de caché por empresa
            //    Puedes usar la del config (TokenSl) o una compuesta con el Id para evitar colisiones
            var cacheKey = string.IsNullOrWhiteSpace(cfg.Cache?.TokenSl)
                ? $"sl_token_{cfg.Id}"
                : $"{cfg.Cache.TokenSl}_{cfg.Id}";

            // 3) Si hay token cacheado, devolverlo
            if (_cache.TryGetValue<string>(cacheKey, out var tokenValue))
                return tokenValue;

            // 4) Preparar llamada al Service Layer de esa empresa
            //    (se mantiene HttpWebRequest para no cambiar tu desarrollo)
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

            var loginBody = new
            {
                CompanyDB = cfg.ServiceLayer.CompanyDB,
                UserName = cfg.ServiceLayer.UserName,
                Password = cfg.ServiceLayer.Password,
                Language = "23"
            };
            var data = JsonConvert.SerializeObject(loginBody);

            // Asegura que la ruta termine con "/"
            var baseUrl = (cfg.ServiceLayer.sl_route ?? string.Empty).Trim();
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            // Construye la URL de Login
            var loginUrl = $"{baseUrl}Login";

            var request = (HttpWebRequest)WebRequest.Create(loginUrl);
            request.ContentType = "application/json";
            request.Method = "POST";

            try
            {
                // 5) Enviar body
                using (var sw = new StreamWriter(request.GetRequestStream()))
                    sw.Write(data);

                // 6) Leer respuesta
                using (var sr = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    var result = await sr.ReadToEndAsync();
                    var session = JsonConvert.DeserializeObject<B1Session>(result);

                    if (session == null || string.IsNullOrWhiteSpace(session.SessionId))
                        return string.Empty;

                    // 7) Guardar en caché por los segundos definidos para esa empresa
                    var ttlSeconds = 900; // fallback
                    if (int.TryParse(cfg.Cache?.TimeSl, out var parsed))
                        ttlSeconds = parsed;

                    _cache.Set(cacheKey, session.SessionId, TimeSpan.FromSeconds(ttlSeconds));
                    return session.SessionId;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
