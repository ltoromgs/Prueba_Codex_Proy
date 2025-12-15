using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;

namespace RusticaPortal_PRMVAN.Api.Helpers
{

    public class ApiHelper
    {
        public const string LOGIN = "/b1s/v2/Login";
        public const string ORDER = "/b1s/v2/Orders";
        public const string BP = "/b1s/v2/BusinessPartners";
        public const string INCOMING_PAYMENTS = "/b1s/v2/IncomingPayments";
        public const string FACTURA = "/b1s/v2/Invoices";
        public const string SETTC = "/b1s/v2/SBOBobService_SetCurrencyRate";
        public const string TKENV = "/b1s/v2/NX_AFIP_TKEN";

        public static T Request<T>(
            string ipSL,
            string rutaServicio,
            Method method,
            HttpStatusCode statusCode,
            out object error,
            dynamic body = null,
            string token = null,
            int? nResultados = null
            ) where T : class
        {
            string osessionID = string.Empty;
            string orouteID = string.Empty;

            return Request<T>
                (ipSL,
                rutaServicio,
                method,
                statusCode,
                ref osessionID,
                ref orouteID,
                out error,
                body,
                token,
                nResultados
               );
        }
        public static T Request<T>(
            string ipSL,
            string rutaServicio,
            Method method,
            HttpStatusCode statusCode,
            ref string sessionID,
            ref string routeID,
            out object error,
            string body = "",
            string token = null,
            int? nResultados = null
            ) where T : class
        {

            T entity = null;
            error = null;
            try
            {


                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                var client = new RestClient(ipSL);
                if (!ipSL.Contains("50000"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                }
                else
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 ;
                }
                var request = new RestRequest(rutaServicio, method);
                request.AddHeader("Content-Type", "application/json");

                if (!String.IsNullOrEmpty(token))
                    request.AddHeader("Authorization", String.Format("Bearer {0}", token));

                if (nResultados.HasValue)
                    request.AddHeader("Prefer", String.Format("odata.maxpagesize={0}", nResultados.Value));

                //if (!String.IsNullOrEmpty(sessionID))
                //{
                //    request.AddCookie("B1SESSION", sessionID);
                //}
                    
                //if (!String.IsNullOrEmpty(routeID))
                //    request.AddCookie("ROUTEID", routeID);

                if (body != null)
                {
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    //request.AddParameter("application/json", JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), ParameterType.RequestBody);

                }


                var response = client.Execute(request);

                if (response.ErrorException != null)
                    throw response.ErrorException;

                //log.DebugFormat("Content: {0}", response?.Content);

                if (response.StatusCode != HttpStatusCode.BadGateway)
                {
                    if (response.StatusCode == statusCode)
                    {
                        if (request.Resource.Contains(LOGIN))
                        {
                            sessionID = response.Cookies[0].Value.ToString();
                            if (response.Cookies.Count > 2)
                            {
                                routeID = response.Cookies[2].Value.ToString();
                            }
                            else
                            {
                                routeID = response.Cookies[1].Value.ToString();
                            }
                        }
                        entity = JsonConvert.DeserializeObject<T>(response.Content);
                    }

                    else
                        error = response.Content; // JsonConvert.DeserializeObject<ErrorModelSL>(response.Content);
                }

            }
            catch (Exception ex)
            {

                throw ex;

            }

            return entity;
        }
    }


    
}
