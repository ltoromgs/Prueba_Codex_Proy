using Newtonsoft.Json;
using RusticaPortal_PRMVAN.Api.Entities.Information;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using System.Net;
using RusticaPortal_PRMVAN.Api.Entities.Login;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using System.Data;
using Sap.Data.Hana;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.Dto.GrupoVan;
using System.Globalization;

namespace RusticaPortal_PRMVAN.Api.Services
{



    public class DocumentService : IDocumentService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmpresaConfigService _empresaConfigService;
        private readonly ILoginService _loginService;

        public DocumentService(
            IConfiguration configuration,
            IEmpresaConfigService empresaConfigService,
            ILoginService loginService)
        {
            _configuration = configuration;
            _empresaConfigService = empresaConfigService;
            _loginService = loginService;
        }

        public async Task<ResponseInformation> GetGrupoVanTipos(string empresa)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return error;
            }

            var tipos = new List<VanTipoDto>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanTipo";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    tipos.Add(new VanTipoDto
                    {
                        Code = reader[nameof(VanTipoDto.Code)]?.ToString() ?? string.Empty,
                        Name = reader[nameof(VanTipoDto.Name)]?.ToString() ?? string.Empty
                    });
                }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(tipos)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error al cargar tipos VAN.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<AditionalInfomation> GetClienteMoneda(string project, string BaseDatos)
        {
            string HANAConnectionString = "";
            HANAConnectionString = BaseDatos == "1" ? _configuration["ConnectionStringsSAP"] : _configuration["ConnectionStringsSAP2"];
            HanaConnection conn = new HanaConnection(HANAConnectionString);
            AditionalInfomation ai = new AditionalInfomation();
            try
            {

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();

                    using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_ClienteMon";
                    cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = project ?? string.Empty;
                    cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ai.CardCode = reader.GetString(reader.GetOrdinal("CardCode"));
                        ai.CurCode = reader.GetString(reader.GetOrdinal("CurCode"));
                        ai.Proyecto_Nombre = reader.GetString(reader.GetOrdinal("Proyecto_Nombre"));
                        ai.Activo = reader.GetString(reader.GetOrdinal("Activo"));
                        ai.Estado = reader.GetString(reader.GetOrdinal("Estado"));
                        ai.Moneda = reader.GetString(reader.GetOrdinal("Moneda"));

                    }

                    reader.Close();
                    conn.Close();
                }
                

            }
            catch (Exception ex)
            {
                if (conn.State.Equals(ConnectionState.Open))
                    conn.Close();
                //result = null;
                //throw;
            }
            return ai;
        }

        public async Task<string> GetOVByProject(string project, bool esOV, string BaseDatos)
        {
            string result = "";
            string HANAConnectionString = "";
            HANAConnectionString = BaseDatos;

            HanaConnection conn = new HanaConnection(HANAConnectionString);
            AditionalInfomation ai = new AditionalInfomation();
            try
            {

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();

                    using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_OVByPrj";
                    cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = project ?? string.Empty;
                    cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = esOV ? "Y" : "N";
                    cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result = reader.GetString(reader.GetOrdinal("DocEntry"));

                    }

                    reader.Close();
                    conn.Close();
                }


            }
            catch (Exception ex)
            {
                if (conn.State.Equals(ConnectionState.Open))
                    conn.Close();
                result = "";
                //throw;
            }
            return result;
        }
        public async Task<List<string>> GetOVByContract(string contract, string BaseDatos)
        {
            List<string> result = new List<string>();
            string HANAConnectionString = "";
            HANAConnectionString = BaseDatos == "1" ? _configuration["ConnectionStringsSAP"] : _configuration["ConnectionStringsSAP2"];

            HanaConnection conn = new HanaConnection(HANAConnectionString);
            AditionalInfomation ai = new AditionalInfomation();
            try
            {

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();

                    using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_OVByCnt";
                    cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = contract ?? string.Empty;
                    cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result.Add(reader.GetString(reader.GetOrdinal("DocEntry")));
                    }

                    reader.Close();
                    conn.Close();
                }


            }
            catch (Exception ex)
            {
                if (conn.State.Equals(ConnectionState.Open))
                    conn.Close();
                result = null;
                //throw;
            }
            return result;
        }



        public async Task<ResponseInformation> PostInfo(RequestInformation requestInformation, string nomSistem, EmpresaConfig BaseDatos)
        {
            OrderAddDTO asd = new OrderAddDTO();
            HttpWebRequest httpWebGetRequest = null;

            ResponseInformation ri = new ResponseInformation();
            try
            {
                string route = "";

                //ltoro 02/10/2025
                route = BaseDatos.ServiceLayer.sl_route + requestInformation.Route;

                /*if (BaseDatos == "1") {
                    route = _configuration["ServiceLayer:sl_route"].ToString() + requestInformation.Route;
                }
                else
                {
                    route = _configuration["ServiceLayer2:sl_route"].ToString() + requestInformation.Route;
                }*/


                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

                httpWebGetRequest = (HttpWebRequest)WebRequest.Create(route);



                httpWebGetRequest.ContentType = "application/json";
                httpWebGetRequest.Method = "POST";
                CookieContainer cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", requestInformation.Token.ToString()) { Domain = BaseDatos.ServiceLayer.sl_value.ToString() });
                //cookies.Add(new Cookie("ROUTEID", ".node1") { Domain = _configuration["ServiceLayer:ip_value"].ToString() });
                httpWebGetRequest.CookieContainer = cookies;



                if (requestInformation.Route == "Projects")
                {
                    dynamic json = JsonConvert.DeserializeObject(requestInformation.Doc);
                    var ruc = Convert.ToString(json.U_MGS_CL_RUCPRO);
                    ri = await ValidaRUC(BaseDatos.ConnectionString.ToString(), ruc, requestInformation);
                    if (!ri.Registered)
                    {
                        return ri;
                    }
                }

                using (var streamWriter = new StreamWriter(httpWebGetRequest.GetRequestStream()))
                { streamWriter.Write(requestInformation.Doc); }

                using (var streamReader = new StreamReader(httpWebGetRequest.GetResponse().GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    ri.Content = result;
                    requestInformation.CodGenerado = await ObtenerID(result, requestInformation);
                    ri.Registered = true;
                    ri.Message = "Se registro con exito";
                }
            }
            catch (WebException ex)
            {
                ErrorSL errorMessage = null;
                var obj = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                errorMessage = JsonConvert.DeserializeObject<ErrorSL>(obj);
                ri.Content = "";
                ri.Registered = false;
                ri.Message = errorMessage.error.message.ToString();

                if (requestInformation.Route == "JournalEntries")
                {
                    string searchTerm = "Código de cuenta no válido";
                    bool found = ContainsString(ri.Message, searchTerm);
                    if (found)
                    {
                        JournalEntriesAddDTO JournalEntries = JsonConvert.DeserializeObject<JournalEntriesAddDTO>(requestInformation.Doc);
                        // Comprueba si JournalEntryLines no es null y tiene elementos
                        if (JournalEntries.JournalEntryLines != null && JournalEntries.JournalEntryLines.Count > 0)
                        {
                            int j = 0;
                            while (j < JournalEntries.JournalEntryLines.Count)
                            {
                                ri = await ValidaRUC(BaseDatos.ConnectionString, JournalEntries.JournalEntryLines[j].Account.ToString(), requestInformation);
                                if (!ri.Registered)
                                {
                                    return ri;
                                }

                                j++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ri.Content = "";
                ri.Registered = false;
                ri.Message = "Ocurrio error al registrar ";
            }
            finally
            {
                AditionalInfomation ai = await InserLog(ri, requestInformation, httpWebGetRequest, nomSistem, BaseDatos);
            }


            return ri;


        }


        public static bool ContainsString(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            {
                return false;
            }

            return text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public async Task<string> ObtenerID(string MessageJson, RequestInformation tipo)
        {
            string id = "";

            try
            {
                dynamic json = JsonConvert.DeserializeObject(MessageJson);
                

                switch (tipo.Route)
                {
                    case "JournalEntries":
                        id = Convert.ToString(json.JdtNum);
                        break;
                    case "BusinessPartners":
                        id = Convert.ToString(json.CardCode);
                        break;
                    case "MD":
                        id = Convert.ToString(json.TableName);
                        break;
                    case "CC":
                        id = Convert.ToString(json.Name);
                        break;
                    case "UDO":
                        id = Convert.ToString(json.DocEntry);
                        break;
                     case "Projects":
                        id = Convert.ToString(json.Code);
                        break;
                    default: //doc //Order
                        id = Convert.ToString(json.DocNum);
                        break;

                }

                return id;
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<ResponseInformation> ValidaDatos(string empresaId)
        {
            var rp = new ResponseInformation();
            if (!int.TryParse(empresaId, out var id))
            {
                rp.Registered = false;
                rp.Message = "Error: Parámetro 'Empresa' debe ser un número válido.";
                return rp;
            }

            var cfg = _empresaConfigService.GetEmpresa(id);
            if (cfg == null)
            {
                rp.Registered = false;
                rp.Message = $"Error: No existe configuración para la empresa con ID = {id}.";
            }
            else
            {
                rp.Registered = true;
            }
            return rp;
        }
        /*public async Task<ResponseInformation> ValidaDatos(string BaseDatos)
        {
            
            ResponseInformation rp = new ResponseInformation();
            try
            {

                if (BaseDatos != "1" && BaseDatos != "2")
                {
                    
                    throw new Exception();
                }
                else
                {
                    rp.Registered = true;
                }
            }          
            catch (Exception ex)
            {
                rp.Content = "";
                rp.Message = "Error: Parámetro 'Empresa' dede ser 1 ó 2 (1. Arellano / 2.TRADING)";
                rp.Registered = false;
            }

            return rp;


        }*/
        public async Task<ResponseInformation> ValidaRUC(string BaseDatos, string ruc, RequestInformation requestInformation)
        {
            string result = "";
            string HANAConnectionString = "";
            HANAConnectionString = BaseDatos == "1" ? _configuration["ConnectionStringsSAP"] : _configuration["ConnectionStringsSAP2"];
            HanaConnection conn = new HanaConnection(HANAConnectionString);
            ResponseInformation rp = new ResponseInformation();
            try
            {

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();

                    using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_ValidaRuc";
                    cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = ruc ?? string.Empty;
                    cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = requestInformation.Route ?? string.Empty;
                    cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result = reader.GetString(reader.GetOrdinal("CardCode"));

                    }

                    if (result == "") throw new Exception();
                    else rp.Registered = true;


                    reader.Close();
                    conn.Close();
                }


            }
            catch (Exception ex)
            {
                if (conn.State.Equals(ConnectionState.Open))
                    conn.Close();
                rp.Content = "";
                if (requestInformation.Route == "Projects")
                {
                    rp.Message = "Error: Ruc '"+ ruc + "' no existe en SAP";
                }
                else if (requestInformation.Route == "JournalEntries")
                {
                    rp.Message = "Error: Cuenta '"+ ruc + "' no existe en SAP";
                }
               
                rp.Registered = false;

               
            }            

            return rp;


        }

        async Task<AditionalInfomation> InserLog(ResponseInformation ri, RequestInformation requestInformation, HttpWebRequest httpWebGetRequest, string nomSistem, EmpresaConfig BaseDatos)
        {
            string result = "";
            string estado = "E";
            string usuario = "";
            usuario = BaseDatos.ServiceLayer.UserName.ToString(); ;
            //usuario = BaseDatos == "1" ?  _configuration["ServiceLayer:UserName"].ToString() : _configuration["ServiceLayer2:UserName"].ToString();
            string HANAConnectionString = "";
            HANAConnectionString = BaseDatos.ConnectionString.ToString();
            HanaConnection conn = new HanaConnection(HANAConnectionString);
            AditionalInfomation ai = new AditionalInfomation();
            try
            {

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();

                    bool method1 = true;

                    if (ri.Registered) estado = "R";
                    DateTime fecha = DateTime.Now;

                    // Formatear la fecha con el formato año-mes-día
                    string fechaFormateada = fecha.ToString("yyyyMMdd");

                    // Formatear la hora con el formato hora:minuto:segundo
                    string horaFormateada = fecha.ToString("HH:mm");

                    if (httpWebGetRequest.Method != "DELETE")
                        ri.Message = estado == "R" ? ri.Message.Replace("'", "") + " con el codigo: " + requestInformation.CodGenerado : ri.Message.Replace("'", "");

                    string query = " INSERT INTO \"@MGS_CL_WEBLOG\"(                                            ";
                    query += "       \"Code\", \"Name\", \"U_DocEntry\",                                      ";
                    query += "       \"U_MGS_CL_USUARIO\",                                                  ";
                    query += "       \"U_MGS_CL_FECHA\",                                                    ";
                    query += "       \"U_MGS_CL_HORA\",                                                     ";
                    query += "       \"U_MGS_CL_JSON\",                                                     ";
                    query += "       \"U_MGS_CL_ESTADO\",                                                   ";
                    query += "       \"U_MGS_CL_URL\",                                                      ";
                    query += "       \"U_MGS_CL_METHOD\",                                                   ";
                    query += "       \"U_MGS_CL_SISTEM\",                                                   ";
                    query += "       \"U_MGS_CL_MENSAJ\")                                                   ";
                    query += "   SELECT                                                                     ";
                    query += "       CAST((IFNULL(MAX(\"U_DocEntry\"),0) + 1) AS VARCHAR),                                  ";
                    query += "       CAST((IFNULL(MAX(\"U_DocEntry\"),0) + 1) AS VARCHAR),                                  ";
                    query += "       IFNULL(MAX(\"U_DocEntry\"),0) + 1,                                                     ";
                    query += "       '" + usuario + "',    ";
                    query += "       '" + fechaFormateada + "',     ";
                    query += "       '" + horaFormateada + "',     ";
                    query += "       '" + requestInformation.Doc + "',     ";
                    query += "       '" + estado + "',   ";
                    query += "       '" + httpWebGetRequest.Address + "',      ";
                    query += "       '" + httpWebGetRequest.Method + "',   ";
                    query += "       '" + nomSistem + "',  ";
                    query += "       '" + ri.Message + "' ";
                    query += " FROM \"@MGS_CL_WEBLOG\"";

                    HanaCommand cmd = new HanaCommand(query, conn);
                    HanaDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                    }

                    reader.Close();
                    conn.Close();
                }


            }
            catch (Exception ex)
            {
                if (conn.State.Equals(ConnectionState.Open))
                    conn.Close();
                result = null;
                //throw;
            }
            return ai;
        }
        public async Task<ResponseInformation> UpdateInfo(RequestInformation requestInformation, string nomSistem, EmpresaConfig BaseDatos)
        {
            ResponseInformation ri = new ResponseInformation();
            HttpWebRequest httpWebGetRequest = null;

            try
            {

                string route = BaseDatos.ServiceLayer.sl_route + requestInformation.Route;

                /*if (BaseDatos == "1")
                {
                    route = _configuration["ServiceLayer:sl_route"].ToString() + requestInformation.Route;
                }
                else
                {
                    route = _configuration["ServiceLayer2:sl_route"].ToString() + requestInformation.Route;
                }*/

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

                httpWebGetRequest = (HttpWebRequest)WebRequest.Create(route);
                httpWebGetRequest.ContentType = "application/json";
                httpWebGetRequest.Method = "PATCH";
                CookieContainer cookies = new CookieContainer();

                cookies.Add(new Cookie("B1SESSION", requestInformation.Token.ToString()) { Domain = BaseDatos.ServiceLayer.sl_value.ToString() });
                //  cookies.Add(new Cookie("B1SESSION", requestInformation.Token.ToString()) { Domain = BaseDatos == "1" ? _configuration["ServiceLayer:sl_value"].ToString() : _configuration["ServiceLayer2:sl_value"].ToString() });
                //cookies.Add(new Cookie("ROUTEID", ".node1") { Domain = _configuration["ServiceLayer:ip_value"].ToString() });
                httpWebGetRequest.CookieContainer = cookies;

                using (var streamWriter = new StreamWriter(httpWebGetRequest.GetRequestStream()))
                { streamWriter.Write(requestInformation.Doc); }

                using (var streamReader = new StreamReader(httpWebGetRequest.GetResponse().GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    // documentResult = JsonConvert.DeserializeObject<DeliveryRpta>(result);
                    ri.Registered = true;
                    ri.Message = "Se actualizó con exito";
                }
            }
            catch (WebException ex)
            {
                ErrorSL errorMessage = null;
                errorMessage = JsonConvert.DeserializeObject<ErrorSL>(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                ri.Content = "";
                ri.Registered = false;
                ri.Message = errorMessage.error.message.ToString();
            }
            catch (Exception ex)
            {
                ri.Content = "";
                ri.Registered = false;
                ri.Message = "Ocurrio error al actualizar ";
            }
            finally
            {
                AditionalInfomation ai = await InserLog(ri, requestInformation, httpWebGetRequest, nomSistem, BaseDatos);
            }

            return ri;
        }

        public async Task<ResponseInformation> DeleteInfo(RequestInformation requestInformation, string nomSistem, EmpresaConfig BaseDatos, string docEntry)
        {
            ResponseInformation ri = new ResponseInformation();
            HttpWebRequest httpWebGetRequest = null;

            try
            {
                //OrderUpdateDTO objetoDeserializado = JsonSerializer.Deserialize<OrderUpdateDTO>();
                PurchaseRequestUpdateDTO or = JsonConvert.DeserializeObject<PurchaseRequestUpdateDTO>(requestInformation.Doc.ToString());



                List<ListaDelete> lista1 = new List<ListaDelete>();


                //List<int> lista = new List<int>();

                foreach (var item in or.DocumentLines)
                {
                    ListaDelete lista = new ListaDelete();

                    lista.catCost_Code = item.CatCost_Code;
                    lista.item_del_Presupuesto = int.Parse(item.Item_del_Presupuesto) - 1;

                    lista1.Add(lista);
                }
                requestInformation.Doc = JsonConvert.SerializeObject(lista1);

                if (lista1.Count > 0)
                {

                    LoginDIAPI loginDIAPI = new LoginDIAPI();
                    loginDIAPI.Server = BaseDatos.ServiceLayer.Server.ToString();
                    loginDIAPI.Licencia = BaseDatos.ServiceLayer.ServerLicencia.ToString();
                    loginDIAPI.Db = BaseDatos.ServiceLayer.CompanyDB.ToString();
                    loginDIAPI.User = BaseDatos.ServiceLayer.UserName.ToString();
                    loginDIAPI.Password = BaseDatos.ServiceLayer.Password.ToString();
                    /*if (BaseDatos == "1")
                    {
                        loginDIAPI.Server = _configuration["ServiceLayer:Server"].ToString();
                        loginDIAPI.Licencia = _configuration["ServiceLayer:ServerLicencia"].ToString();
                        loginDIAPI.Db = _configuration["ServiceLayer:CompanyDB"].ToString();
                        loginDIAPI.User = _configuration["ServiceLayer:UserName"].ToString();
                        loginDIAPI.Password = _configuration["ServiceLayer:Password"].ToString();
                    }
                    else
                    {
                        loginDIAPI.Server = _configuration["ServiceLayer2:Server"].ToString();
                        loginDIAPI.Licencia = _configuration["ServiceLayer2:ServerLicencia"].ToString();
                        loginDIAPI.Db = _configuration["ServiceLayer2:CompanyDB"].ToString();
                        loginDIAPI.User = _configuration["ServiceLayer2:UserName"].ToString();
                        loginDIAPI.Password = _configuration["ServiceLayer2:Password"].ToString();
                    }
                    */

                    httpWebGetRequest = (HttpWebRequest)WebRequest.Create("https://conexionDIAPI/" + requestInformation.Route + "/DataBase:" + loginDIAPI.Db);
                    //httpWebGetRequest.Address = "application/json";
                    httpWebGetRequest.Method = "DELETE";


                    var result = Connection.Conexion.MainConnection(int.Parse(docEntry.ToString()), lista1, loginDIAPI);
                    ri.Registered = result.Item1;
                    ri.Message = result.Item2;
                }
                else
                {
                    ri.Registered = false;
                    ri.Message = "No ha especificado lineas a eliminar del documento.";
                }


            }
            catch (Exception ex)
            {
                ri.Content = "";
                ri.Registered = false;
                ri.Message = "Ocurrio error al actualizar " + ex.Message;
            }
            finally
            {
                AditionalInfomation ai = await InserLog(ri, requestInformation, httpWebGetRequest, nomSistem, BaseDatos);
            }

            return ri;
        }
        /*
         public async Task<ResponseInformation> DeleteInfo(RequestInformation requestInformation, string nomSistem, string BaseDatos, string docEntry)
        {
            ResponseInformation ri = new ResponseInformation();
            HttpWebRequest httpWebGetRequest = null;
            
            try
            {
                //OrderUpdateDTO objetoDeserializado = JsonSerializer.Deserialize<OrderUpdateDTO>();
                PurchaseRequestUpdateDTO or = JsonConvert.DeserializeObject<PurchaseRequestUpdateDTO>(requestInformation.Doc.ToString());

                List<int> lista = new List<int>();
                foreach (var item in or.DocumentLines)
                {
                    lista.Add(int.Parse(item.Item_del_Presupuesto) - 1);
                }


                if(lista.Count > 0)
                {

                    LoginDIAPI loginDIAPI = new LoginDIAPI();

                    if (BaseDatos == "1")
                    {
                        loginDIAPI.Server = _configuration["ServiceLayer:Server"].ToString();
                        loginDIAPI.Licencia = _configuration["ServiceLayer:ServerLicencia"].ToString();
                        loginDIAPI.Db = _configuration["ServiceLayer:CompanyDB"].ToString();
                        loginDIAPI.User = _configuration["ServiceLayer:UserName"].ToString();
                        loginDIAPI.Password = _configuration["ServiceLayer:Password"].ToString();
                    }
                    else
                    {
                        loginDIAPI.Server = _configuration["ServiceLayer2:Server"].ToString();
                        loginDIAPI.Licencia = _configuration["ServiceLayer2:ServerLicencia"].ToString();
                        loginDIAPI.Db = _configuration["ServiceLayer2:CompanyDB"].ToString();
                        loginDIAPI.User = _configuration["ServiceLayer2:UserName"].ToString();
                        loginDIAPI.Password = _configuration["ServiceLayer2:Password"].ToString();
                    }

                    httpWebGetRequest = (HttpWebRequest)WebRequest.Create("https://conexionDIAPI/" + requestInformation.Route +"/DataBase:"+ loginDIAPI.Db);
                    // httpWebGetRequest.Address = "application/json";
                    httpWebGetRequest.Method = "DELETE";


                    var result = Connection.Conexion.MainConnection(int.Parse(docEntry.ToString()), lista, loginDIAPI);
                    ri.Registered = result.Item1;
                    ri.Message = result.Item2;
                }
                else
                {
                    ri.Registered = false;
                    ri.Message = "No ha especificado lineas a eliminar del documento.";
                }
                    

            }
            catch (Exception ex)
            {
                ri.Content = "";
                ri.Registered = false;
                ri.Message = "Ocurrio error al actualizar " + ex.Message;
            }
            finally
            {
                AditionalInfomation ai = await InserLog(ri, requestInformation, httpWebGetRequest, nomSistem, BaseDatos);
            }

            return ri;
        }
        */
        public async Task<DocumentGetDTO> GetInfo(RequestInformation requestInformation, string BaseDatos)
        {
            DocumentGetDTO ri = new DocumentGetDTO();
            try
            {
                string route = "";

                if (BaseDatos == "1")
                {
                    route = _configuration["ServiceLayer:sl_route"].ToString() + requestInformation.Route;
                }
                else
                {
                    route = _configuration["ServiceLayer2:sl_route"].ToString() + requestInformation.Route;
                }

                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls;

                var httpWebGetRequest = (HttpWebRequest)WebRequest.Create(route);
                httpWebGetRequest.ContentType = "application/json";
                httpWebGetRequest.Method = "GET";
                CookieContainer cookies = new CookieContainer();
                cookies.Add(new Cookie("B1SESSION", requestInformation.Token.ToString()) { Domain = BaseDatos == "1" ?  _configuration["ServiceLayer:sl_value"].ToString() : _configuration["ServiceLayer2:sl_value"].ToString() });
                //cookies.Add(new Cookie("ROUTEID", ".node1") { Domain = _configuration["ServiceLayer:ip_value"].ToString() });
                httpWebGetRequest.CookieContainer = cookies;

                //using (var streamWriter = new StreamWriter(httpWebGetRequest.GetRequestStream()))
                //{ streamWriter.Write(requestInformation.Doc); }

                using (var streamReader = new StreamReader(httpWebGetRequest.GetResponse().GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    ri = JsonConvert.DeserializeObject<DocumentGetDTO>(result);
                    
                }
            }
            catch (Exception ex)
            {
                ri=null;
            }

            return ri;
        }

        /* public Task<DocumentGetDTO> GetDoc(RequestInformation requestInformation, string nomSistem, string BaseDatos, string docEntry)
         {
             ResponseInformation ri = new ResponseInformation();

             try
             {
                 //OrderUpdateDTO objetoDeserializado = JsonSerializer.Deserialize<OrderUpdateDTO>();
                 //PurchaseRequestUpdateDTO or = JsonConvert.DeserializeObject<PurchaseRequestUpdateDTO>(requestInformation.Doc.ToString());

                 List<int> lista = new List<int>();
                 foreach (var item in or.DocumentLines)
                 {
                     lista.Add(int.Parse(item.Item_del_Presupuesto) - 1);
                 }


                 //if (lista.Count > 0)
                 //{

                     LoginDIAPI loginDIAPI = new LoginDIAPI();

                     if (BaseDatos == "1")
                     {
                         loginDIAPI.Server = _configuration["ServiceLayer:Server"].ToString();
                         loginDIAPI.Licencia = _configuration["ServiceLayer:ServerLicencia"].ToString();
                         loginDIAPI.Db = _configuration["ServiceLayer:CompanyDB"].ToString();
                         loginDIAPI.User = _configuration["ServiceLayer:UserName"].ToString();
                         loginDIAPI.Password = _configuration["ServiceLayer:Password"].ToString();
                     }
                     else
                     {
                         loginDIAPI.Server = _configuration["ServiceLayer2:Server"].ToString();
                         loginDIAPI.Licencia = _configuration["ServiceLayer2:ServerLicencia"].ToString();
                         loginDIAPI.Db = _configuration["ServiceLayer2:CompanyDB"].ToString();
                         loginDIAPI.User = _configuration["ServiceLayer2:UserName"].ToString();
                         loginDIAPI.Password = _configuration["ServiceLayer2:Password"].ToString();
                     }


                 GetQueryDoc

                 var result = Connection.Conexion.MainConnection(int.Parse(docEntry.ToString()), lista, loginDIAPI);
                     ri.Registered = result.Item1;
                     ri.Message = result.Item2;
                 }
                 else
                 {
                     ri.Registered = false;
                     ri.Message = "No ha especificado lineas a eliminar del documento.";
                 }


             }
             catch (Exception ex)
             {
                 ri.Content = "";
                 ri.Registered = false;
                 ri.Message = "Ocurrio error al actualizar " + ex.Message;
             }

             return ri;
         }*/
        //(RequestInformation requestInformation, string nomSistem, string BaseDatos, string project)
        //c(string project, bool esOV, string BaseDatos)
        public async Task<DocumentGetDTO> GetDoc(RequestInformation requestInformation, string nomSistem, string BaseDatos, string project)
        {
            
            //string result = "";
            string HANAConnectionString = "";
            DocumentGetDTO result = new DocumentGetDTO();
            var vTipoCabecera = string.Empty;
            var vTipoDetalle = string.Empty;
            HANAConnectionString = BaseDatos == "1" ? _configuration["ConnectionStringsSAP"] : _configuration["ConnectionStringsSAP2"];

            HanaConnection conn = new HanaConnection(HANAConnectionString);
            AditionalInfomation ai = new AditionalInfomation();
            try
            {

                switch (requestInformation.Route)
                {
                    case "PurchaseRequests": //Solicitud de compras
                        vTipoCabecera = "Get_DocCabPRQ";
                        vTipoDetalle = "Get_DocDetPRQ";
                        break;

                    case "PurchaseInvoices": //factura de compras
                        vTipoCabecera = "Get_DocCabPCH";
                        vTipoDetalle = "Get_DocDetPCH";
                        break;
                    case "PurchaseCreditNote": //nota de credito de compras
                        vTipoCabecera = "Get_DocCabRPC";
                        vTipoDetalle = "Get_DocDetRPC";
                        break;
                    case "SalesInvoice": //factura de ventas
                        vTipoCabecera = "Get_DocCabINV";
                        vTipoDetalle = "Get_DocDetINV";
                        break;
                    case "SalesCreditNote": //nota de credito de ventas
                        vTipoCabecera = "Get_DocCabRIN";
                        vTipoDetalle = "Get_DocDetRIN";
                        break;                    
                }

                if (string.IsNullOrWhiteSpace(vTipoCabecera) || string.IsNullOrWhiteSpace(vTipoDetalle))
                {
                    return result;
                }

                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();

                    using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = vTipoCabecera;
                    cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = project ?? string.Empty;
                    cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = BaseDatos ?? string.Empty;
                    cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                    cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                    using var reader = cmd.ExecuteReader();

                    // Objeto que representará la cabecera
                    List<Value1> lisCab = new List<Value1>();
                    while (reader.Read())
                    {
                        Value1 cab = new Value1();                        
                        cab.DocEntry = reader.GetInt32(reader.GetOrdinal("DocEntry"));
                        cab.DocNum = reader.GetInt32(reader.GetOrdinal("DocNum"));
                        cab.DocDate = reader.GetString(reader.GetOrdinal("DocDate"));
                        cab.DocDueDate = reader.GetString(reader.GetOrdinal("DocDueDate"));                       
                                             
                                       
                        cab.NumAtCard = reader.GetString(reader.GetOrdinal("NumAtCard"));
                        cab.TaxDate = reader.GetString(reader.GetOrdinal("TaxDate"));
                        cab.Cod_PyP = reader.GetString(reader.GetOrdinal("Cod_PyP"));
                        cab.idEmpresa = BaseDatos;
                        cab.Project = reader.GetString(reader.GetOrdinal("Project"));
                        cab.tipoDoc = reader.GetString(reader.GetOrdinal("tipoDoc"));
                        cab.DocCur = reader.GetString(reader.GetOrdinal("DocCur"));
                        cab.TotalSol = reader.GetDouble(reader.GetOrdinal("TotalSol"));
                        cab.TotalDol = reader.GetDouble(reader.GetOrdinal("TotalDol"));
                        cab.Status = reader.GetString(reader.GetOrdinal("Status"));
                        cab.Ruc = reader.GetString(reader.GetOrdinal("Ruc"));
                        cab.Proveedor = reader.GetString(reader.GetOrdinal("Proveedor"));
                        cab.JrnlMemo = reader.GetString(reader.GetOrdinal("JrnlMemo"));
                        cab.UserCreator = reader.GetString(reader.GetOrdinal("UserCreator"));



                        ////detalle
                        using var cmdDetalle = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmdDetalle.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = vTipoDetalle;
                        cmdDetalle.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = cab.DocEntry.ToString();
                        cmdDetalle.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = project ?? string.Empty;
                        cmdDetalle.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = BaseDatos ?? string.Empty;
                        cmdDetalle.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = cab.NumAtCard ?? string.Empty;

                        using var readerdet = cmdDetalle.ExecuteReader();

                        List<DocumentLineD> lisDet = new List<DocumentLineD>();

                        while (readerdet.Read())
                        {
                            DocumentLineD mDetalles = new DocumentLineD();
                            mDetalles.ItemCode = readerdet.GetString(readerdet.GetOrdinal("ItemCode"));
                            mDetalles.U_MGS_CL_NITEMPYP = readerdet.GetString(readerdet.GetOrdinal("U_MGS_CL_NITEMPYP"));
                            mDetalles.Quantity = readerdet.GetDouble(readerdet.GetOrdinal("Quantity"));
                            mDetalles.Price = readerdet.GetDouble(readerdet.GetOrdinal("Price"));
                            mDetalles.Project = readerdet.GetString(readerdet.GetOrdinal("Project"));
                            mDetalles.U_MGS_CL_TIPBENPRO = readerdet.GetString(readerdet.GetOrdinal("U_MGS_CL_TIPBENPRO"));
                            mDetalles.LineTotal = readerdet.GetDouble(readerdet.GetOrdinal("LineTotal"));
                            mDetalles.UnidadNegocio = readerdet.GetString(readerdet.GetOrdinal("UnidadNegocio"));
                            mDetalles.JefeCuenta = readerdet.GetString(readerdet.GetOrdinal("JefeCuenta"));
                            mDetalles.Familia = readerdet.GetString(readerdet.GetOrdinal("Familia"));
                            mDetalles.EstadoProyecto = readerdet.GetString(readerdet.GetOrdinal("EstadoProyecto"));

                            if (requestInformation.Route == "PurchaseRequests")
                            {
                                mDetalles.QuantityIni = readerdet.GetDouble(readerdet.GetOrdinal("cantidad_Inicial"));
                                mDetalles.PriceIni = readerdet.GetDouble(readerdet.GetOrdinal("costo_Unit_Inicial"));
                            }

                            if ((requestInformation.Route == "SalesInvoice") || (requestInformation.Route == "SalesCreditNote"))
                            {
                                mDetalles.Porcentaje = readerdet.GetDouble(readerdet.GetOrdinal("Porcentaje"));
                               
                            }

                            lisDet.Add(mDetalles); 
                        }
                        
                        cab.DocumentLines = lisDet;
                        lisCab.Add(cab);                       
                        readerdet.Close();
                   }
                    result.value = lisCab;
                    reader.Close();
                    
                    conn.Close();
                }


            }
            catch (Exception ex)
            {
                if (conn.State.Equals(ConnectionState.Open))
                    conn.Close();
                //result = "";
                //throw;
            }
            return result;
        }     
    
    public async Task<AmountAvailableGetDTO> GetAmountAvailable(RequestInformation requestInformation, string nomSistem, string BaseDatos, string project)
    {

        //string result = "";
        string HANAConnectionString = "";
            AmountAvailableGetDTO result = new AmountAvailableGetDTO();
        HANAConnectionString = BaseDatos == "1" ? _configuration["ConnectionStringsSAP"] : _configuration["ConnectionStringsSAP2"];

        HanaConnection conn = new HanaConnection(HANAConnectionString);
        AditionalInfomation ai = new AditionalInfomation();
            //AmountAvailableGetDTO datos = new AmountAvailableGetDTO();
        try
        {            

            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_AmtAvail";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = project ?? string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = cmd.ExecuteReader();

                    List<Value2> lisCab = new List<Value2>();
                    
                    while (reader.Read())                        
                    {
                        Value2 datos = new Value2();
                        datos.contrato = reader.GetString(reader.GetOrdinal("contrato"));
                        datos.Project = reader.GetString(reader.GetOrdinal("Project"));
                        datos.CategoriaCosto = reader.GetString(reader.GetOrdinal("categoriaCosto"));
                        //datos.precioUnit = reader.GetDouble(reader.GetOrdinal("precioUnit"));
                        datos.montoPresu = reader.GetDouble(reader.GetOrdinal("montoPresu"));
                        datos.montoOC = reader.GetDouble(reader.GetOrdinal("montoOC"));
                        datos.montoFact = reader.GetDouble(reader.GetOrdinal("montoFact"));
                        datos.montoDisp = reader.GetDouble(reader.GetOrdinal("montoDisp"));
                        lisCab.Add(datos);
                    }
                    //result.value = datos;
                    
                    result.value = lisCab;
                    reader.Close();

                conn.Close();
            }


        }
        catch (Exception ex)
        {
            if (conn.State.Equals(ConnectionState.Open))
                conn.Close();
            //result = "";
            //throw;
        }
        return result;
    }

        /* public async Task<ResponseInformation> GetInfoDB(RequestInformation requestInformation, string nomSistem, string BaseDatos)
         {
             string HANAConnectionString = BaseDatos == "1"
                 ? _configuration["ConnectionStringsSAP"]
                 : _configuration["ConnectionStringsSAP2"];

             ResponseInformation result = new ResponseInformation();          


             dynamic json = JsonConvert.DeserializeObject(requestInformation.Doc);
             var Param1 = Convert.ToString(json.Param1);
             var Param2 = Convert.ToString(json.Param2);

             using (HanaConnection conn = new HanaConnection(HANAConnectionString))
             {
                 try
                 {
                     int count = 0;
                     await conn.OpenAsync();

                     using (HanaCommand cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn))
                     {
                         cmd.CommandType = CommandType.StoredProcedure;

                         // Parámetro de entrada desde requestInformation
                         cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = requestInformation.Route;
                         cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = Param1;
                         cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = Param2;
                         cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = "";
                         cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = "";



                         using (HanaDataReader reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                         {
                             while (reader.Read())
                             {

                                 count = reader.GetInt32(reader.GetOrdinal("Count"));

                                 if (count == 1)
                                 {
                                     result.Registered = true;
                                     result.Message = "";
                                     result.Content = "";
                                 }
                                 else
                                 {
                                     result.Registered = false;
                                     result.Message = "Credenciales inválidas";
                                     result.Content = "";
                                 }

                             }
                         }


                     }
                 }
                 catch (Exception ex)
                 {
                     result.Registered = false;
                     result.Message = "Error en base de datos.";
                     result.Content = ex.Message;
                 }
                 finally
                 {
                     if (conn.State == ConnectionState.Open)
                         conn.Close();
                 }
             }

             return result;
         }*/


        public async Task<ResponseInformation> GetInfoDB(RequestInformation requestInformation, string nomSistem, string empresa)
        {
            var result = new ResponseInformation();

            // 1) Validar y obtener configuración de la empresa
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                result.Registered = false;
                result.Message = "Error: Parámetro 'empresa' debe ser un número válido.";
                return result;
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {
                result.Registered = false;
                result.Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.";
                return result;
            }

            // 2) Usar ConnectionString de la empresa
            string HANAConnectionString = cfg.ConnectionString;

            //string HANAConnectionString = empresa == "1"
            //     ? _configuration["ConnectionStringsSAP"]
            //     : _configuration["ConnectionStringsSAP2"];          

            dynamic json = JsonConvert.DeserializeObject(requestInformation.Doc);
            string param1 = Convert.ToString(json.Param1);
            string param2 = Convert.ToString(json.Param2);
            string param3 = DateTime.Now.Year.ToString();

            using (var conn = new HanaConnection(HANAConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = requestInformation.Route;
                        cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = param1;
                        cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = param2;
                        cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = param3;
                        cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                        using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {                                
                                string usuarioID = reader.IsDBNull(reader.GetOrdinal("UsuarioID"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("UsuarioID"));
                                string NombreCompleto = reader.IsDBNull(reader.GetOrdinal("NombreCompleto"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("NombreCompleto"));
                                string CardCode = reader.IsDBNull(reader.GetOrdinal("CardCode"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("CardCode"));

                               string PerfilId = reader.IsDBNull(reader.GetOrdinal("PerfilId"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("PerfilId"));

                               string NombrePerfil = reader.IsDBNull(reader.GetOrdinal("NombrePerfil"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("NombrePerfil"));

                                string Popup = reader.IsDBNull(reader.GetOrdinal("Popup"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Popup"));
                                //"C:\\MGSPortalWeb\\imagenpopup.png"

                                var dataUri = "";

                                if (Popup != "")
                                {
                                    var fileBytes = await System.IO.File.ReadAllBytesAsync(Popup);
                                    var base64 = Convert.ToBase64String(fileBytes);
                                    dataUri = $"data:image/png;base64,{base64}";
                                }

                                result.Registered = true;
                                    result.Message = string.Empty;
                                    // Mapear a modelo para mantener orden de propiedades
                                    var info = new AccountInfoDto
                                    {
                                        UsuarioID = usuarioID,
                                        NombreCompleto = NombreCompleto,
                                        CardCode = CardCode,
                                        PerfilId = PerfilId,
                                        NombrePerfil = NombrePerfil,
                                        Popup = dataUri
                                    };
                                    result.Content = JsonConvert.SerializeObject(info);                              
                               
                            }
                            else
                            {
                                // No se devolvieron filas
                                result.Registered = false;
                                result.Message = "Credenciales inválidas";
                                result.Content = string.Empty;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Registered = false;
                    result.Message = "Error en base de datos.";
                    result.Content = ex.Message;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return result;
        }
    

        public async Task<ResponseInformation> GetMenuDB(string empresa, string userId)
        {
            //string HANAConnectionString = empresa == "1"
            //    ? _configuration["ConnectionStringsSAP"]
            //    : _configuration["ConnectionStringsSAP2"];

            //1) Validar y obtener configuración de la empresa
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error: Parámetro 'empresa' debe ser un número válido.",
                    Content = ""

                };
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {

                return new ResponseInformation
                {
                    Registered = false,
                    Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.",
                    Content = ""
                };
            }


            //2) Usar ConnectionString de la empresa
            string HANAConnectionString = cfg.ConnectionString;

            var flatList = new List<dynamic>();

            using var conn = new HanaConnection(HANAConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand($"CALL MGS_HDB_PE_SP_PORTALWEB('Get_menu', '{userId}', '', '', '')", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    flatList.Add(new
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Nombre = reader.GetString(reader.GetOrdinal("NombreMenu")),
                        Ruta = reader.IsDBNull(reader.GetOrdinal("Ruta")) ? null : reader.GetString(reader.GetOrdinal("Ruta")),
                        EsPadre = reader.GetString(reader.GetOrdinal("EsPadre")) == "SI",
                        PadreId = reader.IsDBNull(reader.GetOrdinal("PadreID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PadreID"))

                    });
                }
                if (flatList.Count == 0)
                {
                    return new ResponseInformation
                    {
                        Registered = false,
                        Message = "No tienes módulos disponibles en tu menú",
                        Content = ""
                    };
                }
                var lookup = flatList.ToDictionary(x => x.Id, x => new MenuItemDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Ruta = x.Ruta
                });

                foreach (var item in flatList)
                {
                    if (item.PadreId is int padreId && lookup.ContainsKey(padreId))
                    {
                        lookup[padreId].Hijos.Add(lookup[item.Id]);
                    }
                }

                var rootItems = lookup.Values.Where(x => flatList.First(f => f.Id == x.Id).EsPadre).ToList();

                return new ResponseInformation
                {
                    Registered = true,
                    Message = "",
                    Content = JsonConvert.SerializeObject(new { menu = rootItems })
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error al cargar tiendas.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<ResponseInformation> GetRecepcionDB(string empresa)
        {
            //string HANAConnectionString = empresa == "1"
            //    ? _configuration["ConnectionStringsSAP"]
            //    : _configuration["ConnectionStringsSAP2"];

            //1) Validar y obtener configuración de la empresa
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error: Parámetro 'empresa' debe ser un número válido.",
                    Content = ""

                };
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {

                return new ResponseInformation
                {
                    Registered = false,
                    Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.",
                    Content = ""
                };
            }


            //2) Usar ConnectionString de la empresa
            string HANAConnectionString = cfg.ConnectionString;

            var lista = new List<FechaRecepcionDTO>();
            using var conn = new HanaConnection(HANAConnectionString);

            try
            {
                await conn.OpenAsync();

                // Ejecutar Stored Procedure
                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_fechaRecepcion";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    // Intento de lectura segura de fechas con cast directo
                    DateTime fechaInicio;
                    DateTime fechaFin;

                    int ordIni = reader.GetOrdinal("U_MGS_CL_FERINI");
                    int ordFin = reader.GetOrdinal("U_MGS_CL_FERFIN");

                    var iniObj = reader.GetValue(ordIni);
                    fechaInicio = iniObj is DateTime dtIni ? dtIni : default;

                    var finObj = reader.GetValue(ordFin);
                    fechaFin = finObj is DateTime dtFin ? dtFin : default;

                    string mes = reader.IsDBNull(reader.GetOrdinal("Mes"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Mes"));
                    string diaSemanaNumero = reader.IsDBNull(reader.GetOrdinal("DiaSemanaNumero"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("DiaSemanaNumero"));

                    lista.Add(new FechaRecepcionDTO
                    {
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin,
                        Mes = mes,
                        DiaSemanaNumero = diaSemanaNumero
                    });
                }

                // Si no hay resultados, retornar false
                if (!lista.Any())
                {
                    return new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontraron fechas de recepción",
                        Content = string.Empty
                    };
                }

                // Retornar JSON en Content
                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(lista)
                };

            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };               
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

        
        }
        public async Task<ResponseInformation> GetFactoresDB(string empresa, string periodo, string tiendasCsv)
        {
            //string HANAConnectionString = empresa == "1"
            //    ? _configuration["ConnectionStringsSAP"]
            //    : _configuration["ConnectionStringsSAP2"];

            //1) Validar y obtener configuración de la empresa
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error: Parámetro 'empresa' debe ser un número válido.",
                    Content = ""

                };
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {

                return new ResponseInformation
                {
                    Registered = false,
                    Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.",
                    Content = ""
                };
            }


            //2) Usar ConnectionString de la empresa           
            var lista = new List<MatrizFactorDTO>();
            using var conn = new HanaConnection(cfg.ConnectionString);            

            try
            {
                await conn.OpenAsync();

                // Ejecutar Stored Procedure
                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_MatrizFactores";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = periodo;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 255).Value = tiendasCsv; // tiendasCsv ?? ""; // P0045,P0031,...
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var rd = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (rd.Read())
                {
                    // Nombres de columnas DEVUELTAS por tu SP (ajústalos si difieren)
                    lista.Add(new MatrizFactorDTO
                    {
                        U_MGS_CL_PERIODO = rd["U_MGS_CL_PERIODO"] as string,
                        U_MGS_CL_TIENDA = rd["U_MGS_CL_TIENDA"] as string,
                        U_MGS_CL_NOMTIE = rd["U_MGS_CL_NOMTIE"] as string,
                        DocEntry = rd["DocEntry"]?.ToString(),
                        LineId = rd["LineId"]?.ToString(),

                        // numéricos (Edm.Double)
                        U_MGS_CL_META = ToDec(rd, "U_MGS_CL_META"),
                        U_MGS_CL_RENTA = ToDec(rd, "U_MGS_CL_RENTA"),
                        U_MGS_CL_VAN = ToDec(rd, "U_MGS_CL_VAN"),
                        U_MGS_CL_ESTPER = ToDec(rd, "U_MGS_CL_ESTPER"),
                        U_MGS_CL_GASADM = ToDec(rd, "U_MGS_CL_GASADM"),
                        U_MGS_CL_COMTAR = ToDec(rd, "U_MGS_CL_COMTAR"),
                        U_MGS_CL_IMPUES = ToDec(rd, "U_MGS_CL_IMPUES"),
                        U_MGS_CL_REGALI = ToDec(rd, "U_MGS_CL_REGALI"),
                        U_MGS_CL_AUSER = ToDec(rd, "U_MGS_CL_AUSER"),
                        U_MGS_CL_AUOPE = ToDec(rd, "U_MGS_CL_AUOPE"),
                        U_MGS_CL_AUCCC = ToDec(rd, "U_MGS_CL_AUCCC"),
                        U_MGS_CL_AUADH = ToDec(rd, "U_MGS_CL_AUADH"),
                        U_MGS_CL_CLIMA = ToDec(rd, "U_MGS_CL_CLIMA"),
                        U_MGS_CL_RUSTI = ToDec(rd, "U_MGS_CL_RUSTI"),
                        U_MGS_CL_MELID = ToDec(rd, "U_MGS_CL_MELID"),
                        U_MGS_CL_ADMGR = ToDec(rd, "U_MGS_CL_ADMGR"),
                        U_MGS_CL_EXGES = ToDec(rd, "U_MGS_CL_EXGES"),
                        U_MGS_CL_EXSER = ToDec(rd, "U_MGS_CL_EXSER"),
                        U_MGS_CL_EXMAR = ToDec(rd, "U_MGS_CL_EXMAR"),

                        U_MGS_CL_PRIMARY = rd["U_MGS_CL_PRIMARY"] as string
                    });
                }

                if (!lista.Any())
                    return new ResponseInformation { Registered = false, Message = "Sin datos", Content = "" };

                return new ResponseInformation
                {
                    Registered = true,
                    Message = "",
                    Content = JsonConvert.SerializeObject(lista)
                };

            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };               
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            static decimal ToDec(HanaDataReader r, string col)
           => r.IsDBNull(r.GetOrdinal(col)) ? 0m : Convert.ToDecimal(r[col]);

        }

        public async Task<ResponseInformation> GetFactoresNuevoDB(string empresa)
        {
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error: Parámetro 'empresa' debe ser un número válido.",
                    Content = "",

                };
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.",
                    Content = "",
                };
            }

            var lista = new List<MatrizFactorDTO>();
            using var conn = new HanaConnection(cfg.ConnectionString);

            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_FactoresNuevo";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 255).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var rd = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (rd.Read())
                {
                    lista.Add(new MatrizFactorDTO
                    {
                        U_MGS_CL_PERIODO = rd["U_MGS_CL_PERIODO"] as string,
                        U_MGS_CL_PERIODO_DEST = rd["U_MGS_CL_PERIODO_DEST"] as string,
                        U_MGS_CL_TIENDA = rd["U_MGS_CL_TIENDA"] as string,
                        U_MGS_CL_NOMTIE = rd["U_MGS_CL_NOMTIE"] as string,
                        DocEntry = rd["DocEntry"]?.ToString(),
                        LineId = rd["LineId"]?.ToString(),

                        U_MGS_CL_META = ToDec(rd, "U_MGS_CL_META"),
                        U_MGS_CL_RENTA = ToDec(rd, "U_MGS_CL_RENTA"),
                        U_MGS_CL_VAN = ToDec(rd, "U_MGS_CL_VAN"),
                        U_MGS_CL_ESTPER = ToDec(rd, "U_MGS_CL_ESTPER"),
                        U_MGS_CL_GASADM = ToDec(rd, "U_MGS_CL_GASADM"),
                        U_MGS_CL_COMTAR = ToDec(rd, "U_MGS_CL_COMTAR"),
                        U_MGS_CL_IMPUES = ToDec(rd, "U_MGS_CL_IMPUES"),
                        U_MGS_CL_REGALI = ToDec(rd, "U_MGS_CL_REGALI"),
                        U_MGS_CL_AUSER = ToDec(rd, "U_MGS_CL_AUSER"),
                        U_MGS_CL_AUOPE = ToDec(rd, "U_MGS_CL_AUOPE"),
                        U_MGS_CL_AUCCC = ToDec(rd, "U_MGS_CL_AUCCC"),
                        U_MGS_CL_AUADH = ToDec(rd, "U_MGS_CL_AUADH"),
                        U_MGS_CL_CLIMA = ToDec(rd, "U_MGS_CL_CLIMA"),
                        U_MGS_CL_RUSTI = ToDec(rd, "U_MGS_CL_RUSTI"),
                        U_MGS_CL_MELID = ToDec(rd, "U_MGS_CL_MELID"),
                        U_MGS_CL_ADMGR = ToDec(rd, "U_MGS_CL_ADMGR"),
                        U_MGS_CL_EXGES = ToDec(rd, "U_MGS_CL_EXGES"),
                        U_MGS_CL_EXSER = ToDec(rd, "U_MGS_CL_EXSER"),
                        U_MGS_CL_EXMAR = ToDec(rd, "U_MGS_CL_EXMAR"),

                        U_MGS_CL_PRIMARY = rd["U_MGS_CL_PRIMARY"] as string
                    });
                }

                if (!lista.Any())
                    return new ResponseInformation { Registered = false, Message = "Sin datos", Content = "" };

                return new ResponseInformation
                {
                    Registered = true,
                    Message = "",
                    Content = JsonConvert.SerializeObject(lista)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error al cargar grupos VAN.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            static decimal ToDec(HanaDataReader r, string col)
           => r.IsDBNull(r.GetOrdinal(col)) ? 0m : Convert.ToDecimal(r[col]);
        }

        public async Task<ResponseInformation> GetTiendasActivas(string empresa)
        {
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error: Parámetro 'empresa' debe ser un número válido.",
                    Content = string.Empty,
                };
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.",
                    Content = string.Empty,
                };
            }

            var tiendas = new List<TiendaActivaDTO>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_TiendasActivas";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    tiendas.Add(new TiendaActivaDTO
                    {
                        Codigo = reader[nameof(TiendaActivaDTO.Codigo)]?.ToString() ?? string.Empty,
                        Nombre = reader[nameof(TiendaActivaDTO.Nombre)]?.ToString() ?? string.Empty
                    });
                }

                if (!tiendas.Any())
                {
                    return new ResponseInformation
                    {
                        Registered = false,
                        Message = "No se encontraron tiendas activas.",
                        Content = string.Empty
                    };
                }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(tiendas)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error al cargar artículos maestro.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        public async Task<ResponseInformation> GetContactoDB(string empresa)
        {
          //  public async Task<ResponseInformation> GetContactoDB(RequestInformation requestInformation, string nomSistem, string empresa)
        //{
            var result = new ResponseInformation();

            // 1) Validar y obtener configuración de la empresa
            if (!int.TryParse(empresa, out var idEmpresa))
            {
                result.Registered = false;
                result.Message = "Error: Parámetro 'empresa' debe ser un número válido.";
                return result;
            }

            var cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {
                result.Registered = false;
                result.Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.";
                return result;
            }

            // 2) Usar ConnectionString de la empresa
            string HANAConnectionString = cfg.ConnectionString;

            //string HANAConnectionString = empresa == "1"
            //     ? _configuration["ConnectionStringsSAP"]
            //     : _configuration["ConnectionStringsSAP2"];         
                       
            using (var conn = new HanaConnection(HANAConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    using (var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_contactoEmpresa";
                        cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                        cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                        cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                        cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                        using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                string Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Phone"));
                                string Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Email"));
                                string LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("LogoUrl"));

                                var dataUri = "";

                                if (LogoUrl != "")
                                {
                                    var fileBytes = await System.IO.File.ReadAllBytesAsync(LogoUrl);
                                    var base64 = Convert.ToBase64String(fileBytes);
                                    dataUri = $"data:image/png;base64,{base64}";
                                }                       

                                result.Registered = true;
                                result.Message = string.Empty;
                                // Mapear a modelo para mantener orden de propiedades
                                var info = new ContactoInfoDto
                                {
                                    Phone = Phone,
                                    Email = Email,                                    
                                    LogoUrl = dataUri
                                };
                                result.Content = JsonConvert.SerializeObject(info);

                            }
                            else
                            {
                                // No se devolvieron filas
                                result.Registered = false;
                                result.Message = "No se encontraron datos de contacto";
                                result.Content = string.Empty;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Registered = false;
                    result.Message = "Error en base de datos. Contacto";
                    result.Content = ex.Message;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }

            return result;
        }

        private bool TryGetEmpresaConfig(string empresa, out EmpresaConfig cfg, out ResponseInformation error)
        {
            cfg = null;
            error = null;

            if (!int.TryParse(empresa, out var idEmpresa))
            {
                error = new ResponseInformation
                {
                    Registered = false,
                    Message = "Error: Parámetro 'empresa' debe ser un número válido.",
                    Content = string.Empty
                };
                return false;
            }

            cfg = _empresaConfigService.GetEmpresa(idEmpresa);
            if (cfg == null)
            {
                error = new ResponseInformation
                {
                    Registered = false,
                    Message = $"Error: No existe configuración para la empresa con ID = {idEmpresa}.",
                    Content = string.Empty
                };
                return false;
            }

            return true;
        }

        public async Task<ResponseInformation> GetGrupoVanTiendas(string empresa)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return error;
            }

            var tiendas = new List<VanTiendaDto>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanTienda";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    tiendas.Add(new VanTiendaDto
                    {
                        PrjCode = reader[nameof(VanTiendaDto.PrjCode)]?.ToString() ?? string.Empty,
                        PrjName = reader[nameof(VanTiendaDto.PrjName)]?.ToString() ?? string.Empty
                    });
                }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(tiendas)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error al cargar grupos por tienda.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<ResponseInformation> GetGrupoVanMaestro(string empresa)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return error;
            }

            var grupos = new List<VanGrupoMaestroDto>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanGrupoM";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    grupos.Add(new VanGrupoMaestroDto
                    {
                        Code = reader[nameof(VanGrupoMaestroDto.Code)]?.ToString() ?? string.Empty,
                        Name = reader[nameof(VanGrupoMaestroDto.Name)]?.ToString() ?? string.Empty
                    });
                }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(grupos)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error al cargar artículos del grupo.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<ResponseInformation> GetGrupoVanItemsMaestro(string empresa, string search)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return error;
            }

            var items = new List<VanItemMaestroDto>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanItemM";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = search ?? string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    items.Add(new VanItemMaestroDto
                    {
                        ItemCode = reader["ItemCode"]?.ToString() ?? string.Empty,
                        ItemName = reader["ItemName"]?.ToString() ?? string.Empty
                    });
                }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(items)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<ResponseInformation> GetGrupoVanPorTienda(string empresa, string tiendaCodigo)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return error;
            }

            var grupos = new List<VanGrupoDetalleDto>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanTdaGrp";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        grupos.Add(new VanGrupoDetalleDto
                        {
                            DocEntry = reader.IsDBNull(reader.GetOrdinal("DocEntry")) ? (int?)null : Convert.ToInt32(reader["DocEntry"]),
                            LineId = reader.IsDBNull(reader.GetOrdinal("LineId")) ? 0 : Convert.ToInt32(reader["LineId"]),
                            U_MGS_CL_GRPCOD = reader["U_MGS_CL_GRPCOD"]?.ToString() ?? string.Empty,
                            U_MGS_CL_GRPNOM = reader["U_MGS_CL_GRPNOM"]?.ToString() ?? string.Empty,
                            U_MGS_CL_TIPO = HasColumn(reader, "U_MGS_CL_TIPO") ? reader["U_MGS_CL_TIPO"]?.ToString() ?? string.Empty : string.Empty,
                            U_MGS_CL_PORC = HasColumn(reader, "U_MGS_CL_PORC") && decimal.TryParse(reader["U_MGS_CL_PORC"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var porc) ? porc : (decimal?)null
                        });
                    }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(grupos)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<ResponseInformation> GetGrupoVanArticulos(string empresa, string tiendaCodigo, string grupoCodigo)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return error;
            }

            var articulos = new List<VanArticuloDetalleDto>();

            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();

                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanGrpArt";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = grupoCodigo ?? string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;

                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        articulos.Add(new VanArticuloDetalleDto
                        {
                            DocEntry = reader.IsDBNull(reader.GetOrdinal("DocEntry")) ? (int?)null : Convert.ToInt32(reader["DocEntry"]),
                            LineId = reader.IsDBNull(reader.GetOrdinal("LineId")) ? 0 : Convert.ToInt32(reader["LineId"]),
                            U_MGS_CL_GRPCOD = reader["U_MGS_CL_GRPCOD"]?.ToString() ?? string.Empty,
                            U_MGS_CL_ITEMCOD = reader["U_MGS_CL_ITEMCOD"]?.ToString() ?? string.Empty,
                            U_MGS_CL_ITEMNAM = reader["U_MGS_CL_ITEMNAM"]?.ToString() ?? string.Empty,
                            U_MGS_CL_TIPO = HasColumn(reader, "U_MGS_CL_TIPO") ? reader["U_MGS_CL_TIPO"]?.ToString() ?? string.Empty : string.Empty,
                            U_MGS_CL_PORC = HasColumn(reader, "U_MGS_CL_PORC") && decimal.TryParse(reader["U_MGS_CL_PORC"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var porc) ? porc : (decimal?)null,
                            U_MGS_CL_ACTIVO = HasColumn(reader, "U_MGS_CL_ACTIVO") ? reader["U_MGS_CL_ACTIVO"]?.ToString() ?? string.Empty : string.Empty
                        });
                    }

                return new ResponseInformation
                {
                    Registered = true,
                    Message = string.Empty,
                    Content = JsonConvert.SerializeObject(articulos)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public async Task<ResponseInformation> GetGrupoVanArticulosActivos(string empresa, string tiendaCodigo, string grupoCodigo)
        {
            var login = await LoginEmpresa(empresa);
            if (!login.Ok)
            {
                return login.Error;
            }

            try
            {
                var articulos = await ObtenerArticulosActivosPorGrupo(login.Cfg, tiendaCodigo, grupoCodigo);
                var resultado = new
                {
                    Total = articulos.Count,
                    TieneArticulos = articulos.Count > 0
                };

                return new ResponseInformation
                {
                    Registered = true,
                    Message = "Consulta exitosa",
                    Content = JsonConvert.SerializeObject(resultado)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };
            }
        }

        public async Task<ResponseInformation> GetGrupoVanArticuloAsignacion(string empresa, string tiendaCodigo, string itemCode, string grupoCodigo)
        {
            var login = await LoginEmpresa(empresa);
            if (!login.Ok)
            {
                return login.Error;
            }

            try
            {
                var asignacion = await ObtenerGrupoArticuloActivo(login.Cfg, tiendaCodigo, itemCode, grupoCodigo);
                var resultado = new
                {
                    Existe = !string.IsNullOrWhiteSpace(asignacion.GrupoCodigo),
                    GrupoCodigo = asignacion.GrupoCodigo,
                    GrupoNombre = asignacion.GrupoNombre
                };

                return new ResponseInformation
                {
                    Registered = true,
                    Message = "Consulta exitosa",
                    Content = JsonConvert.SerializeObject(resultado)
                };
            }
            catch (Exception ex)
            {
                return new ResponseInformation
                {
                    Registered = false,
                    Message = "Error en base de datos.",
                    Content = ex.Message
                };
            }
        }

        public async Task<ResponseInformation> SetGrupoVanPorTiendaBulk(string empresa, string tiendaCodigo, IEnumerable<VanGrupoDetalleDto> items)
        {
            var login = await LoginEmpresa(empresa);
            if (!login.Ok)
            {
                return login.Error;
            }

            var lista = (items ?? Enumerable.Empty<VanGrupoDetalleDto>()).ToList();
            if (!lista.Any())
            {
                return new ResponseInformation { Registered = false, Message = "No se recibieron grupos para actualizar.", Content = string.Empty };
            }

            foreach (var item in lista)
            {
                if (!item.U_MGS_CL_PORC.HasValue || item.U_MGS_CL_PORC.Value <= 0)
                {
                    item.U_MGS_CL_PORC = 100;
                }
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_ACTIVO))
                {
                    item.U_MGS_CL_ACTIVO = "SI";
                }
            }

            var existingDocEntry = await ObtenerDocEntryVanCab(login.Cfg, tiendaCodigo);
            if (!existingDocEntry.HasValue)
            {
                existingDocEntry = lista.FirstOrDefault(i => i.DocEntry.HasValue)?.DocEntry;
            }
            if (existingDocEntry.HasValue)
            {
                foreach (var item in lista.Where(i => !i.DocEntry.HasValue))
                {
                    item.DocEntry = existingDocEntry;
                }

                foreach (var item in lista)
                {
                    if (string.IsNullOrWhiteSpace(item.U_MGS_CL_GRPCOD))
                    {
                        continue;
                    }

                    if (item.LineId <= 0)
                    {
                        var detalle = await ObtenerGrupoVanDetalle(login.Cfg, tiendaCodigo, item.U_MGS_CL_GRPCOD);
                        if (detalle.LineId.HasValue)
                        {
                            item.LineId = detalle.LineId.Value;
                            if (string.Equals(detalle.Activo, "NO", StringComparison.OrdinalIgnoreCase))
                            {
                                item.U_MGS_CL_ACTIVO = "SI";
                            }
                        }
                    }
                }
            }

            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            if (existingDocEntry.HasValue)
            {
                var updateReq = new
                {
                    MGS_CL_VANTDETCollection = lista.Select(i => new
                    {
                        i.LineId,
                        i.U_MGS_CL_GRPCOD,
                        i.U_MGS_CL_GRPNOM,
                        i.U_MGS_CL_TIPO,
                        U_MGS_CL_PORC = i.U_MGS_CL_PORC ?? 0,
                        U_MGS_CL_ACTIVO = string.IsNullOrWhiteSpace(i.U_MGS_CL_ACTIVO) ? "SI" : i.U_MGS_CL_ACTIVO
                    })
                };

                var requestInformation = new RequestInformation
                {
                    Route = $"MGS_CL_VANTCAB({existingDocEntry})",
                    Token = login.Token,
                    Doc = JsonConvert.SerializeObject(updateReq, settings)
                };

                var updateResponse = await UpdateInfo(requestInformation, "PYP", login.Cfg);
                if (!updateResponse.Registered)
                {
                    return updateResponse;
                }

                var gruposDesactivados = lista
                    .Where(g => string.Equals(g.U_MGS_CL_ACTIVO, "NO", StringComparison.OrdinalIgnoreCase))
                    .Select(g => g.U_MGS_CL_GRPCOD)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (gruposDesactivados.Any())
                {
                    var docEntryCab = existingDocEntry ?? await ObtenerDocEntryVanCab(login.Cfg, tiendaCodigo);
                    if (!docEntryCab.HasValue)
                    {
                        return new ResponseInformation
                        {
                            Registered = false,
                            Message = "No se encontró la cabecera VAN para desactivar artículos.",
                            Content = string.Empty
                        };
                    }

                    foreach (var grupo in gruposDesactivados)
                    {
                        var articulosActivos = await ObtenerArticulosActivosPorGrupo(login.Cfg, tiendaCodigo, grupo);
                        if (articulosActivos.Count == 0)
                        {
                            continue;
                        }

                        var updateArticulosReq = new
                        {
                            MGS_CL_VANTIADCollection = articulosActivos.Select(a => new
                            {
                                a.LineId,
                                a.U_MGS_CL_GRPCOD,
                                a.U_MGS_CL_ITEMCOD,
                                a.U_MGS_CL_ITEMNAM,
                                a.U_MGS_CL_TIPO,
                                U_MGS_CL_PORC = a.U_MGS_CL_PORC ?? 0,
                                U_MGS_CL_ACTIVO = "NO"
                            })
                        };

                        var requestArticulos = new RequestInformation
                        {
                            Route = $"MGS_CL_VANTCAB({docEntryCab.Value})",
                            Token = login.Token,
                            Doc = JsonConvert.SerializeObject(updateArticulosReq, settings)
                        };

                        var respArticulos = await UpdateInfo(requestArticulos, "PYP", login.Cfg);
                        if (!respArticulos.Registered)
                        {
                            return respArticulos;
                        }
                    }
                }

                return updateResponse;
            }
            else
            {
                var nombreTienda = await ObtenerNombreTienda(login.Cfg, tiendaCodigo);
                var createReq = new
                {
                    U_MGS_CL_TIENDA = tiendaCodigo,
                    U_MGS_CL_NOMTIE = string.IsNullOrWhiteSpace(nombreTienda) ? tiendaCodigo : nombreTienda,
                    MGS_CL_VANTDETCollection = lista.Select(i => new
                    {
                        i.U_MGS_CL_GRPCOD,
                        i.U_MGS_CL_GRPNOM,
                        i.U_MGS_CL_TIPO,
                        U_MGS_CL_PORC = i.U_MGS_CL_PORC ?? 0,
                        U_MGS_CL_ACTIVO = string.IsNullOrWhiteSpace(i.U_MGS_CL_ACTIVO) ? "SI" : i.U_MGS_CL_ACTIVO
                    })
                };

                var requestInformation = new RequestInformation
                {
                    Route = "MGS_CL_VANTCAB",
                    Token = login.Token,
                    Doc = JsonConvert.SerializeObject(createReq, settings)
                };

                return await PostInfo(requestInformation, "PYP", login.Cfg);
            }
        }

        public async Task<ResponseInformation> SetGrupoVanArticulosBulk(string empresa, string tiendaCodigo, string grupoCodigo, IEnumerable<VanArticuloDetalleDto> items)
        {
            var login = await LoginEmpresa(empresa);
            if (!login.Ok)
            {
                return login.Error;
            }

            var lista = (items ?? Enumerable.Empty<VanArticuloDetalleDto>()).ToList();
            if (!lista.Any())
            {
                return new ResponseInformation { Registered = false, Message = "No se recibieron artículos para actualizar.", Content = string.Empty };
            }

            foreach (var item in lista)
            {
                if (!item.U_MGS_CL_PORC.HasValue || item.U_MGS_CL_PORC.Value <= 0)
                {
                    item.U_MGS_CL_PORC = 100;
                }
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_ACTIVO))
                {
                    item.U_MGS_CL_ACTIVO = "SI";
                }
                if (string.IsNullOrWhiteSpace(item.U_MGS_CL_GRPCOD))
                {
                    item.U_MGS_CL_GRPCOD = grupoCodigo ?? string.Empty;
                }
            }

            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var existingDocEntry = await ObtenerDocEntryVanCab(login.Cfg, tiendaCodigo);
            if (!existingDocEntry.HasValue)
            {
                existingDocEntry = lista.FirstOrDefault(i => i.DocEntry.HasValue)?.DocEntry;
            }
            var grupoExiste = existingDocEntry.HasValue && await ExisteGrupoVan(login.Cfg, tiendaCodigo, grupoCodigo);

            foreach (var item in lista.Where(i => string.Equals(i.U_MGS_CL_ACTIVO, "SI", StringComparison.OrdinalIgnoreCase)))
            {
                var grupoAsignado = await ObtenerGrupoArticuloActivo(login.Cfg, tiendaCodigo, item.U_MGS_CL_ITEMCOD, item.U_MGS_CL_GRPCOD);
                if (!string.IsNullOrWhiteSpace(grupoAsignado.GrupoCodigo))
                {
                    return new ResponseInformation
                    {
                        Registered = false,
                        Message = $"El artículo ya está asignado al grupo {grupoAsignado.GrupoCodigo}. No puede repetirse en otra asignación de la misma tienda.",
                        Content = string.Empty
                    };
                }
            }

            if (!existingDocEntry.HasValue)
            {
                var nombreTienda = await ObtenerNombreTienda(login.Cfg, tiendaCodigo);
                var nombreGrupo = await ObtenerNombreGrupo(login.Cfg, grupoCodigo);
                var itemBase = lista.FirstOrDefault();
                var createReq = new
                {
                    U_MGS_CL_TIENDA = tiendaCodigo,
                    U_MGS_CL_NOMTIE = string.IsNullOrWhiteSpace(nombreTienda) ? tiendaCodigo : nombreTienda,
                    MGS_CL_VANTDETCollection = new[]
                    {
                        new
                        {
                            U_MGS_CL_GRPCOD = grupoCodigo ?? string.Empty,
                            U_MGS_CL_GRPNOM = string.IsNullOrWhiteSpace(nombreGrupo) ? (grupoCodigo ?? string.Empty) : nombreGrupo,
                            U_MGS_CL_TIPO = itemBase?.U_MGS_CL_TIPO ?? string.Empty,
                            U_MGS_CL_PORC = itemBase?.U_MGS_CL_PORC ?? 100,
                            U_MGS_CL_ACTIVO = "SI"
                        }
                    },
                    MGS_CL_VANTIADCollection = lista.Select(i => new
                    {
                        i.U_MGS_CL_GRPCOD,
                        i.U_MGS_CL_ITEMCOD,
                        i.U_MGS_CL_ITEMNAM,
                        i.U_MGS_CL_TIPO,
                        U_MGS_CL_PORC = i.U_MGS_CL_PORC ?? 0,
                        U_MGS_CL_ACTIVO = string.IsNullOrWhiteSpace(i.U_MGS_CL_ACTIVO) ? "SI" : i.U_MGS_CL_ACTIVO
                    })
                };

                var requestInformation = new RequestInformation
                {
                    Route = "MGS_CL_VANTCAB",
                    Token = login.Token,
                    Doc = JsonConvert.SerializeObject(createReq, settings)
                };

                return await PostInfo(requestInformation, "PYP", login.Cfg);
            }

            if (!grupoExiste)
            {
                var nombreGrupo = await ObtenerNombreGrupo(login.Cfg, grupoCodigo);
                var itemBase = lista.FirstOrDefault();
                var grupoReq = new
                {
                    MGS_CL_VANTDETCollection = new[]
                    {
                        new
                        {
                            U_MGS_CL_GRPCOD = grupoCodigo ?? string.Empty,
                            U_MGS_CL_GRPNOM = string.IsNullOrWhiteSpace(nombreGrupo) ? (grupoCodigo ?? string.Empty) : nombreGrupo,
                            U_MGS_CL_TIPO = itemBase?.U_MGS_CL_TIPO ?? string.Empty,
                            U_MGS_CL_PORC = itemBase?.U_MGS_CL_PORC ?? 100,
                            U_MGS_CL_ACTIVO = "SI"
                        }
                    }
                };

                var requestGrupo = new RequestInformation
                {
                    Route = $"MGS_CL_VANTCAB({existingDocEntry.Value})",
                    Token = login.Token,
                    Doc = JsonConvert.SerializeObject(grupoReq, settings)
                };

                var grupoResp = await UpdateInfo(requestGrupo, "PYP", login.Cfg);
                if (!grupoResp.Registered)
                {
                    return grupoResp;
                }
            }

            var updateReq = new
            {
                MGS_CL_VANTIADCollection = lista.Select(i => new
                {
                    i.LineId,
                    i.U_MGS_CL_GRPCOD,
                    i.U_MGS_CL_ITEMCOD,
                    i.U_MGS_CL_ITEMNAM,
                    i.U_MGS_CL_TIPO,
                    U_MGS_CL_PORC = i.U_MGS_CL_PORC ?? 0,
                    U_MGS_CL_ACTIVO = string.IsNullOrWhiteSpace(i.U_MGS_CL_ACTIVO) ? "SI" : i.U_MGS_CL_ACTIVO
                })
            };

            var requestInformationFinal = new RequestInformation
            {
                Route = $"MGS_CL_VANTCAB({existingDocEntry.Value})",
                Token = login.Token,
                Doc = JsonConvert.SerializeObject(updateReq, settings)
            };

            return await UpdateInfo(requestInformationFinal, "PYP", login.Cfg);
        }

        private static bool HasColumn(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<(bool Ok, ResponseInformation Error, string Token, EmpresaConfig Cfg)> LoginEmpresa(string empresa)
        {
            if (!TryGetEmpresaConfig(empresa, out var cfg, out var error))
            {
                return (false, error, string.Empty, null);
            }

            var token = await _loginService.Login(cfg);
            if (string.IsNullOrEmpty(token))
            {
                return (false, new ResponseInformation
                {
                    Registered = false,
                    Message = "No fue posible conectarse al Service Layer",
                    Content = string.Empty
                }, string.Empty, null);
            }

            return (true, null, token, cfg);
        }

        private async Task<string> ObtenerNombreTienda(EmpresaConfig cfg, string tiendaCodigo)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            try
            {
                await conn.OpenAsync();
                using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanTdaNom";
                cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
                cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
                cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    return reader["PrjName"]?.ToString();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener nombre de tienda ({tiendaCodigo}): {ex.Message}", ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }

        private async Task<int?> ObtenerDocEntryVanCab(EmpresaConfig cfg, string tiendaCodigo)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            await conn.OpenAsync();
            using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanCab";
            cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("DocEntry")))
            {
                return Convert.ToInt32(reader["DocEntry"]);
            }
            return null;
        }

        private async Task<List<VanArticuloDetalleDto>> ObtenerArticulosActivosPorGrupo(EmpresaConfig cfg, string tiendaCodigo, string grupoCodigo)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            await conn.OpenAsync();
            using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanGrpArt";
            cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = grupoCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            var articulos = new List<VanArticuloDetalleDto>();
            while (reader.Read())
            {
                articulos.Add(new VanArticuloDetalleDto
                {
                    DocEntry = reader.IsDBNull(reader.GetOrdinal("DocEntry")) ? (int?)null : Convert.ToInt32(reader["DocEntry"]),
                    LineId = reader.IsDBNull(reader.GetOrdinal("LineId")) ? 0 : Convert.ToInt32(reader["LineId"]),
                    U_MGS_CL_GRPCOD = reader["U_MGS_CL_GRPCOD"]?.ToString() ?? string.Empty,
                    U_MGS_CL_ITEMCOD = reader["U_MGS_CL_ITEMCOD"]?.ToString() ?? string.Empty,
                    U_MGS_CL_ITEMNAM = reader["U_MGS_CL_ITEMNAM"]?.ToString() ?? string.Empty,
                    U_MGS_CL_TIPO = HasColumn(reader, "U_MGS_CL_TIPO") ? reader["U_MGS_CL_TIPO"]?.ToString() ?? string.Empty : string.Empty,
                    U_MGS_CL_PORC = HasColumn(reader, "U_MGS_CL_PORC") && decimal.TryParse(reader["U_MGS_CL_PORC"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var porc) ? porc : (decimal?)null,
                    U_MGS_CL_ACTIVO = HasColumn(reader, "U_MGS_CL_ACTIVO") ? reader["U_MGS_CL_ACTIVO"]?.ToString() ?? string.Empty : string.Empty
                });
            }
            return articulos;
        }

        private async Task<(string GrupoCodigo, string GrupoNombre)> ObtenerGrupoArticuloActivo(EmpresaConfig cfg, string tiendaCodigo, string itemCode, string grupoExclusion)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            await conn.OpenAsync();
            using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanItemTienda";
            cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = itemCode ?? string.Empty;
            cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = grupoExclusion ?? string.Empty;
            cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return (reader["U_MGS_CL_GRPCOD"]?.ToString() ?? string.Empty, reader["U_MGS_CL_GRPNOM"]?.ToString() ?? string.Empty);
            }
            return (string.Empty, string.Empty);
        }

        private async Task<bool> ExisteGrupoVan(EmpresaConfig cfg, string tiendaCodigo, string grupoCodigo)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            await conn.OpenAsync();
            using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanGrpEx";
            cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = grupoCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                var count = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0 : Convert.ToInt32(reader["Total"]);
                return count > 0;
            }
            return false;
        }

        private async Task<(int? LineId, string Activo)> ObtenerGrupoVanDetalle(EmpresaConfig cfg, string tiendaCodigo, string grupoCodigo)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            await conn.OpenAsync();
            using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanGrpDet";
            cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = tiendaCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = grupoCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                var lineId = reader.IsDBNull(reader.GetOrdinal("LineId")) ? (int?)null : Convert.ToInt32(reader["LineId"]);
                var activo = reader["U_MGS_CL_ACTIVO"]?.ToString() ?? string.Empty;
                return (lineId, activo);
            }
            return (null, string.Empty);
        }

        private async Task<string> ObtenerNombreGrupo(EmpresaConfig cfg, string grupoCodigo)
        {
            using var conn = new HanaConnection(cfg.ConnectionString);
            await conn.OpenAsync();
            using var cmd = new HanaCommand("MGS_HDB_PE_SP_PORTALWEB", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@vTipo", HanaDbType.NVarChar, 20).Value = "Get_VanGrpNom";
            cmd.Parameters.Add("@vParam1", HanaDbType.NVarChar, 50).Value = grupoCodigo ?? string.Empty;
            cmd.Parameters.Add("@vParam2", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam3", HanaDbType.NVarChar, 50).Value = string.Empty;
            cmd.Parameters.Add("@vParam4", HanaDbType.NVarChar, 50).Value = string.Empty;
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return reader["Name"]?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

    }
}
    
