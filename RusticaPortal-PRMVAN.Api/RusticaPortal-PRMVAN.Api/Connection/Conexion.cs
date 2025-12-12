using ArellanoCore.Api.Entities.Information;
using ArellanoCore.Api.Entities.Login;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ArellanoCore.Api.Connection
{
    static class Conexion
    {
        
        public static SAPbobsCOM.Company oCompany;
        public static SAPbouiCOM.Application m_SBO_App;


        public static (bool, string) MainConnection(int docEntryOV, List<ListaDelete> lineas, LoginDIAPI lg)
        {
            string rs = "";
            bool rsb = true;
            try
            {
                Company company = new Company();
                company.Server = lg.Server; // "SK1@saphaarellano:30013";
                company.LicenseServer = lg.Licencia; // "saphaarellano:40000";
                company.DbServerType = BoDataServerTypes.dst_HANADB;
                company.CompanyDB = lg.Db;// "PRUEBAS_INTEGRALES_V2";
                company.UserName = lg.User;// "manager";
                company.Password = lg.Password;// "Main00$%";
                company.language = BoSuppLangs.ln_Spanish; // Adjust language as needed

                if (company.Connect() != 0)
                {
                    rs = "Connection to SAP Business One failed: " + company.GetLastErrorDescription();
                    rsb = false;
                    
                }
                
                Documents salesOrder = (Documents)company.GetBusinessObject(BoObjectTypes.oPurchaseRequest);
                if (salesOrder.GetByKey(docEntryOV)) // Replace 123 with your Sales Order's DocEntry
                {
                    int i = 0;
                    foreach (ListaDelete line in lineas)                    {
                        salesOrder.Lines.SetCurrentLine(line.item_del_Presupuesto); // Replace 1 with the line number you want to remove
                        ////salesOrder.Lines.Delete();                      

                        if (salesOrder.Lines.LineStatus == SAPbobsCOM.BoStatus.bost_Open && salesOrder.Lines.ItemCode == line.catCost_Code)
                        {
                            // Cerrar la línea actual
                            salesOrder.Lines.LineStatus = SAPbobsCOM.BoStatus.bost_Close ;
                            salesOrder.Lines.UserFields.Fields.Item("U_MGS_CL_ESTADO").Value = "01";

                            // Si solo quieres cerrar una línea específica, puedes agregar un condicional para asegurarte que es la correcta.
                            // Por ejemplo, verificar el código de artículo:
                            // if (salesOrder.Lines.ItemCode == "CODIGO_ARTICULO") { /*Cerrar la línea*/ }
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        int result = salesOrder.Update();
                        if (result != 0)
                        {
                            rsb = false;
                            rs = company.GetLastErrorDescription().ToString();
                        }
                        else
                        {
                            rsb = true;
                            rs = "El documento se actualizó correctamente";
                        }
                    }
                    else {
                        rsb = false;
                        rs = "No se encontraron documentos coincidentes para eliminar";
                    }
                    
                }
                else
                {
                    rsb = false;
                    rs = "Documento no encontrado";
                }

                company.Disconnect();
            }catch(Exception ex)
            {
                rsb = false;
                rs = "Error: " + ex.Message.ToString();
            }

            return (rsb, rs);
            
        }

       
    }
}
